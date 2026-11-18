# Contrato de integración entre BiblioLink y Academia Central

BiblioLink · Programación 2 (UAP) · Documento compartido del equipo

BiblioLink consulta a Academia para validar alumnos antes de prestar libros. Academia consulta a BiblioLink para comprobar préstamos antes de matricular alumnos. Este documento describe las rutas, respuestas y responsabilidades vigentes.

## 1. Resumen

| Sentido | Pregunta | Ruta | Estado |
|---|---|---|---|
| BiblioLink → Academia | ¿El alumno está activo? | `GET /api/alumnos/{matricula}/estado` | Academia implementa la consulta, valida `X-API-Key` y devuelve el estado. Falta integrar el cliente en BiblioLink. |
| Academia → BiblioLink | ¿El alumno tiene préstamos activos? | `GET /api/prestamos/activos/{matricula}` | Falta implementar esta ruta y el cliente real en BiblioLink. Academia ya envía la consulta. |

En desarrollo local, Academia escucha en `http://localhost:5111` y BiblioLink en `http://localhost:5245`. Las URL base de las APIs incluyen `/api`: `http://localhost:5111/api` y `http://localhost:5245/api`. Academia lee la URL base de BiblioLink desde `ConfiguracionesApiBiblioteca.UrlBase`; esa fila se carga mediante datos iniciales.

## 2. BiblioLink consulta a Academia

**Regla:** un Alumno puede pedir un préstamo solo si Academia responde `esActivo: true`. La consulta a Academia es la última validación del `POST /api/prestamos`. El Personal se considera elegible sin consultar; esta simplificación está pendiente de confirmación con el profesor.

| Elemento | Valor |
|---|---|
| Método y URL | `GET {UrlBase}/alumnos/{matricula}/estado`, con `UrlBase` = `http://localhost:5111/api` |
| `matricula` | `Usuario.Matricula` de BiblioLink = `Alumno.MatriculaUniversitaria` de Academia |
| Header | `X-API-Key: {ApiKey}`; Academia valida la clave configurada como `BiblioLink:ApiKey` |
| JWT | No requerido |
| Timeout | 5 segundos, responsabilidad del cliente de BiblioLink |

Respuesta `200 OK`:

```json
{ "matricula": "A1001", "esActivo": true }
```

Respuesta `404 Not Found` si la matrícula no existe:

```json
{ "mensaje": "Matrícula no encontrada" }
```

| Situación | Respuesta de Academia | Resultado esperado en `POST /api/prestamos` |
|---|---|---|
| Alumno activo | `200` con `esActivo: true` | Sigue el flujo del préstamo |
| Alumno no activo | `200` con `esActivo: false` | `403`, usuario no elegible |
| Matrícula inexistente | `404` con campo `mensaje` | `403`, usuario no elegible |
| API key ausente o incorrecta | `401` | `502 Bad Gateway` |
| Error interno, respuesta inválida o `404` sin `mensaje` | Error o cuerpo inválido | `502 Bad Gateway` |
| Academia caída | Sin conexión | `503 Service Unavailable` |
| Más de 5 segundos sin respuesta | Timeout | `504 Gateway Timeout` |

Un fallo de Academia nunca debe convertirse en `500` de BiblioLink; ese código queda para errores propios de BiblioLink.

## 3. Academia consulta a BiblioLink

**Regla:** Academia no matricula a un alumno que tiene préstamos activos. Si BiblioLink no responde o devuelve una respuesta no exitosa, Academia bloquea la matrícula por precaución.

| Elemento | Valor |
|---|---|
| Método y URL | `GET {UrlBase}/prestamos/activos/{matricula}`, con `UrlBase` = `http://localhost:5245/api` |
| Header | `X-API-Key: {ApiKey}`, tomado de `BiblioLink:OutboundApiKey` en Academia |
| JWT | No aplica entre servicios |

`BiblioLink:OutboundApiKey` es independiente de `BiblioLink:ApiKey`: la primera es la clave que Academia envía a BiblioLink; la segunda es la que BiblioLink envía a Academia. Los valores reales se configuran como secretos y nunca se guardan en el repositorio, logs o respuestas. BiblioLink todavía debe implementar la validación de la clave que recibe de Academia.

Respuesta `200 OK`, siempre una lista:

```json
[
  { "prestamoId": 12, "libroTitulo": "El Aleph", "estado": "Vigente" },
  { "prestamoId": 15, "libroTitulo": "Rayuela", "estado": "Vencido" }
]
```

| Campo | Tipo | Valores |
|---|---|---|
| `prestamoId` | número | Id del préstamo en BiblioLink |
| `libroTitulo` | texto | Título del libro |
| `estado` | texto | `Vigente` o `Vencido`; este último se calcula si pasó la fecha prevista de devolución |

Si la matrícula no tiene préstamos activos, o BiblioLink no la conoce, responde `200` con `[]`, nunca `404`. Academia trata una respuesta no exitosa o un error de comunicación como préstamos pendientes y `POST /api/matriculas` responde `403`.

Academia también expone `GET /api/alumnos/{id}/loans`, que reenvía el JSON de préstamos. Requiere JWT y está disponible para los roles `Admin` y `Personal`; el rol `Personal` no habilita operaciones administrativas.

## 4. Pendientes de cada equipo

| Equipo | Tarea |
|---|---|
| BiblioLink | Crear `GET /api/prestamos/activos/{matricula}` según la sección 3; devolver `200` con `[]` cuando no haya préstamos o no conozca la matrícula |
| BiblioLink | Crear el `UniversidadClient` real (`IUniversidadClient.EsElegibleAsync`) según la sección 2 y reemplazar `UniversidadClientFalso` |
| BiblioLink | Registrar `IPrestamoService`, `IUniversidadClient` y `PrestamosOptions` en `Program.cs`, y agregar la sección `Prestamos` en `appsettings.json` |
| BiblioLink | Crear `ApiExterna` y cargar la URL base de Academia |
| BiblioLink | Enviar la clave configurada por el equipo en el header `X-API-Key` al consultar el estado del alumno en Academia |
| BiblioLink | Acordar e implementar la validación de la clave recibida en las llamadas desde Academia |
| Academia | Hecho: determinar el estado del alumno según una matrícula `Vigente` cuya `FechaFinVigencia` sea hoy o posterior |
| Academia | Hecho: cargar `ConfiguracionesApiBiblioteca` con la URL base `http://localhost:5245/api` |
| Academia | Hecho: exigir `X-API-Key` en la validación de estado y usar claves entrantes y salientes separadas |
| Academia | Hecho: cargar datos de prueba con un alumno activo y otro con matrícula finalizada |
| Ambos | Probar las dos direcciones de la integración con matrículas válidas, inexistentes, claves incorrectas y fallos de comunicación |

## 5. Decisiones abiertas

- **Personal:** se considera elegible sin consultar a Academia. Falta confirmarlo con el profesor, porque el TP pide validar también “Personal Vigente”.
- **Qué bloquea la matriculación:** Academia bloquea cualquier lista no vacía. Falta decidir si debe bloquear solo los préstamos vencidos.
- **Validación de API key:** Academia ya valida la clave entrante de BiblioLink. BiblioLink todavía no valida la clave entrante de Academia; ambos equipos deben acordar y configurar la clave aceptada por cada servicio.

## 6. Acuerdos internos de BiblioLink

- **Stock:** el servicio de préstamos modifica `Libro.CantidadDisponible` con el mismo `DbContext` y lo guarda en el mismo `SaveChangesAsync` que crea o cierra el préstamo, para que la operación sea atómica.
- **Excepciones compartidas** (carpeta `Exceptions/`): `NotFoundException` (404), `ReglaNegocioException` (409), `NoElegibleException` (403) y `ServicioExternoException` con su `StatusCode` (502, 503 o 504).
- **Estado `Vencido`:** no se guarda, se calcula.
- **Seguridad en logs:** la `ApiKey` nunca se escribe en logs ni se devuelve en una respuesta.
- **Nombres comunes:** contexto `BibliotecaContext`, namespaces `BiblioLink.*`, un `XConfiguration` por entidad en `Data/Configurations`.

## 7. Cambios al contrato

Quien necesite agregar un campo, un valor o un código de respuesta lo propone a los dos equipos, actualiza este documento y recién después modifica el código, para evitar diferencias al integrar.
