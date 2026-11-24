# Fase 3 — Seguridad y protección

## Implementado

- La herramienta kill_process del servidor MCP exige confirm=true, rechaza PID del sistema y el PID del propio servidor, protege procesos esenciales y solo termina el proceso seleccionado.
- La interfaz gráfica solicita confirmación indicando el PID, el riesgo de perder datos sin guardar y que la acción no se puede deshacer.
- ProcessManagerService vuelve a validar PID y nombre protegido en el punto de ejecución, aunque se invoque sin pasar por la interfaz.
- get_environment_variables redacta valores asociados a nombres sensibles (token, secreto, contraseña, credencial, claves y autenticación), además de patrones conocidos de tokens.
- list_processes y list_services siguen siendo operaciones de solo lectura.
- El borrado del mapa de espacio mantiene selección explícita, tres confirmaciones y validación de raíces temporales permitidas.

## Verificación y límites

- Compilación de SutilBox.McpServer en .NET 8: correcta, sin advertencias ni errores.
- Compilación WPF: correcta; se observaron advertencias CA1416 en llamadas de APIs Windows existentes. No se silenciaron.
- La lista de nombres protegidos es una defensa adicional; Windows también puede rechazar el cierre por permisos o protección del proceso.
- El servidor MCP no tiene un canal de confirmación interactiva. Por eso, la herramienta requiere una confirmación booleana explícita por llamada.
