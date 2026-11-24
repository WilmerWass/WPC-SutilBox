# Fase 6 — Dashboard y estado del equipo

## Cambios

- El score de salud ya no informa 100 cuando no hay ninguna comprobación válida: muestra “Sin datos”.
- Los datos ausentes o lecturas fallidas no suman como resultados saludables.
- El score usa penalizaciones diferenciadas (10 puntos por advertencia, 25 por crítico) y se limita de 0 a 100.
- Se limita la lista principal a tres alertas/recomendaciones.
- Las alertas de CPU y RAM enlazan con Rendimiento en vez de Hardware.
- Se conservan las mejoras visuales locales ya presentes en DashboardView.xaml; no se mezclaron en el commit porque estaban antes de esta intervención.

## Pendiente

- La puntuación procedente del escáner rápido conserva el cálculo de SystemScanService y requiere auditarse junto con su cambio local preexistente.
- La validación visual en escritorio no está disponible en esta sesión.
- La distinción visual completa entre estado de aplicación y salud del equipo requiere una revisión de UI con la aplicación ejecutándose.

## Validación

- Compilación WPF en salida aislada: correcta, 0 advertencias y 0 errores. La salida normal estaba bloqueada porque SutilBox.exe seguía abierto; no se cerró la aplicación del usuario.
