<div align="center">
  <h1>馃Ь Microservicio de Pedidos</h1>
  <h3> Encargado de la gesti贸n de 贸rdenes, mesas y su ciclo de vida en el restaurante. </h3>
</div>

* **Gesti贸n de 脫rdenes:** Creaci贸n de pedidos, 铆tems y notas, con seguimiento de historial de estados.
* **Gesti贸n de Mesas:** Alta, edici贸n y control de disponibilidad de mesas (Sal贸n, Patio, Barra y Terraza).
* **Estados y Transiciones:** M谩quina de estados con validaci贸n estricta para el ciclo de vida de 贸rdenes e 铆tems.
* **Integraci贸n con Cocina y Stock:** Env铆o de tickets a cocina y reserva/liberaci贸n de stock de productos.
* **Notificaciones en Tiempo Real:** Actualizaci贸n instant谩nea de pedidos y mesas mediante **SignalR**.
* **Autenticaci贸n y Roles:** Protecci贸n por **JWT**, con acceso diferenciado para roles Admin, Waitress y Kitchen.
