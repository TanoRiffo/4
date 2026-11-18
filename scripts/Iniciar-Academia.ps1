$ErrorActionPreference = "Stop"

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$projectFile = Join-Path $projectRoot "AcademiaCentralApi.csproj"
if (-not (Test-Path -LiteralPath $projectFile)) {
    throw "No se encontro AcademiaCentralApi.csproj en $projectRoot."
}
if ([string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) {
    throw "No se encontro LOCALAPPDATA; este script esta preparado para Windows."
}

$databasePath = [System.IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA "AcademiaCentralTutorial.db"))
$swaggerUrl = "http://localhost:5111/swagger"
$previousEnvironment = [Environment]::GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Process")
$previousConnection = [Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Process")

function Get-UserSecrets {
    $lines = & dotnet user-secrets list 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudieron leer los User Secrets del proyecto."
    }

    $secrets = @{}
    foreach ($line in $lines) {
        $separator = $line.IndexOf("=")
        if ($separator -gt 0) {
            $name = $line.Substring(0, $separator).Trim()
            $value = $line.Substring($separator + 1).Trim()
            if (-not [string]::IsNullOrWhiteSpace($name)) {
                $secrets[$name] = $value
            }
        }
    }

    return ,$secrets
}

function Set-UserSecret {
    param(
        [string]$Name,
        [string]$Value
    )

    $null = & dotnet user-secrets set $Name $Value
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo guardar el secreto '$Name'."
    }
    $script:UserSecrets[$Name] = $Value
}

function ConvertFrom-SecureStringValue {
    param([System.Security.SecureString]$Value)

    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
}

function Read-RequiredSecret {
    param([string]$Prompt)

    while ($true) {
        $secureValue = Read-Host -Prompt $Prompt -AsSecureString
        $value = ConvertFrom-SecureStringValue $secureValue
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }
        Write-Warning "El valor no puede quedar vacio."
    }
}

function Read-AdminPassword {
    while ($true) {
        $password = Read-RequiredSecret "Contrasena de Admin (minimo 8 caracteres)"
        if ($password.Length -lt 8) {
            Write-Warning "La contrasena debe tener al menos 8 caracteres."
            continue
        }

        $confirmation = Read-RequiredSecret "Repite la contrasena de Admin"
        if ($password -ceq $confirmation) {
            return $password
        }
        Write-Warning "Las contrasenas no coinciden."
    }
}

function New-RandomJwtKey {
    $bytes = New-Object byte[] 32
    $generator = [Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $generator.GetBytes($bytes)
        return [Convert]::ToBase64String($bytes)
    }
    finally {
        $generator.Dispose()
    }
}

function Ensure-Configuration {
    param(
        [switch]$ReplaceAdmin
    )

    $jwtKey = [string]$script:UserSecrets["Jwt:Key"]
    if ([string]::IsNullOrWhiteSpace($jwtKey) -or
        [Text.Encoding]::UTF8.GetByteCount($jwtKey) -lt 32) {
        Set-UserSecret "Jwt:Key" (New-RandomJwtKey)
    }

    if ([string]::IsNullOrWhiteSpace([string]$script:UserSecrets["BiblioLink:ApiKey"])) {
        Set-UserSecret "BiblioLink:ApiKey" (Read-RequiredSecret "Clave que BiblioLink enviara a Academia")
    }
    if ([string]::IsNullOrWhiteSpace([string]$script:UserSecrets["BiblioLink:OutboundApiKey"])) {
        Set-UserSecret "BiblioLink:OutboundApiKey" (Read-RequiredSecret "Clave que Academia enviara a BiblioLink")
    }

    if ($ReplaceAdmin -or [string]::IsNullOrWhiteSpace([string]$script:UserSecrets["BootstrapAdmin:NombreUsuario"])) {
        $username = Read-Host "Nombre de usuario Admin [admin]"
        if ([string]::IsNullOrWhiteSpace($username)) {
            $username = "admin"
        }
        Set-UserSecret "BootstrapAdmin:NombreUsuario" $username.Trim()
    }
    if ($ReplaceAdmin -or [string]::IsNullOrWhiteSpace([string]$script:UserSecrets["BootstrapAdmin:Contrasena"])) {
        Set-UserSecret "BootstrapAdmin:Contrasena" (Read-AdminPassword)
    }
}

function Remove-TestDatabase {
    foreach ($suffix in @("", "-wal", "-shm", "-journal")) {
        $file = $databasePath + $suffix
        if (Test-Path -LiteralPath $file -PathType Leaf) {
            Remove-Item -LiteralPath $file -Force
        }
    }
}

function Test-ApiPortInUse {
    $connections = @(Get-NetTCPConnection -LocalPort 5111 -State Listen -ErrorAction SilentlyContinue)
    return $connections.Count -gt 0
}

Push-Location $projectRoot
try {
    $sdkVersions = & dotnet --list-sdks
    $hasNet10 = $false
    foreach ($version in $sdkVersions) {
        if ($version -match "^10\.") {
            $hasNet10 = $true
        }
    }
    if (-not $hasNet10) {
        throw "Se requiere .NET SDK 10."
    }

    $null = & dotnet ef --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "No se encontro dotnet-ef. Instalalo con: dotnet tool install --global dotnet-ef --version 10.0.12"
    }

    if (Test-ApiPortInUse) {
        Write-Warning "El puerto 5111 ya esta en uso. No se inicio la API ni se modifico ninguna base. Deten la instancia antes de reiniciar datos."
        Start-Process $swaggerUrl
        return
    }

    $script:UserSecrets = Get-UserSecrets
    $resetMode = "none"
    if (Test-Path -LiteralPath $databasePath -PathType Leaf) {
        $resetAnswer = Read-Host "Ya existe la base de prueba. Quieres reiniciarla? [s/N]"
        if ($resetAnswer -match "^(s|si)$") {
            Write-Host "1. Reinicio seguro: borra los datos, conserva las claves de BiblioLink y renueva JWT/Admin."
            Write-Host "2. Borrado total: borra la base y todos los User Secrets del proyecto."
            Write-Host "3. Cancelar."
            $resetChoice = Read-Host "Elige una opcion [1-3]"

            if ($resetChoice -eq "1") {
                $resetMode = "safe"
            }
            elseif ($resetChoice -eq "2") {
                Write-Warning "Se borraran todos los User Secrets asociados a este UserSecretsId. Esto puede afectar otras copias del proyecto y romper la integracion hasta volver a configurar las claves."
                $confirmation = Read-Host "Escribe BORRAR TODO para continuar"
                if ($confirmation -cne "BORRAR TODO") {
                    Write-Host "Borrado total cancelado."
                    return
                }
                $resetMode = "full"
            }
            else {
                Write-Host "Reinicio cancelado."
                return
            }
        }
    }

    if ($resetMode -eq "safe") {
        Remove-TestDatabase
        Set-UserSecret "Jwt:Key" (New-RandomJwtKey)
        $script:UserSecrets["BootstrapAdmin:NombreUsuario"] = ""
        $script:UserSecrets["BootstrapAdmin:Contrasena"] = ""
        Ensure-Configuration -ReplaceAdmin
        Write-Host "Base de prueba reiniciada. Se conservaron las claves de BiblioLink."
    }
    elseif ($resetMode -eq "full") {
        Remove-TestDatabase
        $null = & dotnet user-secrets clear
        if ($LASTEXITCODE -ne 0) {
            throw "No se pudieron borrar los User Secrets."
        }
        $script:UserSecrets = @{}
        Ensure-Configuration -ReplaceAdmin
        Write-Host "Base de prueba y User Secrets reiniciados."
    }
    else {
        Ensure-Configuration
    }

    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ConnectionStrings__DefaultConnection = "Data Source=$databasePath"

    & dotnet ef database update
    if ($LASTEXITCODE -ne 0) {
        throw "Fallo la actualizacion de la base de prueba."
    }

    Write-Host "Iniciando Academia Central en http://localhost:5111. Swagger: $swaggerUrl"
    Write-Host "Presiona Ctrl+C para detener la API."
    & dotnet run --launch-profile http
    if ($LASTEXITCODE -ne 0) {
        throw "La API termino con un error."
    }
}
finally {
    Pop-Location
    [Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", $previousEnvironment, "Process")
    [Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $previousConnection, "Process")
}
