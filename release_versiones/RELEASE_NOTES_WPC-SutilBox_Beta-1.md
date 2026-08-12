# WPC-SutilBox Beta-1

## Entrega

Primera entrega funcional de la nueva línea WPC-SutilBox. Esta Beta-1 representa un nuevo comienzo; la futura versión estable se publicará más adelante como **WPC-SutilBox V1.0.0**.

## Artefacto

- Archivo: `WPC-SutilBox-Beta-1-win-x64-self-contained.zip`
- Plataforma: Windows x64
- Tipo: autocontenido / single-file
- Requisito: no necesita instalar .NET Runtime
- Tag: `wpc-sutilbox-beta-1`

## Funcionalidad incluida

- Botón **Optimizar** con limpieza de temporales, liberación de RAM y aplicación del perfil activo.
- Monitorización de CPU, RAM, temperatura, batería, red, disco y núcleos.
- Revisar mi PC con resumen de salud, almacenamiento, hardware/temperaturas y procesos.
- Limpieza de espacio y análisis de archivos grandes.
- Gestor de inicio, bloatware y actualizaciones Winget.
- Perfiles Equilibrado, Gaming, Productividad, Desarrollo y A tu medida.
- Puntos de restauración antes de cambios de perfil.
- Historial de logs dentro de la aplicación.
- Configuración persistente, modo claro, modo oscuro e idioma.

## Instalación

1. Descargar el ZIP.
2. Extraerlo en una carpeta local.
3. Ejecutar `WassControlSys.exe`.
4. Aceptar UAC cuando una operación administrativa lo requiera.

## Verificación

Compilado con `dotnet publish -c Release -r win-x64 --self-contained true`.

Resultado: 0 advertencias y 0 errores.

## Consideraciones

- Algunas funciones requieren permisos de administrador.
- La temperatura depende de los sensores expuestos por el firmware.
- Winget debe estar disponible para actualizar aplicaciones.
- La creación de puntos de restauración requiere que Protección del sistema esté habilitada.
