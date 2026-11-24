# Fase 5 — Navegación y Zona Gamer

## Biblioteca implementada

- Se añadió una ruta Zona Gamer con ViewModel propio y registro DI.
- Steam: se leen las bibliotecas locales y manifiestos appmanifest; una entrada solo se muestra si existe su carpeta de instalación.
- Epic Games: se leen manifiestos locales .item y se comprueba la carpeta InstallLocation.
- Se deduplican entradas por ruta y se muestra nombre, plataforma, ruta y método de verificación.
- Se puede filtrar por nombre/plataforma y volver a analizar de forma asíncrona.
- La pantalla ya no muestra perfiles o métricas ficticias ni botones de optimización sin implementación.

## Límites visibles

- Xbox/Microsoft Store, Ubisoft, EA, Battle.net, GOG e instalaciones independientes no se detectan en esta fase.
- No se inspeccionan compras en cuentas ni se requiere iniciar sesión.
- No se muestra ejecutable, tamaño, FPS, GPU, VRAM o temperatura porque no se verifican de manera fiable con las integraciones actuales.
- No se aplican perfiles ni cambios al sistema al detectar un juego.
- La reorganización completa de las diez categorías del menú queda parcial; se añadió una ruta visible para Zona Gamer y se conservaron los accesos existentes sin retirar opciones.

## Validación

- Compilación WPF .NET 8: correcta, 0 errores, 0 advertencias.
- Detección real con bibliotecas presentes y revisión visual de escritorio pendientes en esta sesión.
