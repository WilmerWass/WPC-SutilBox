# Historial de Cambios — WPC-SutilBox

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/).
Las versiones `1.1.x-beta` de 2026 describen el código actual de WPC-SutilBox.
Las notas históricas de `release_versiones/` anteriores a la Beta 1.1.1 pertenecen a la generación WassControlSys y no describen este binario.

## [1.1.4-beta] - 2026-10-15 (Beta 1.1.4 Perfiles de rendimiento)

### Añadido
* Confirmación explícita antes de aplicar un perfil o restaurar el estado capturado.
* Punto de restauración de Windows como prerequisito: si el servicio no está disponible o el punto no se crea, el perfil no se aplica.
* Resultado explícito (`ApplyProfileResult`) para éxito, cancelación y fallo.

### Cambiado
* El modo Equilibrado restaura únicamente el plan de energía y los servicios guardados en el snapshot.
* El snapshot se conserva si falla el plan de energía o cualquier servicio que se intenta restaurar.
* Si el plan de energía no se aplica o se cancela UAC, no continúan los ajustes de servicios ni las acciones secundarias.
* El estado visible del perfil solo cambia cuando el flujo termina con éxito.
* Los nombres de servicio duplicados se procesan una sola vez, sin distinguir mayúsculas.
* Identidad de producto sincronizada a `1.1.4-beta` / `1.1.4.0` y etiqueta del Dashboard actualizada.

### Limitaciones
* El snapshot no cubre todos los valores del Registro ni cada acción secundaria. Restaurar no es una reversión universal.
* `powercfg` se inicia elevado, pero esta versión no espera ni valida su código de salida.
* Algunos ajustes heredados (Windows Update, telemetría, indexación y throttling de red) no informan fielmente su resultado.
* La validación funcional en un Windows interactivo elevado sigue pendiente.

## [1.1.3-beta] - 2026 (Beta 1.1.3 Limpieza controlada)

### Añadido
* Previsualización cancelable de archivos temporales y borrado posterior solo de la lista confirmada.
* Resultado con archivos, bytes y fallos de acceso.

### Cambiado
* El Dashboard ya no encadena limpieza, vaciado de working sets y aplicación de perfiles.
* Papelera y caché de navegador quedan fuera de esta beta y se indican como no disponibles.

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