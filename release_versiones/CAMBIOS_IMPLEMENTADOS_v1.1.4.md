# Cambios implementados — WPC-SutilBox Beta 1.1.4

**Fecha de cierre documental:** 2026-10-15  
**Versión:** `1.1.4-beta` (`1.1.4.0`)  
**Commit de implementación:** `9921711`  
**Estado:** compilado en Release; validación manual en Windows pendiente

Este documento sustituye la nota histórica de WassControlSys fechada en diciembre de 2025. Aquella nota describía otra generación del producto y no corresponde al código de esta beta.

## Objetivo

Hacer verificable la aplicación de los perfiles de rendimiento existentes, sin ampliar la lista de ajustes del sistema.

## Comportamiento implementado

1. `MainViewModel.ExecuteApplyPerformanceModeAsync` pide confirmación. Cancelar no modifica el sistema ni el modo visible.
2. Solo se acepta un valor definido de `PerformanceMode`.
3. `PerformanceProfileService.ApplyProfileAsync` exige `IRestorePointService` y un punto de restauración creado con éxito.
4. El modo `General` (Equilibrado) llama a `RestoreOriginalStateAsync`.
5. El snapshot guarda el plan de energía activo y, al detener servicios, su tipo de inicio y si estaban en ejecución. Solo se crea si aún no existe.
6. Si `powercfg` no puede iniciarse o UAC devuelve la cancelación 1223, el flujo se detiene.
7. Los nombres de `ServicesToStop` se deduplican sin distinguir mayúsculas.
8. Una excepción en las acciones posteriores al plan devuelve fallo, aunque el plan ya se haya solicitado.
9. La restauración no borra el snapshot si falla el plan o cualquier servicio. Lo borra solo cuando esas restauraciones reportan éxito.
10. `CurrentMode` solo se actualiza cuando `ApplyProfileResult.Success` es verdadero.

## Archivos

| Archivo | Cambio |
|---|---|
| `Core/PerformanceProfiles.cs` | Validación, punto de restauración, plan, snapshot, deduplicación y resultado explícito |
| `Core/IPerformanceProfileService.cs` | Contrato `ApplyProfileResult` |
| `ViewModels/MainViewModel.cs` | Confirmación, rechazo de selección inválida y estado visible condicionado al éxito |
| `Views/DashboardView.xaml` | Etiqueta `Beta 1.1.4 · Perfiles de rendimiento` |
| `WPC-SutilBox.csproj` | `1.1.4-beta`, `AssemblyVersion` y `FileVersion` `1.1.4.0` |
| `app.manifest` | Identidad `1.1.4.0` y ejecución como administrador |

## Perfiles existentes que cubre el flujo

| Perfil | `PerformanceMode` | Plan de energía predeterminado |
|---|---|---|
| Equilibrado | `General` | Restaura el GUID capturado |
| Gaming | `Gamer` | Alto rendimiento |
| Desarrollo | `Dev` | Equilibrado |
| Productividad | `Oficina` | Equilibrado |
| A tu medida | `Personalizado` | Configuración guardada o equilibrado |

Los ajustes concretos de cada perfil siguen saliendo de `ProfileConfig` y de la configuración persistida. Esta beta no redefine esa lista.

## Límites que el código deja abiertos

- `TrySetPowerPlan` inicia el proceso y no espera `WaitForExit` ni lee el código de salida.
- `SetWindowsUpdateStatus`, `SetTelemetryStatus`, `SetIndexingStatus` y `SetNetworkThrottling` capturan errores sin devolver un resultado al flujo principal.
- Esas acciones secundarias no forman parte del snapshot restaurable.
- Un punto de restauración de Windows no equivale a revertir cada clave modificada por la aplicación.

## Compilación

```text
dotnet build WPC-SutilBox.csproj -c Release
Compilación correcta.
0 Advertencia(s)
0 Errores
```

Salida: `bin/Release/net8.0-windows/win-x64/`.  
Paquete: `bin/Release/WPC-SutilBox-1.1.4-beta-win-x64.zip`.

## Prueba manual todavía pendiente

1. Registrar el plan activo y el estado de los servicios antes de aplicar.
2. Cancelar el diálogo y comprobar que nada cambia.
3. Aplicar un perfil elevado y confirmar que existe el punto de restauración.
4. Cancelar UAC y comprobar que no continúan acciones secundarias.
5. Volver a Equilibrado y comprobar solo los valores presentes en el snapshot.
6. Forzar un servicio inexistente o denegado y comprobar que la interfaz no anuncia éxito.
