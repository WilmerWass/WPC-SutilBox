# Guía de Desarrollo y Normas de Código — WPC-SutilBox

## 1. Regla de Oro de Implementación
> **Una funcionalidad NO se considera implementada hasta que exista código ejecutable y pruebas reproducibles que la respalden.**

No se deben actualizar estados a 🟢 IMPLEMENTADO basándose en ideas, mockups XAML aislados ni código sin integrar al flujo principal.

## 2. Convenciones de Código
* **Lenguaje:** C# con reglas de estilo estándar de Microsoft.
* **Asincronía:** Todo método I/O, acceso a disco, WMI, Registro o PowerShell debe ser `async Task` (nunca `async void` salvo controladores de eventos UI).
* **Nombres:**
  * Vistas: `[Nombre]View.xaml`
  * ViewModels: `[Nombre]ViewModel.cs`
  * Servicios: `[Nombre]Service.cs` e Interfaces `I[Nombre]Service.cs`

## 3. Manejo de UAC, Registro y PowerShell
* **UAC:** Las acciones que requieran elevación de privilegios deben estar claramente delimitadas. Se debe comprobar si el proceso actual corre con privilegios elevados (`WindowsIdentity.GetCurrent()`) antes de invocar operaciones restringidas.
* **Registro de Windows:** Toda modificación debe ir acompañada de un bloque `try-catch` específico para `UnauthorizedAccessException` y `SecurityException`, y registrar un punto de restauración/backup de la clave previa.
* **PowerShell:** Se prefiere el uso de APIs nativas de .NET sobre invocaciones de scripts de PowerShell siempre que sea posible por motivos de rendimiento y seguridad.

## 4. Estándar de Logging y Excepciones
* No silenciar excepciones con bloques `catch {}` vacíos.
* Todo fallo debe canalizarse a través del servicio centralizado de logs (`ILoggerService`) guardando el contexto en `ANOSUBIR/` o en el directorio operativo local.

## 5. Control de Calidad Antes de Realizar Commits
1. Compilación limpia sin errores ni warnings.
2. Verificar que los DataBindings XAML no reporten fallos en la ventana de Salida de Debugging.
3. Actualizar `docs/PLAN_BETAS.md` y `docs/CHANGELOG.md` únicamente con los cambios verificados.