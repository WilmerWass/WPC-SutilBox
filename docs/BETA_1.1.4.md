# Beta 1.1.4 — Perfiles de rendimiento

**Estado:** Implementación y compilación completadas; validación funcional en Windows pendiente.  
**Versión:** `1.1.4-beta` (`1.1.4.0`)  
**Base:** commit local de Beta 1.1.3 `8b2735d`.

## Objetivo

Hacer más explícita y verificable la aplicación de perfiles ya existentes, sin ampliar la lista de ajustes del sistema. El alcance se limita al flujo de perfiles de rendimiento del Dashboard: confirmación previa, validación de la selección, protección ante prerequisitos ausentes, resultados que reflejen fallos y actualización del estado visible solo tras éxito.

## Cambios

- Se solicita confirmación antes de aplicar un perfil o volver a Equilibrado/restaurar el snapshot.
- Se rechazan parámetros que no correspondan a un valor definido de `PerformanceMode`.
- Se requiere que el servicio de punto de restauración esté disponible y que consiga crear el punto antes de iniciar cambios del perfil.
- Se captura el estado original solo antes del primer perfil y se preserva el mecanismo de snapshot ya existente.
- La restauración conserva la captura cuando falla el plan de energía o cualquier servicio que intenta restaurar, y solo limpia el snapshot cuando todas esas restauraciones reportan éxito.
- Se comprueba el resultado al aplicar el plan de energía; si falla o se cancela UAC, no se continúan aplicando ajustes de servicios/sistema.
- Se evita procesar repetidamente nombres de servicios duplicados (sin distinguir mayúsculas/minúsculas).
- El resultado del perfil solo se comunica como éxito tras completar el flujo; las excepciones de acciones adicionales producen un estado de fallo explicativo.
- Se sincroniza la versión del ensamblado, el manifiesto y la etiqueta Beta del Dashboard.

## Alcance y límites detectados

Esta iteración no presenta los perfiles como una transacción completamente reversible: el snapshot persistente actualmente guarda plan de energía y estados de ciertos servicios, pero no todos los valores del Registro ni cada acción secundaria. La restauración no debe interpretarse como una reversión universal. La configuración existente incluye acciones cuyo código es incompleto o no informa fielmente el resultado, por ejemplo algunos cambios de Windows Update, telemetría, indexación y throttling de red. No se han ampliado ni se declaran validadas aquí; requieren una beta dedicada antes de anunciar reversibilidad completa.

La creación de punto de restauración es un prerequisito en el servicio de perfiles. No sustituye al snapshot ni garantiza por sí sola que Windows permita revertir cada ajuste.

## Archivos modificados

- `Core/PerformanceProfiles.cs`: validación del enum, prerequisito de punto de restauración, comprobación del plan, resultado de ejecución explícito y deduplicación de servicios.
- `ViewModels/MainViewModel.cs`: confirmación contextual, rechazo de selección inválida y protección contra dependencias de diálogo ausentes.
- `WPC-SutilBox.csproj`: versión `1.1.4-beta` y versiones de ensamblado/archivo `1.1.4.0`.
- `app.manifest`: identidad `1.1.4.0`.
- `Views/DashboardView.xaml`: etiqueta `Beta 1.1.4 · Perfiles de rendimiento`.

## Validación ejecutada

- `dotnet build WPC-SutilBox.csproj -c Release --no-restore`: aprobado, 0 errores y 0 advertencias.
- `dotnet test WPC-SutilBox.csproj -c Release --no-build`: no hay proyecto de pruebas automatizadas asociado; no se atribuyen pruebas funcionales.
- No se han ejecutado pruebas en un Windows interactivo elevado: aplicación de perfiles, creación de punto de restauración, cancelación UAC, inspección de cambios reales y restauración siguen pendientes de validación manual.

## Pruebas manuales pendientes

1. Conservar evidencia del plan activo y del tipo/estado de los servicios involucrados.
2. Cancelar el diálogo de confirmación y verificar que el sistema no cambia.
3. Aplicar un perfil en una máquina de prueba/elevada; confirmar el punto de restauración y observar los resultados reales.
4. Cancelar UAC para `powercfg` y confirmar que no se continúan acciones secundarias.
5. Elegir Equilibrado y verificar que se restauran únicamente valores respaldados por el snapshot.
6. Probar servicios no disponibles o cambios denegados y verificar que la interfaz no anuncia éxito.
7. Verificar estado visible y logs en cada camino de éxito, cancelación y fallo.

## Riesgos residuales

- El proceso de elevación de `powercfg` inicia un proceso sin esperar y validar el código de salida; esta limitación heredada queda reflejada y requiere un arreglo específico antes de poder asegurar que su éxito representa el resultado efectivo del sistema.
- Los métodos heredados de ajustes secundarios ocultan algunos fallos y/o no esperan la finalización de operaciones de servicio. Esta versión evita ampliar esa funcionalidad, pero la validación de reversibilidad sigue pendiente.
- Un punto de restauración puede no estar habilitado o fallar por política, disponibilidad o permisos; en ese caso el perfil se cancela deliberadamente.

## Resultado

Beta 1.1.4 deja más seguro y honesto el flujo de aplicación del perfil: solicita consentimiento, detiene el flujo ante prerequisitos/plan fallidos y no comunica éxito tras una excepción. No se declara completada la validación funcional del sistema ni restauración integral. Sin push; commit local por separado.
