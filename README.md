# UsersApi

[![CI](https://github.com/MurenDev/UsersApi/actions/workflows/ci.yml/badge.svg)](https://github.com/MurenDev/UsersApi/actions/workflows/ci.yml)

API REST hecha con .NET Minimal API para la prueba técnica. Tiene CRUD de usuarios, direcciones por usuario y un módulo de conversión de divisas.

## Tecnologías

- .NET 10 (Minimal API)
- Entity Framework Core 10 con SQLite
- FluentValidation
- Swagger (Swashbuckle)
- BCrypt.Net para el hash del password
- CQRS con commands, queries y handlers propios (sin MediatR)

## Cómo correr el proyecto

Requisito: tener instalado el SDK de .NET 10.

```bash
git clone https://github.com/MurenDev/UsersApi.git
cd UsersApi/UsersApi
dotnet run
```

La API queda en `http://localhost:5163` y Swagger en `http://localhost:5163/swagger`.

## Base de datos

Se usa SQLite y el archivo `app.db` se crea solo al levantar el proyecto. Al iniciar se aplican las migraciones pendientes y, si la tabla de monedas está vacía, se cargan algunas monedas de ejemplo (PYG, USD, EUR, BRL, ARS). La moneda base es PYG = 1 y las tasas son solo de ejemplo.

Si se quiere manejar las migraciones a mano, los comandos se ejecutan dentro de la carpeta del proyecto (la misma desde la que se hace `dotnet run`):

```bash
dotnet tool install --global dotnet-ef

# aplicar las migraciones
dotnet ef database update

# crear una migración nueva
dotnet ef migrations add NombreDeLaMigracion
```

Para empezar de cero alcanza con borrar el archivo `app.db` y volver a levantar.

## API Key

Toda la API pide el header `X-API-KEY`. La key está en `appsettings.json`:

```
X-API-KEY: prueba123
```

Si falta o es incorrecta la API responde `401 Unauthorized`. La única ruta que no la pide es `/swagger`, para poder ver la documentación. Dentro de Swagger hay que usar el botón **Authorize** y cargar la key para poder probar los endpoints.

Ejemplo con curl:

```bash
curl http://localhost:5163/users -H "X-API-KEY: prueba123"
```

## Endpoints

El archivo `UsersApi/UsersApi.http` tiene un ejemplo de cada endpoint ya con la API Key cargada. Se puede ejecutar directamente desde Visual Studio, Rider o VS Code (con la extensión REST Client).

Los ejemplos con curl de este README están escritos para bash (Git Bash, Linux o Mac). En la consola de Windows es más cómodo usar Swagger o el archivo `.http`.

### Users

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/users` | Crear usuario |
| GET | `/users` | Listar usuarios. Filtro opcional `?isActive=true` o `false` |
| GET | `/users/{id}` | Obtener usuario por id |
| PUT | `/users/{id}` | Modificar name, email e isActive. Si no se envía `isActive` se mantiene el valor actual |
| DELETE | `/users/{id}` | Eliminar usuario (borra también sus direcciones) |
| POST | `/users/bulk` | Carga masiva |

```bash
curl -X POST http://localhost:5163/users \
  -H "X-API-KEY: prueba123" -H "Content-Type: application/json" \
  -d '{ "name": "Juan", "email": "juan@test.com" }'
```

El campo `password` es opcional. Si se envía se guarda hasheado y nunca se devuelve en las respuestas.

### Addresses

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/users/{userId}/addresses` | Crear dirección para un usuario |
| GET | `/users/{userId}/addresses` | Listar direcciones de un usuario |
| PUT | `/addresses/{id}` | Modificar dirección |
| DELETE | `/addresses/{id}` | Eliminar dirección |

Si el usuario no existe se responde `404`.

### Currencies

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/currencies` | Listar monedas |
| POST | `/currencies` | Crear moneda |
| POST | `/currency/convert` | Convertir un monto entre dos monedas |

```bash
curl -X POST http://localhost:5163/currency/convert \
  -H "X-API-KEY: prueba123" -H "Content-Type: application/json" \
  -d '{ "fromCurrencyCode": "USD", "toCurrencyCode": "PYG", "amount": 100 }'
```

Respuesta:

```json
{ "fromCurrency": "USD", "toCurrency": "PYG", "originalAmount": 100, "convertedAmount": 730000 }
```

### Errores

- `400`: errores de validación, con el detalle por campo. También cuando el body no es un JSON válido.
- `401`: falta la API Key o es incorrecta.
- `404`: el recurso no existe.
- `409`: email o código de moneda repetido.
- `500`: error inesperado. Se registra en el log y no se expone el detalle.

Los errores de validación indican el campo y el motivo:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "name": ["El nombre es obligatorio"],
    "email": ["El email no tiene un formato valido"]
  }
}
```

El resto de los errores devuelve un JSON con el motivo:

```json
{ "error": "Usuario no encontrado" }
```

## Carga masiva

El archivo de prueba está en `sample-data/bulk_users_test.json`. Desde la raíz del repo:

```bash
curl -X POST http://localhost:5163/users/bulk \
  -H "X-API-KEY: prueba123" -H "Content-Type: application/json" \
  --data-binary @sample-data/bulk_users_test.json
```

La respuesta indica cuántos se crearon y cuáles fallaron con el motivo. `index` es la posición del usuario dentro del arreglo enviado:

```json
{
  "total": 4,
  "created": 3,
  "failed": 1,
  "createdUsers": [ ... ],
  "errors": [
    { "index": 2, "email": "marta.silva@test.com", "error": "Ya existe un usuario con ese email" }
  ]
}
```

Un usuario inválido o repetido no cancela el resto de la carga. El máximo es de 1000 usuarios por request y se procesan de a 10 en paralelo.

## Tests

El proyecto `UsersApi.Tests` usa xUnit y tiene dos tipos de tests. Desde la raíz del repo:

```bash
dotnet test
```

- **Unitarios** (carpeta `Unit`): prueban los validadores de FluentValidation y el cálculo de la conversión, sin base de datos.
- **Integración** (carpeta `Integration`): levantan la API en memoria con una base SQLite temporal, así que no tocan el archivo `app.db`. Cubren la API Key, el CRUD de usuarios y direcciones, la carga masiva (usando el mismo JSON de `sample-data`), las monedas y la conversión.

Para correr solo los unitarios:

```bash
dotnet test --filter "FullyQualifiedName~UsersApi.Tests.Unit"
```

Además hay un workflow de GitHub Actions (`.github/workflows/ci.yml`) que compila y corre todos los tests en cada push a `main`.

## Estructura

```
UsersApi.slnx             Solución con los dos proyectos
UsersApi.Tests/           Tests unitarios y de integración
UsersApi/
  Application/
    Common/               Result que devuelven los handlers
    Users/Commands        Crear, modificar, eliminar y carga masiva
    Users/Queries         Listar y obtener por id
    Addresses/Commands
    Addresses/Queries
    Currencies/Commands
    Currencies/Queries
    CurrencyConversion
  Data/                   DbContext e inicialización de la base
  Endpoints/              Rutas de Minimal API
  Middleware/             Manejo de errores no controlados
  Migrations/
  Models/
  Security/               Middleware de API Key
sample-data/              JSON de prueba para la carga masiva
```

## Decisiones

- **DbContextFactory:** se registra `IDbContextFactory<AppDbContext>` con `AddDbContextFactory` y cada handler crea su propio contexto. No se inyecta el `DbContext` directamente porque no se puede compartir una misma instancia entre tareas que corren al mismo tiempo.
- **Paralelismo:** hay dos flujos con `Task.WhenAll`. En la carga masiva cada usuario se inserta en una tarea con su propio `DbContext`, con un `SemaphoreSlim` que limita a 10 las que corren a la vez. En la conversión se buscan la moneda de origen y la de destino en paralelo. En los dos casos las tareas se lanzan con `Task.Run`, porque el proveedor de SQLite ejecuta las llamadas async de forma síncrona y sin eso correrían una detrás de otra.
- **CQRS:** cada caso de uso tiene su command o query, su handler y su validador en archivos separados. Los handlers devuelven un `Result` y el endpoint lo traduce al código HTTP. No se usó MediatR para no agregar una dependencia más.
- **Validaciones:** los commands y queries que reciben datos del body tienen su validador de FluentValidation, y los errores se devuelven con `400` indicando el campo. Los de eliminar y los de consulta por id no lo necesitan porque solo reciben un id.
- **Email:** se guarda en minúsculas, porque SQLite distingue mayúsculas y si no `Juan@test.com` y `juan@test.com` quedarían como dos usuarios distintos.
- **Monedas:** el código se guarda en mayúsculas. Si en la conversión alguna moneda no existe se responde `404`.

## Qué quedó implementado y qué no

Implementado:

- CRUD de Users y Addresses (relación 1:N)
- Monedas y conversión de divisas
- Seguridad por API Key
- EF Core con SQLite y migraciones
- Validadores con FluentValidation
- CQRS con las carpetas pedidas
- Paralelismo con `Task.WhenAll` y uso de `IDbContextFactory`
- Carga masiva con su JSON de prueba
- Swagger
- Tests unitarios y de integración

Pendiente:

- No hay endpoint de login, el password solo se guarda hasheado.
- Paginación en los listados.
