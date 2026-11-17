# Beta 1.1.5 — Diagnóstico y consistencia

**Versión:** `1.1.5-beta` (`1.1.5.0`)  
**Estado:** compilación verificada; validación visual interactiva pendiente.

## Alcance

- Mejoras de estado, progreso y cancelación en flujos de análisis y limpieza.
- Pruebas interactivas de teclado, mouse, pantalla y audio predeterminado; enumeración Plug and Play ampliada.
- Vista de salud SMART de discos conectada a los datos del sistema.
- Navegación/configuración corregidas y consolidación de pantallas duplicadas de aplicaciones.

## Límites

- Windows expone eventos de teclado y mouse a la aplicación, pero no identifica el dispositivo físico que los generó. Para aislar un dispositivo, desconecta los demás.
- La prueba de audio reproduce un tono en la salida predeterminada; no mide todos los canales, niveles ni micrófonos.
- La prueba de pantalla presenta colores sólidos y necesita inspección humana; no detecta píxeles defectuosos automáticamente.
- Estado PnP y SMART dependen de lo que expongan Windows, firmware y controladores. «Sin datos» no significa que el dispositivo esté averiado.
- La compilación no sustituye la prueba manual de cada vista y operación en un equipo Windows.

## Validación de esta entrega

- `dotnet build .\WPC-SutilBox.csproj --no-restore -p:OutDir=.\bin\Debug\audit-hardware-final\ -v:q`: 0 errores; 760 advertencias, principalmente CA1416 de APIs Windows.
- `git diff --check`: correcto.
- No se realizó navegación visual de la aplicación en esta sesión.
