# Academia Central API

API REST de Academia Central para consultar y administrar alumnos, carreras, matrículas y la integración con BiblioLink. Usa ASP.NET Core, SQLite y JWT para proteger las operaciones administrativas.

## Requisitos

- .NET SDK 10
- Entity Framework Core CLI (`dotnet tool install --global dotnet-ef --version 10.0.12`, si aún no está instalado)

## Tutorial: primer inicio y consulta como Admin

### 1. Clonar y ejecutar el script

Desde PowerShell:

```powershell
git clone https://github.com/TanoRiffo/4.git
Set-Location .\4
```

Si todavía no tienes Entity Framework CLI, instálalo una vez:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

Inicia la configuración y la API con:

```powershell
.\scripts\Iniciar-Academia.ps1
```

Si Windows bloquea la ejecución del script, puedes habilitar scripts solo en esa ventana de PowerShell y volver a ejecutarlo:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\Iniciar-Academia.ps1
```

En el primer uso, el script genera la clave JWT y pide los secretos que falten, junto con el usuario y contraseña iniciales del Admin. No muestra las claves al escribirlas; la contraseña se solicita dos veces. Aplica las migraciones, carga los datos de prueba y levanta la API. Usa una base separada en `%LOCALAPPDATA%`, así que no modifica `AcademiaCentral.db`.

Si vuelves a ejecutar el script con la API detenida y la base existente, pregunta si quieres reiniciar:

- **Reinicio seguro:** borra solo la base de prueba, conserva las claves de BiblioLink, renueva la clave JWT y pide credenciales nuevas para el Admin.
- **Borrado total:** borra la base y todos los User Secrets del proyecto. Requiere escribir `BORRAR TODO`; puede afectar otras copias del repo que comparten `UserSecretsId` y obliga a configurar nuevamente las claves de integración.

Si el puerto `5111` ya está en uso, el script no borra datos ni inicia otra instancia. Detén la API con `Ctrl+C` antes de reiniciar.

### 2. Iniciar sesión y consultar una ruta protegida

Abre [Swagger](http://localhost:5111/swagger):

1. Ejecuta `POST /api/auth/login` con el usuario y contraseña iniciales:

   ```json
   {
     "nombreUsuario": "admin",
     "contrasena": "<la-contraseña-configurada>"
   }
   ```

2. Copia el valor `token` de la respuesta, pulsa **Authorize** y pega el token.
3. Ejecuta `GET /api/alumnos`, una ruta que requiere el rol Admin. Debe responder `200 OK` con los alumnos de prueba.

Sin token, `GET /api/alumnos` responde `401`; con un token de un usuario que no sea Admin, responde `403`.

## Autenticación

Inicia sesión con `POST /api/auth/login`:

```json
{
  "nombreUsuario": "admin",
  "contrasena": "<contraseña>"
}
```

La respuesta contiene un token JWT. En Swagger, pulsa **Authorize**, pega el token y úsalo para las operaciones que lo requieren. También puedes enviarlo desde otro cliente HTTP como `Authorization: Bearer <token>`.

Solo Admin puede crear usuarios con `POST /api/auth/usuarios`. Ejemplo de cuerpo:

```json
{
  "nombreUsuario": "alumno1",
  "contrasena": "<contraseña-de-al-menos-8-caracteres>",
  "rol": "Alumno",
  "alumnoId": 9001
}
```

`alumnoId` es opcional; si se proporciona, debe corresponder a un alumno existente. Las contraseñas se almacenan como hashes y no se devuelven en las respuestas.

## Acceso por rol

- Los endpoints de alumnos y carreras requieren autenticación. Admin puede listar y modificar; Alumno solo puede consultar su propio alumno y sus matrículas.
- Los cambios de matrículas (`POST`, `PUT`, `DELETE`) requieren Admin.
- `GET /api/alumnos/{id}/loans` está disponible para Admin y Personal; Personal solo puede consultar préstamos.
- Para comprobarlo en Swagger, inicia sesión como Admin, copia el token en **Authorize** y crea un usuario Alumno asociado a un alumno existente. Luego inicia sesión con ese Alumno y verifica que `GET /api/alumnos/{id}` y `GET /api/matriculas` solo permitan consultar sus propios datos. Sin token, las rutas protegidas responden `401`; con rol insuficiente, `403`.
- Si la biblioteca rechaza una matrícula por préstamos pendientes, la API conserva la respuesta `403`.

## Rutas principales

- `/api/auth`: inicio de sesión y administración de usuarios.
- `/api/alumnos`: consulta y gestión de alumnos; incluye la validación de estado y el proxy de préstamos.
- `/api/carreras`: consulta y gestión de carreras.
- `/api/matriculas`: consulta y gestión de matrículas.

## Integración con BiblioLink

En desarrollo local, Academia Central usa `http://localhost:5111/api` y BiblioLink usa `http://localhost:5245/api`. Academia obtiene la URL base de BiblioLink del campo `UrlBase` en `ConfiguracionesApiBiblioteca`.

El contrato de rutas, headers, respuestas y tareas pendientes está en [CONTRATO_INTEGRACION.md](./CONTRATO_INTEGRACION.md).

- **BiblioLink consulta a Academia:** `GET /api/alumnos/{matricula}/estado`. No requiere JWT; requiere `X-API-Key` con el valor configurado en `BiblioLink:ApiKey`. Responde `200` con `{ "matricula": "A1001", "esActivo": true }` (o `false`), `404` con `{ "mensaje": "Matrícula no encontrada" }` si la matrícula no existe y `401` si falta la clave o no coincide.
- **Academia consulta a BiblioLink:** `GET {UrlBase}/prestamos/activos/{matricula}`. Academia envía `X-API-Key` con `BiblioLink:OutboundApiKey`. BiblioLink debe responder `200` con una lista de préstamos (`prestamoId`, `libroTitulo`, `estado`); si no hay préstamos, la lista es `[]`. Una lista con préstamos, una respuesta no exitosa o un fallo de comunicación impiden crear la matrícula y producen `403`.
- **Consulta de préstamos desde Academia:** `GET /api/alumnos/{id}/loans` requiere JWT y está disponible para los roles Admin y Personal. Personal puede consultar préstamos, pero no realizar operaciones administrativas.
