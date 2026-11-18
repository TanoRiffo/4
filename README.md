# Academia Central API

API REST de Academia Central para consultar y administrar alumnos, carreras, matrículas y la integración con BiblioLink. Usa ASP.NET Core, SQLite y JWT para proteger las operaciones administrativas.

## Requisitos

- .NET SDK 10
- Entity Framework Core CLI (`dotnet tool install --global dotnet-ef`, si aún no está instalado)

## Configuración y ejecución

Configura la clave de firma JWT y las credenciales temporales del primer administrador con User Secrets. Usa una clave aleatoria de al menos 32 bytes y una contraseña segura:

```powershell
dotnet user-secrets set "Jwt:Key" "<clave-aleatoria-de-al-menos-32-bytes>"
dotnet user-secrets set "BiblioLink:ApiKey" "<clave-compartida-con-BiblioLink>"
dotnet user-secrets set "BiblioLink:OutboundApiKey" "<clave-que-Academia-envia-a-BiblioLink>"
dotnet user-secrets set "BootstrapAdmin:NombreUsuario" "admin"
dotnet user-secrets set "BootstrapAdmin:Contrasena" "<contraseña-segura>"
```

La aplicación crea ese Admin al iniciar solamente cuando la tabla de usuarios está vacía. Después del primer inicio, las claves `BootstrapAdmin` se pueden quitar; `Jwt:Key`, `BiblioLink:ApiKey` y `BiblioLink:OutboundApiKey` deben permanecer configuradas. `BiblioLink:ApiKey` valida las llamadas entrantes de BiblioLink a Academia; `BiblioLink:OutboundApiKey` se envía en las llamadas de Academia a BiblioLink. No guardes claves reales en `appsettings.json` ni las registres en logs.

Aplica las migraciones y ejecuta la API:

```powershell
dotnet ef database update
dotnet run --launch-profile http
```

La documentación interactiva de Swagger está disponible en [http://localhost:5111/swagger](http://localhost:5111/swagger).

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
- Para comprobarlo en Swagger, inicia sesión como Admin, copia el token en **Authorize** y crea un usuario Alumno asociado a un alumno existente. Luego inicia sesión con ese Alumno y verifica que `GET /api/alumnos/{id}` y `GET /api/matriculas` solo permitan consultar sus propios datos. Sin token, las rutas protegidas responden `401`; con rol insuficiente, `403`.
- Si la biblioteca rechaza una matrícula por préstamos pendientes, la API conserva la respuesta `403`.

## Rutas principales

- `/api/auth`: inicio de sesión y administración de usuarios.
- `/api/alumnos`: consulta y gestión de alumnos; incluye la validación de estado y el proxy de préstamos.
- `/api/carreras`: consulta y gestión de carreras.
- `/api/matriculas`: consulta y gestión de matrículas.

La validación `GET /api/alumnos/{matricula}/estado` no requiere JWT, pero sí el header `X-API-Key` con el valor configurado en `BiblioLink:ApiKey`. Si falta o no coincide, responde `401`.
