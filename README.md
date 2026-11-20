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

### ¿Para qué sirve cada dato que solicita?

El script reutiliza los secretos que ya estén configurados; solo pregunta por los que falten o cuando eliges un reinicio que los renueva:

| Dato | Para qué se usa |
|---|---|
| `BiblioLink:ApiKey` | Clave que BiblioLink envía a Academia en `X-API-Key` para consultar el estado de un alumno. |
| `BiblioLink:OutboundApiKey` | Clave que Academia envía a BiblioLink en `X-API-Key` al consultar sus préstamos. Debe coincidir con la que BiblioLink acepte. |
| Usuario Admin | Identifica la cuenta inicial creada si la base no tiene usuarios. |
| Contraseña y confirmación | Permiten iniciar sesión como Admin; se confirma para evitar errores al escribirla y debe tener al menos 8 caracteres. |
| Clave JWT | El script la genera automáticamente si falta o no es válida. Firma los tokens de inicio de sesión; no se comparte con BiblioLink. |

Las claves API se ingresan de forma oculta y se guardan en User Secrets. Si BiblioLink todavía no está disponible, puedes usar claves locales para levantar y probar Academia; tendrás que coordinar valores coincidentes al integrar ambos sistemas.

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

### 3. Crear una cuenta y probar como Alumno

Las migraciones cargan alumnos y matrículas de prueba, pero no crean cuentas de inicio de sesión para esos alumnos. Con el token Admin autorizado en Swagger, crea una cuenta asociada al alumno activo:

`POST /api/auth/usuarios`

```json
{
  "nombreUsuario": "alumno.prueba",
  "contrasena": "<elige-una-contrasena-de-al-menos-8-caracteres>",
  "rol": "Alumno",
  "alumnoId": 9001
}
```

Después inicia sesión en `POST /api/auth/login` con esas credenciales y reemplaza en **Authorize** el token Admin por el token de Alumno. Prueba:

- `GET /api/alumnos/9001` → `200 OK`, consulta su propio perfil.
- `GET /api/matriculas` → `200 OK`, devuelve solo las matrículas de ese alumno.
- `GET /api/alumnos/9002` → `403 Forbidden`, no puede consultar el perfil de otro alumno.

### Agregar otro alumno de prueba (opcional)

Con el token Admin, ejecuta `POST /api/alumnos`:

```json
{
  "nombre": "Alumno",
  "apellido": "Prueba adicional",
  "matriculaUniversitaria": "TEST-EXTRA-9003",
  "email": "alumno.extra@example.invalid",
  "dni": "TEST-DNI-9003"
}
```

La respuesta incluye el `id` asignado. Usa ese valor como `alumnoId` al crearle una cuenta con `POST /api/auth/usuarios`. Crear el alumno no crea automáticamente una cuenta de acceso ni una matrícula.

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

La base de prueba de Academia incluye `TEST-ACTIVO-9001` (alumno ID `9001`, activo) y `TEST-INACTIVO-9002` (ID `9002`, matrícula finalizada). Esos registros no crean usuarios en BiblioLink ni afectan su base. Las migraciones los cargan en cualquier base de Academia que las aplique; son datos para desarrollo, no para producción. Si BiblioLink no está levantada, consultar el perfil o las matrículas ya existentes funciona, pero `POST /api/matriculas` bloquea por precaución y devuelve `403`.

Para cruzar los casos, el otro equipo debe crear en su propia base usuarios de BiblioLink cuya `Usuario.Matricula` coincida exactamente con `TEST-ACTIVO-9001` y `TEST-INACTIVO-9002`. El usuario activo sin préstamos debe producir `[]`; al agregarle un préstamo, Academia debe bloquear la matrícula con `403`. Para `TEST-NO-EXISTE`, crea un usuario solo en BiblioLink y ninguno en Academia; así se verifica el `404` del chequeo de estado.

En desarrollo local, Academia Central usa `http://localhost:5111/api` y BiblioLink usa `http://localhost:5245/api`. Academia obtiene la URL base de BiblioLink del campo `UrlBase` en `ConfiguracionesApiBiblioteca`.

El contrato de rutas, headers, respuestas y tareas pendientes está en [CONTRATO_INTEGRACION.md](./CONTRATO_INTEGRACION.md).

- **BiblioLink consulta a Academia:** `GET /api/alumnos/{matricula}/estado`. No requiere JWT; requiere `X-API-Key` con el valor configurado en `BiblioLink:ApiKey`. Responde `200` con `{ "matricula": "A1001", "esActivo": true }` (o `false`), `404` con `{ "mensaje": "Matrícula no encontrada" }` si la matrícula no existe y `401` si falta la clave o no coincide.
- **Academia consulta a BiblioLink:** `GET {UrlBase}/prestamos/activos/{matricula}`. Academia envía `X-API-Key` con `BiblioLink:OutboundApiKey`. BiblioLink debe responder `200` con una lista de préstamos (`prestamoId`, `libroTitulo`, `estado`); si no hay préstamos, la lista es `[]`. Una lista con préstamos, una respuesta no exitosa o un fallo de comunicación impiden crear la matrícula y producen `403`.
- **Consulta de préstamos desde Academia:** `GET /api/alumnos/{id}/loans` requiere JWT y está disponible para los roles Admin y Personal. Personal puede consultar préstamos, pero no realizar operaciones administrativas.


### Caso de integración: alumno con un préstamo pendiente

Este caso prueba la regla central de la integración de punta a punta: **Academia no matricula a un alumno que tiene préstamos activos en BiblioLink**. Se necesitan las dos APIs levantadas al mismo tiempo.

**Antes de empezar**

- Academia corre en `http://localhost:5111` y BiblioLink en `http://localhost:5245`.
- `BiblioLink:OutboundApiKey` de Academia coincide con la clave que BiblioLink acepta, y `BiblioLink:ApiKey` de Academia coincide con la que BiblioLink envía.
- La fila de `ConfiguracionesApiBiblioteca` apunta a BiblioLink. Si en la clase BiblioLink corre en otro puerto o en otro equipo, actualiza `UrlBase` en esa fila de la base de Academia; el valor sembrado es `http://localhost:5245/api`.
- Tienes el token Admin autorizado en Swagger (ver el tutorial de arriba).

**Paso 1. Crear el alumno en Academia**

`POST /api/alumnos`

```json
{
  "nombre": "Alumno",
  "apellido": "Con préstamo",
  "matriculaUniversitaria": "TEST-PRESTAMO-9004",
  "email": "alumno.prestamo@example.invalid",
  "dni": "TEST-DNI-9004"
}
```

Anota el `id` de la respuesta. Se usa como `alumnoId` en los pasos siguientes.

**Paso 2. Matricularlo por primera vez (todavía sin préstamos)**

BiblioLink solo presta libros a alumnos activos, y en Academia un alumno es activo cuando tiene una matrícula `Vigente`. Por eso primero se matricula:

`POST /api/matriculas`

```json
{
  "alumnoId": 9003,
  "carreraId": 9001,
  "periodoAcademico": "2026-2"
}
```

Respuesta esperada: `201 Created`. (Reemplaza `9003` por el `id` del paso 1.)

**Paso 3. Registrar el préstamo en BiblioLink**

En BiblioLink, crea un usuario con `Usuario.Matricula = TEST-PRESTAMO-9004` y registra un préstamo para él (`POST /api/prestamos` en BiblioLink). BiblioLink consulta a Academia `GET /api/alumnos/TEST-PRESTAMO-9004/estado`, recibe `esActivo: true` y permite el préstamo.

**Paso 4. Ver el préstamo desde Academia**

`GET /api/alumnos/{id}/loans` (token Admin o Personal). Debe devolver el préstamo que reporta BiblioLink:

```json
[
  { "prestamoId": 12, "libroTitulo": "El Aleph", "estado": "Vigente" }
]
```

**Paso 5. Intentar matricularlo en otro período**

Se usa un período distinto porque, con el mismo período, Academia respondería antes que ya existe una matrícula vigente (`400`) y no llegaría a consultar la biblioteca.

`POST /api/matriculas`

```json
{
  "alumnoId": 9003,
  "carreraId": 9001,
  "periodoAcademico": "2027-1"
}
```

Respuesta esperada: **`403 Forbidden`**

```json
{
  "error": "Acceso Denegado",
  "mensaje": "Debe devolver los libros pendientes o en mora antes de matricularse."
}
```

**Paso 6. Devolver el libro y reintentar**

Registra la devolución en BiblioLink. Con el préstamo cerrado, BiblioLink responde `[]` y repetir el `POST` del paso 5 devuelve `201 Created`.

**Resultado esperado**

| Situación en BiblioLink | `POST /api/matriculas` en Academia |
|---|---|
| Alumno sin préstamos, o matrícula desconocida (`[]`) | `201 Created` |
| Alumno con préstamo `Vigente` o `Vencido` | `403` |
| BiblioLink caída, sin respuesta o con error | `403`, por precaución |
| Alumno o carrera inexistente | `400` |
| Alumno con matrícula vigente en el mismo período | `400` |

> Si `GET /api/alumnos/{id}/loans` devuelve `[]` pero esperabas un préstamo, revisa que `Usuario.Matricula` en BiblioLink coincida exactamente con `MatriculaUniversitaria` y que las claves API sean las correctas. Esa ruta devuelve `[]` también cuando BiblioLink falla, así que una lista vacía no distingue "sin préstamos" de "error de comunicación".
