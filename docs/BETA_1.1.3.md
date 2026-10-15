# Beta 1.1.3 — Optimización controlada

**Estado:** Implementación técnica completada; validación manual en Windows pendiente.  
**Alcance:** previsualización, confirmación explícita y resultado de limpieza temporal.  
**Fuera de alcance:** papelera, cachés de navegador, Windows Update, RAM, perfiles, Registro, servicios y procesos.

## Cambios realizados

- Se añadió `CleanupCandidate` para representar la ruta y tamaño observado de cada archivo temporal elegible.
- `ISystemMaintenanceService`/`SystemMaintenanceService` ahora ofrecen previsualización cancelable y borrado posterior de una lista explícita de candidatos.
- El escaneo solo considera directorios temporales seleccionados y extensiones temporales ya soportadas por el servicio; ignora enlaces/reparse points, evita rutas fuera de ubicaciones temporales permitidas y vuelve a validar cada archivo en el momento del borrado.
- El cálculo de espacio estimado se toma de los tamaños observados durante el escaneo. El borrado usa el tamaño presente inmediatamente antes de eliminar; archivos que ya no existen se omiten y los errores de acceso se cuentan.
- El Dashboard ya no ejecuta de forma encadenada limpieza, vaciado de working sets de procesos y aplicación de perfiles. Su botón presenta la previsualización temporal, solicita consentimiento y enseña el resultado.
- La vista de limpieza indica expresamente que Papelera y caché de navegador no están disponibles en esta beta. La selección de temporales está conectada al ViewModel.
- Se sincronizaron metadatos de producto a `1.1.3-beta` / `1.1.3.0` y el badge del Dashboard.

## Seguridad y limitaciones

La limpieza requiere consentimiento después de mostrar cantidad de archivos y tamaño aproximado. No vacía la Papelera. El alcance deliberadamente no incluye Windows Update, cachés de navegador, cookies, descargas, registros, Prefetch ni carpetas de perfiles ajenos. No se altera ni normaliza el atributo de cada archivo durante la operación previsualizada. Hay una condición de carrera inherente entre escaneo y borrado: antes de cada borrado se verifica ruta, patrón, existencia y tipo de archivo; aun así, el usuario debe cerrar aplicaciones si quiere evitar que estas vuelvan a crear temporales.

El código de la vista de limpieza aún contiene otras acciones antiguas que están fuera del alcance de Beta 1.1.3 y no deben confundirse con la nueva acción controlada.

## Verificación

- `dotnet build WPC-SutilBox.csproj --configuration Release`: aprobado, 0 errores y 0 advertencias.
- `dotnet test --no-restore --verbosity minimal`: comando completado sin fallos; el repositorio no contiene un proyecto de pruebas descubierto en esta validación.
- `dotnet publish WPC-SutilBox.csproj --configuration Release --no-restore`: aprobado; artefactos en `bin/Release/net8.0-windows/win-x64/publish/`.
- Prueba en equipo Windows: pendiente. Confirmar que Cancelar no borra archivos, Confirmar elimina únicamente candidatos previsualizados, el resultado informa archivos/bytes/fallos y archivos bloqueados no interrumpen el lote.
- Ninguna medición en equipo físico se atribuye a esta implementación.

## Archivos principales

- `Models/CleanupCandidate.cs`
- `Core/ISystemMaintenanceService.cs`
- `Core/SystemMaintenanceService.cs`
- `ViewModels/MainViewModel.cs`
- `Views/DashboardView.xaml`
- `Views/CleaningView.xaml`
- `WPC-SutilBox.csproj`
- `app.manifest`
