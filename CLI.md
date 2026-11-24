# 🖥️ SutilBox CLI — Guía de Uso

**SutilBox CLI** (`sbox`) es la versión de línea de comandos de WPC-SutilBox. A diferencia de la interfaz gráfica (WPF) que solo funciona en Windows, el CLI está diseñado para funcionar en **cualquier sistema con terminal**: Windows, Linux y macOS.

## 📦 Instalación

### Requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado en el sistema.

### Compilar desde el código fuente
```bash
cd WPC-SutilBox
dotnet build SutilBox.CLI/SutilBox.CLI.csproj
```

### Ejecutar
```bash
dotnet run --project SutilBox.CLI/SutilBox.CLI.csproj -- <comando>
```

O directamente desde la DLL compilada:
```bash
dotnet SutilBox.CLI/bin/Debug/net8.0/sbox.dll <comando>
```

## 📋 Comandos

### `sbox scan` — Diagnósticos disponibles del sistema

Ejecuta todos los diagnósticos disponibles y muestra un informe con:
- **Score de salud** (0-100) con indicador visual
- **Problemas agrupados por severidad**: 🔴 Crítico, 🟡 Advertencia, ℹ️ Info
- **Soluciones sugeridas** para cada problema detectado

```bash
sbox scan
```

#### Checks realizados

| Plataforma | Checks implementados |
|---|---|
| **Linux** | Uso de disco (`df`), RAM y swap (`/proc/meminfo`), carga de CPU, temperaturas disponibles, SMART cuando `smartctl` está instalado y tiempo activo. |
| **macOS** | Compatibilidad del CLI y consulta de información básica. Los chequeos del motor Unix que dependen de `/proc`, `/sys`, `df` o `lsblk` pueden no estar disponibles. |
| **Windows** | Porcentaje de uso de unidades listas y uso/disponibilidad de RAM mediante APIs de Windows. No implementa todavía SMART, controladores, temperaturas, inicio, seguridad, privacidad ni restauración. |

El score de Windows se calcula solo con los chequeos de memoria y espacio que se pudieron leer; no representa una auditoría completa de salud del equipo.

### `sbox scan --expert` — Modo experto interactivo

Después del escaneo, entra en un menú interactivo que te guía paso a paso para solucionar cada problema:

```bash
sbox scan --expert
```

**Flujo del modo experto:**
1. Se ejecutan los diagnósticos implementados para la plataforma actual
2. Se muestran los problemas solucionables en un menú numerado
3. El usuario elige un número
4. El CLI muestra la solución detallada con comandos a ejecutar
5. El usuario decide si ejecutar automáticamente o manualmente
6. El problema se marca como resuelto
7. Se repite hasta solucionar todos o salir

### `sbox info` — Información del sistema

Muestra información detallada del hardware y software:

```bash
sbox info
```

**Información mostrada:**
- Sistema operativo y versión
- CPU: modelo y núcleos
- RAM: total y disponible
- Discos: modelo, capacidad y puntos de montaje
- Red: interfaces y direcciones IP

### `sbox help` — Ayuda

Muestra la ayuda y los comandos disponibles:

```bash
sbox help
```

## 🎯 Ejemplos de uso

### Diagnóstico rápido
```bash
# Ver el estado general del sistema
sbox scan

# Ver información del hardware
sbox info
```

### Solucionar problemas
```bash
# Escaneo con modo experto
sbox scan --expert

# O escaneo normal y luego aceptar el modo experto
sbox scan
# Cuando pregunte "¿Deseas entrar en modo experto?", responde "s"
```

### Para técnicos
```bash
# Obtener información completa del sistema antes de un diagnóstico
sbox info
sbox scan

# Revisar problemas críticos
sbox scan | grep -A5 "CRÍTICOS"
```

## 🖥️ Interfaz gráfica vs CLI

| Característica | WPF (Interfaz gráfica) | CLI (Terminal) |
|---|---|---|
| **Windows** | ✅ Completo | ⚠️ Parcial: espacio en disco y memoria |
| **Linux** | ❌ No disponible | ✅ Completo |
| **macOS** | ❌ No disponible | ✅ Completo |
| **Escaneo** | ✅ | ⚠️ Parcial en Windows; más comprobaciones en Linux |
| **Info del sistema** | ✅ | ✅ Datos básicos según plataforma |
| **Modo experto** | ✅ | ✅ |
| **Gráficos en tiempo real** | ✅ | ❌ |
| **Navegación visual** | ✅ | ❌ |

## 📁 Estructura del proyecto CLI

```
SutilBox.CLI/
├── SutilBox.CLI.csproj    # Configuración del proyecto
├── Program.cs             # Punto de entrada y manejo de comandos
├── Models/                # Modelos de datos compartidos
│   ├── ScanCategory.cs
│   ├── IssueSeverity.cs
│   ├── ScanIssue.cs
│   └── ScanResult.cs
└── Core/                  # Lógica del CLI
    ├── IDiagnosticEngine.cs        # Interfaz del motor de diagnóstico
    ├── PlatformDetector.cs         # Detección de Windows vs Unix
    ├── DiagnosticEngineFactory.cs  # Fábrica de motores
    ├── UnixDiagnosticEngine.cs     # Motor para Linux/macOS
    ├── WindowsDiagnosticEngine.cs  # Motor para Windows (stub)
    ├── ScanOutput.cs               # Formateo de resultados del escaneo
    ├── InfoOutput.cs               # Formateo de información del sistema
    └── ExpertMode.cs               # Modo experto interactivo
```

## 🔮 Próximas versiones

- [ ] Comando `clean` — Limpieza de archivos temporales
- [ ] Comando `processes` — Lista de procesos con alto consumo
- [ ] Comando `report` — Generar reporte Markdown exportable
- [ ] Comando `drivers` — Gestión de drivers (solo Windows)
- [ ] Escaneo programado
- [ ] Soluciones automáticas más avanzadas
