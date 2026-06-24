# OrderService - Microservicio de Gestión de Pedidos

OrderService es el microservicio encargado de gestionar los pedidos (órdenes) de un restaurante: creación de órdenes, ítems y notas, cambios de estado, mesas, y la integración con cocina, stock, catálogo de productos y usuarios.

## Requisitos
Antes de iniciar, se debe tener instalado en el sistema:
- `.NET 8 SDK`
- `SQL Server LocalDB`
- `Visual Studio 2022` o `Visual Studio Code`

OrderService se comunica con otros microservicios por HTTP, por lo que para operar de forma completa deben estar en ejecución:
- **Users** — emite los JWT y valida la existencia del mozo.
- **MenuCatalog** — catálogo de productos (platos y bebidas).
- **Stock** — reserva y liberación de stock de productos.
- **Kitchen** — recibe el pedido de la orden para su preparación.

Pasos para ejecutar el proyecto:

## 1. Clonar el repositorio
```bash
git clone https://github.com/Organizacion-Proyecto-Final/MicroservicioOrder.git
cd MicroservicioOrder
```

## 2. Configurar la cadena de conexion
La cadena de conexion se encuentra en `OrderService/appsettings.json`.

Configuracion por defecto:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OrderServiceDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## 3. Configurar la URL de los microservicios
En `OrderService/appsettings.json`, dentro de `ExternalServices`, se indican las URLs base de cada microservicio:

```json
"ExternalServices": {
  "Users":       { "BaseUrl": "http://localhost:7060" },
  "MenuCatalog": { "BaseUrl": "http://localhost:7025" },
  "Stock":       { "BaseUrl": "http://localhost:7158" },
  "Kitchen":     { "BaseUrl": "http://localhost:5207" }
}
```

Estos valores se pueden sobrescribir en `OrderService/appsettings.Development.json` para el entorno de desarrollo, donde se usan URLs HTTPS.

## 4. Ejecutar el proyecto
Desde la raiz de la solucion, ejecutar:

```bash
dotnet run --project OrderService
```

Tambien se puede abrir `OrderService.sln` en Visual Studio y establecer `OrderService` (el proyecto web) como proyecto de inicio.

## 5. Inicializacion de base de datos
Al iniciar la aplicacion por primera vez en el entorno de desarrollo, el sistema realiza automaticamente:
- creacion de la base de datos
- aplicacion de migraciones pendientes
- precarga de datos iniciales

No es necesario ejecutar migraciones manualmente.

Importante: si la base de datos contiene datos parciales de una inicializacion anterior, la precarga no se rehace automaticamente. En ese caso, se debe limpiar la base de datos antes de volver a ejecutar el proyecto.

## Datos demo precargados
La aplicacion crea automaticamente 14 mesas distribuidas en Salon, Patio, Barra y Terraza.

## Autenticacion
OrderService esta protegido con JWT por roles (`Admin`, `Waitress`, `Kitchen`). Los tokens los emite el microservicio **Users** y deben enviarse en el header `Authorization: Bearer {token}`. Los usuarios de prueba se crean en ese microservicio.

## Accesos a la aplicacion
Una vez levantada la aplicacion, se puede acceder desde:
- Pagina web HTTP: http://localhost:5231/
- Pagina web HTTPS: https://localhost:7100/
- Swagger UI: https://localhost:7100/swagger