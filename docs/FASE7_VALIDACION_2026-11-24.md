# Fase 7 — QA, estabilización y validación

## Verificaciones ejecutadas

| Verificación | Resultado |
| --- | --- |
| WPF .NET 8, salida aislada | Correcta, 0 errores, 0 advertencias en la última compilación |
| CLI .NET 8, salida aislada | Correcta, 0 errores; NU1900 en una ejecución por no poder consultar el índice de auditoría de NuGet |
| MCP .NET 8, salida aislada | Correcta, 0 errores, 0 advertencias |
| dotnet test sobre WPC-SutilBox.sln | Código de salida correcto; la solución no contiene proyecto de tests, así que no ejecutó pruebas unitarias |
| Ejecución CLI en esta intervención | sbox scan e info respondieron correctamente tras corregir un formato de tiempo |
| GUI / revisión visual | No verificada: el canal Computer Use no pudo conectar con su pipe nativo después de reintentos y reinicialización |

Las compilaciones finales usaron carpetas de salida bajo obj/qa-* para no sobrescribir el ejecutable WPF que estaba abierto. No se cerró el proceso de la aplicación.

## Alcance probado

- Los proyectos WPF, CLI y MCP compilan por separado.
- WPF y CLI están en WPC-SutilBox.sln; MCP es un proyecto aparte y se compiló explícitamente.
- No hay proyecto de pruebas automatizadas en el repositorio. Los scripts Python existentes eran cambios locales del usuario; no se ejecutaron porque usan clics por coordenadas y pueden activar operaciones de sistema.
- La biblioteca de juegos se validó por compilación y revisión de sus rutas de detección; no se probó contra instalaciones reales en esta sesión.

## Pendientes de estabilización

- Crear tests unitarios dedicados para alias, redacción de secretos, seguridad de procesos y detección de juegos.
- Validar visualmente las diez categorías, estados vacíos, tamaños de ventana y enlaces al abrir la app.
- La Zona Gamer solo cubre Steam y Epic; no incluye monitor de sesión ni sensores fiables de FPS/GPU/temperatura.
- Completar la consolidación de nueve vistas desconectadas sin descartar lógica útil.
- La navegación todavía no está reorganizada por completo en los diez dominios definidos.
- El nombre docs/PLAN_V1.2.0_BETA1.md no existe en el repositorio; se conserva la documentación presente y se reporta la ausencia.
- Permanecen cambios locales previos del usuario en limpieza, escaneo, dashboard, navegación, mapa de espacio y documentación, además de scripts/capturas sin seguimiento. No se incluyeron en estos commits.

## Commits propios de la intervención

- 8656686 chore: pre-audit checkpoint
- 8a60526 docs: record initial navigation audit
- b53ad72 fix: connect scanner network and privacy integrations
- 97088b5 security: protect sensitive process operations
- d12502a refactor: centralize legacy navigation routes
- e3b9185 feat: add verified gamer game library
- 45f8e05 ui: make dashboard health score evidence based
- El commit de esta fase cierra la documentación y validación final.
