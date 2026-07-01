<div align="center">
  <h1>🧾 Microservicio de Pedidos</h1>
  <h3> Encargado de la gestión de órdenes, mesas y su ciclo de vida en el restaurante. </h3>
</div>

* **Gestión de Órdenes:** Creación de pedidos, ítems y notas, con seguimiento de historial de estados.
* **Gestión de Mesas:** Alta, edición y control de disponibilidad de mesas (Salón, Patio, Barra y Terraza).
* **Estados y Transiciones:** Máquina de estados con validación estricta para el ciclo de vida de órdenes e ítems.
* **Integración con Cocina y Stock:** Envío de tickets a cocina y reserva/liberación de stock de productos.
* **Notificaciones en Tiempo Real:** Actualización instantánea de pedidos y mesas mediante SignalR.
* **Autenticación y Roles:** Protección por JWT, con acceso diferenciado para roles Admin, Waitress y Kitchen.
