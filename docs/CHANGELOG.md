# Historial de Cambios — WPC-SutilBox

## [1.1.1-beta] - 2026-08-20 (Beta 1.1.1 Renacimiento)
### Añadido
* **Nuevo Dashboard Principal:** Tarjeta de Salud y Optimización con diagnóstico no destructivo y botón "Analizar y optimizar".
* **Monitoreo Global en Tiempo Real:** Módulos visuales modernos para CPU %, RAM %, Temperatura CPU y Batería.
* **Selector de Modos de Rendimiento:** Cards interactivas para modos Equilibrado, Gaming, Productividad y Desarrollo.
* **Centro de Actualizaciones Unificado:** Fusión de actualizaciones de aplicaciones (Winget) y parches del SO (Windows Update) en una sola vista con pestañas.
* **Confirmación de Actualización Global:** Cuadro de diálogo modal que solicita consentimiento antes de ejecutar actualizaciones masivas en Winget.

### Mejorado & Corregido
* **WingetService:** Eliminación de bloqueos infinitos en búsquedas mediante flags `--accept-source-agreements --include-unknown` y timeout de seguridad de 35s.
* **Navegación Lateral (Sidebar):** Limpieza de sub-botones redundantes bajo "Revisar mi PC", trasladando la interacción limpia al `TabControl` interno.
* **Compilación y Metadatos:** Actualización de identidad a WPC-SutilBox, manifiesto UAC v1.1.1.0 y compilación limpia con 0 advertencias y 0 errores.
* **Publicación:** Paquete **Autocontenido (Self-Contained Single-File)** para Windows x64.

---

## [1.1.0-beta] - 2026-08-20
### Cambiado
* Metamorfosis inicial del proyecto a `WPC-SutilBox` y RootNamespace `Wpc_SutilBox`.