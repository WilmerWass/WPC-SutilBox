# Blueprint de producto — WPC-SutilBox Beta 1

## Propuesta

Una consola sencilla para mantener, revisar y optimizar Windows sin repartir el trabajo entre múltiples herramientas del sistema.

## Estructura de producto

```text
Inicio
├── Optimizar
├── Perfiles de rendimiento
└── Uso del sistema

Revisar mi PC
├── Resumen de salud
├── Espacio y almacenamiento
├── Hardware y temperaturas
└── Procesos de alto consumo

Liberar espacio
├── Limpieza básica
├── Descargas
└── Archivos grandes

Aplicaciones
├── Gestor de inicio
├── Bloatware
└── Actualizaciones Winget

Herramientas avanzadas
├── Reparación de Windows
├── Procesos y servicios
└── Perfiles

Historial y seguridad
├── Logs de sesión
├── Restauración / reversión
└── Diagnóstico

Ajustes
├── Idioma y apariencia
├── Frecuencia y optimización en reposo
└── Inicio, bandeja e instalación
```

## Principios de experiencia

1. Cada botón debe ejecutar una acción real o navegar a una vista funcional.
2. Las acciones administrativas deben explicar su alcance y respetar UAC.
3. Los perfiles deben ser reversibles mediante puntos de restauración.
4. Las métricas deben mostrar el estado actual, no valores decorativos.
5. El modo claro y el oscuro deben conservar jerarquía, contraste y espaciado.
6. Los resultados y errores deben quedar visibles en el estado de la interfaz y en el log.

## Modo claro / modo oscuro

El modo claro usa fondo `#F5F7FB`, superficies blancas, texto `#111827`, texto secundario `#374151` y bordes `#D1D5DB`. El modo oscuro conserva superficies profundas y texto claro. Ambos temas comparten nombres de recursos para evitar estilos divergentes.

## Acciones clave

### Optimizar

Limpia temporales, libera working sets de procesos y aplica el perfil activo. Se ejecuta de forma asíncrona y actualiza las métricas al finalizar.

### Perfiles

Equilibrado, Gaming, Productividad, Desarrollo y A tu medida se aplican desde `PerformanceProfileService`. Los cambios sensibles se protegen con restauración previa.

### Historial

`FileLogService` crea un archivo por sesión y expone sus últimas entradas para que el usuario pueda revisar la actividad sin abrir una carpeta externa.

## Fuera del alcance de Beta 1

- Sincronización en la nube.
- Automatizaciones remotas.
- Gestión multiplataforma.
- Reemplazo completo de Windows Defender o Windows Update.
