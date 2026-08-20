# Release Notes — WPC-SutilBox Beta 1.1.1 🚀

¡Bienvenido a la **Beta 1.1.1** de **WPC-SutilBox**!  
Esta versión consolida el verdadero renacimiento del proyecto, transformándolo de un simple inspector de archivos grandes a una **suite integral, moderna y segura de control, diagnóstico y optimización para Windows**.

---

## ✨ Novedades y Mejoras Destacadas

### 🎨 1. Nuevo Dashboard Principal
- **Tarjeta de Salud & Optimización Pro:** Nuevo panel central con diagnóstico en tiempo real de componentes clave (`SysMain`, `Windows Search`, `Efectos visuales`, `Actualizaciones`) y botón destacado **"⚡ Analizar y optimizar"**.
- **Monitoreo Global en Vivo:** Nuevos módulos visuales de alto impacto para **CPU %**, **RAM %** y **Temperatura CPU**, acompañados por barras dinámicas de estado térmico y batería.
- **Selector de Modos de Rendimiento:** Tarjetas interactivas para alternar al instante entre **Equilibrado**, **Gaming**, **Productividad** y **Desarrollo**.
- **Acciones Rápidas del Sistema:** Acceso directo para liberar RAM y crear puntos de restauración de emergencia con un solo clic.

### 🧹 2. Navegación Lateral (Sidebar) Minimalista
- Se eliminó la saturación de sub-botones anidados en el menú lateral.
- La vista de **Revisar mi PC** ahora se gestiona de forma fluida y ordenada mediante sus pestañas internas dedicadas (Resumen de salud, Discos & SMART, Drivers, Procesos y Almacenamiento).

### 🔄 3. Centro de Actualizaciones Unificado
- Se fusionaron las actualizaciones en un único centro de control:
  - **Pestaña 1 (Winget):** Detección, listado y actualización individual de aplicaciones y paquetes instalados.
  - **Pestaña 2 (Windows Update):** Comprobación, pausa preventiva (7 días) y reanudación de parches del sistema operativo y drivers oficiales.

### 🛡️ 4. Winget Blindado & Confirmación de Seguridad
- Se acabaron las búsquedas colgadas: se añadieron flags no interactivos (`--accept-source-agreements --include-unknown`) y un timeout de seguridad de 35 segundos.
- Se implementó un cuadro de diálogo de confirmación previo al ejecutar **"Actualizar Todo"**, previniendo cambios masivos accidentales.

---

## 📦 Ficha Técnica del Artefacto

| Parámetro | Valor |
| :--- | :--- |
| **Versión** | `1.1.1-beta` |
| **Tipo de Binario** | **Autocontenido (Self-Contained) & Single-File** |
| **Arquitectura** | `win-x64` (Windows 10 / Windows 11) |
| **Requisito .NET** | **Ninguno** (No requiere instalar .NET Runtime por separado) |
| **Ejecutable** | `SutilBox.exe` |

---

## 🚀 Instrucciones de Uso

1. Descarga el archivo ejecutable o comprimido `SutilBox.exe`.
2. Ejecuta `SutilBox.exe` en tu equipo (se recomienda ejecutar como Administrador para habilitar todas las funciones de diagnóstico profundo y optimización de servicios).
3. ¡Disfruta de una experiencia fluida, rápida y transparente!

---

## 💡 Filosofía SutilBox
> *Cada cambio es transparente, explicable y reversible. SutilBox no instala programas adicionales ni aplica modificaciones misteriosas.*
