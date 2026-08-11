# Arquitectura de WPC-SutilBox Beta 1

## Capas

```text
Views (XAML)
    ↓ bindings y comandos
ViewModels (MainViewModel, ProfileEditorViewModel)
    ↓ interfaces
Core (servicios Windows)
    ↓
WMI / PerformanceCounter / Registry / Process / PowerStatus
```

## Navegación

`MainWindow` mantiene una única región de contenido. `CurrentSection` decide qué `UserControl` está visible mediante `EnumToVisibilityConverter`; el resto permanece `Collapsed`. `Revisar mi PC` usa `PcReviewView`, que agrupa salud, almacenamiento, hardware/temperatura y procesos de alto consumo en pestañas internas.

## Servicios principales

| Servicio | Responsabilidad |
|---|---|
| `MonitoringService` | CPU, RAM, red, disco, núcleos y conexiones TCP |
| `TemperatureMonitorService` | Temperatura WMI |
| `BatteryService` | `PowerStatus` y `Win32_Battery` |
| `ProcessManagerService` | Procesos, prioridades, finalización y RAM |
| `PerformanceProfileService` | Planes de energía, servicios y ajustes de perfil |
| `RestorePointService` | Puntos de restauración WMI |
| `StartupService` | Aplicaciones de inicio y Registro |
| `BloatwareService` | Detección y desinstalación |
| `WingetService` | Consulta y actualización de aplicaciones |
| `SettingsService` | Persistencia JSON |
| `FileLogService` | Log de sesión y lectura de actividad reciente |

## Flujo de Optimizar

1. El usuario pulsa `QuickOptimizeCommand`.
2. Se eliminan temporales de usuario.
3. Se ejecuta `OptimizeRamAsync`.
4. Se aplica el `PerformanceMode` activo.
5. Se actualizan las métricas y se registra el resultado.

## Temas

`Theme.Dark.xaml` y `Theme.Light.xaml` declaran el mismo contrato de recursos: `BackgroundBrush`, `SurfaceBrush`, `TextPrimaryBrush`, `TextSecondaryBrush`, `BorderBrush`, `PrimaryBrush` y estados semánticos. Los controles reutilizables consumen `DynamicResource`, por lo que el cambio de tema no requiere reiniciar.

## Seguridad

Las operaciones administrativas se ejecutan con UAC. Los perfiles solicitan un punto de restauración antes de modificar el sistema. La configuración se almacena en `%LOCALAPPDATA%\\Wpc_SutilBox` y no contiene credenciales.
