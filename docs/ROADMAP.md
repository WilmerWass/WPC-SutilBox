# WPC-SUTILBOX — Roadmap

**Estado:** Documento de dirección y planificación de producto  
**Prioridad actual:** Estabilización de WPC-SutilBox General (Beta 1)

---

## 🛑 Política de Prioridades

1. **PRIORIDAD 1:** Estabilización y calidad de Beta 1 (refactor visual, compilación sin warnings/errores, asincronía).
2. **PRIORIDAD 2:** Robustez de arquitectura, async/await, seguridad, verificación y rollback.
3. **PRIORIDAD 3:** Funciones futuras de producto (Presets, Monitor, Lanzador de juegos, Presión de RAM).
4. **PRIORIDAD 4:** Funciones experimentales o de ecosistema avanzado (WPC CLI, BOOTBOX OS, OPTIMOS PRO).

> **Regla de desarrollo:** No se introducirán funcionalidades nuevas dentro de la fase de estabilización activa de Beta 1. Las funciones futuras permanecerán en el producto como backlog/roadmap hasta que se cierre la etapa de estabilización.

---

## 🏷️ Leyenda de Clasificación

* 🟢 **IMPLEMENTADO:** Confirmado y funcional en el código fuente.
* 🟡 **PARCIAL:** Presente en el código pero incompleto o en proceso de integración.
* 🔴 **CRÍTICO:** Corrección o requerimiento urgente para estabilidad/seguridad.
* ⚪ **PLANIFICADO:** Diseñado e identificado para una etapa concreta próxima.
* 🔵 **FUTURO:** Idea o funcionalidad prevista para etapas posteriores.
* 🧪 **EXPERIMENTAL:** Concepto en fase de investigación o prueba de concepto.

---

## Etapa 0 — Base Existente

- 🟢 Transición desde la generación anterior.
- 🟢 Aplicación WPF sobre .NET 8.
- 🟢 Navegación principal y shell de aplicación.
- 🟡 Monitorización en tiempo real (CPU, RAM, Disco, Red).
- 🟡 Mantenimiento y herramientas básicas del sistema.
- 🟡 Gestión de aplicaciones e inicio automático.
- 🟡 Herramientas avanzadas (SFC, DISM, Puntos de Restauración).
- 🟡 Historial, logs y ajustes generales de la aplicación.

---

## Etapa 1 — Beta 1 / Estabilización y Calidad (Prioridad 1 y 2)

**Estado actual (En desarrollo activo).**

Objetivo: Refactor visual, rendimiento, asincronía y cero regresiones.

* 🟢 Expansión y contracción de ventana sin bloqueos por logo.
* 🔴 Reducción de advertencias del compilador hasta llegar a cero (0 advertencias, 0 errores).
* 🔴 Refactor visual en `App.xaml` y consolidación de contratos entre `Theme.Dark.xaml` y `Theme.Light.xaml`.
* 🔴 Asincronía completa (`async/await`) en operaciones de WMI, PowerShell y accesos a disco para evitar congelamientos de UI.
* 🔴 Protección de procesos críticos del sistema contra cierre accidental.
* 🟡 Flujo universal de acciones (*Detectar → Analizar → Explicar → Recomendar → Confirmar → Actuar → Verificar → Informar*).
* 🟡 Diagnóstico de almacenamiento SMART con fallback adecuado cuando la lectura sea incompleta.
* ⚪ Limpieza básica de temporales con vista previa y selección explícita por el usuario.
* ⚪ Gestión de inicio, bloatware y winget con trazabilidad y presentación transparente de resultados.

---

## Etapa 2 — Salida de Beta y Consolidación (Prioridad 2)

Objetivo: Lanzamiento de la primera versión estable de WPC-SutilBox General.

* ⚪ Consolidación de WPC-SutilBox General.
* ⚪ Documentación técnica y de usuario completa y sincronizada con el código.
* ⚪ Separación clara de experiencia y capacidades entre versión Portable (autocontenida) e Instalada.
* ⚪ Mecanismos de verificación post-acción (comprobar resultados tras aplicar una optimización).
* ⚪ Diálogos de consentimiento y confirmación explícita para acciones sensibles en el Registro de Windows o Servicios.
* ⚪ Publicación de ejecutable estable con empaquetado autocontenido.

---

## Etapa 3 — Evolución Futura Cercana (Prioridad 3)

Funcionalidades de producto orientadas a ampliar el control, la visibilidad y el mantenimiento comprensible.

* 🔵 **Presets Transparentes:**
  * Categorización en perfiles (**SEGURO**, **MEDIO**, **EXTREMO**).
  * Desglose explícito de los ajustes incluidos/excluidos antes de aplicar.
  * Vista previa (preview), estimación de riesgo e impacto por configuración.
  * Verificación posterior a la aplicación y capacidad de *Rollback* (reversión) cuando sea técnicamente posible.
* 🔵 **Reaplicar Ajustes tras Windows Update:**
  * Detección de configuraciones o servicios revertidos por actualizaciones del sistema operativo.
  * Comparación de estado esperado vs. estado actual.
  * Reporte al usuario y opción de reaplicar de forma selectiva (sin modificaciones silenciosas).
* 🔵 **Gestión Inteligente de Presión de RAM:**
  * Detección de presión real de memoria (evitando limpiadores de RAM placebo).
  * Acciones respaldadas técnicamente (ej. gestión de standby list o caché bajo saturación crítica).
  * Explicación clara al usuario del motivo y la acción ejecutada.
* 🔵 **Monitor Flotante Ultraligero:**
  * Widget de métricas (CPU, GPU, RAM, Red, Almacenamiento y Temperaturas).
  * Transparencia, posición y visibilidad configurables.
  * Arquitectura enfocada en bajo consumo de recursos de CPU/RAM.
* 🔵 **Captura Sutil:**
  * Captura rápida del estado del sistema o pantalla mediante atajo global.
  * Utilidad directa para diagnóstico, soporte o asociación futura con archivos de log.
* 🔵 **Historial Continuo e Historial de Cambios:** Registro persistente de operaciones para usuarios de la versión instalada.

---

## Etapa 4 — Especialización Gaming y Rendimiento (Prioridad 3)

Herramientas avanzadas de medición, optimización contextual y monitoreo para juegos.

* 🔵 **Overlay Gaming:**
  * Capa de telemetría en juego (FPS, CPU, GPU, RAM, Temperaturas).
  * Atajo global para mostrar/ocultar, ajuste de opacidad y posición.
* 🔵 **Benchmark Integrado:**
  * Medición de tasa de cuadros (FPS promedio, máximo, mínimo y 1% lows).
  * Módulo de comparación de rendimiento antes y después de aplicar una optimización.
  * Reportes de resultados exportables y comprensibles.
* 🔵 **Lanzador de Juegos Controlado:**
  * Detección de juegos instalados y plataformas principales (Steam, Epic Games, GOG, etc.).
  * Capacidad de añadir ejecutables manualmente.
  * Lanzamiento de juegos bajo perfiles de recursos específicos desde WPC-SutilBox.
* 🔵 **Perfiles por Juego:**
  * Asignación de prioridad de proceso y configuraciones específicas por título.
  * Restauración automática de la configuración previa al cerrar el juego.
  * Eliminación de tweaks universales invasivos o innecesarios.
* 🔵 **Modo Gamer Medible:** Perfil de rendimiento completamente reversible y validado mediante datos reales de benchmark.

---

## Etapa 5 — Especialización Avanzada y Futuro Lejano (Prioridad 4)

* 🔵 **Alertas Inteligentes de Rendimiento:** Detección contextual de comportamientos anómalos o fugas de recursos con notificación y recomendaciones al usuario (sin acciones automáticas no confirmadas).
* 🔵 **Herramientas Developer:** Módulos de diagnóstico y mantenimiento especializado para entornos de desarrollo y técnicos.
* 🧪 **Ecosistema WPC:**
  * **WPC CLI:** Interfaz por línea de comandos para automatización y usuarios avanzados.
  * **WPC BOOTBOX OS:** Entorno independiente de mantenimiento y recuperación del sistema.
  * **WPC OPTIMOS PRO:** Evolución futura o versión especializada dentro del ecosistema WPC.

---

## 💡 Referencia Externa de Inspiración (ASERE)

Como parte de la investigación de mercado y análisis de soluciones existentes en la comunidad, se toman como **referencia e inspiración técnica** algunos conceptos observados en la suite *ASERE*. 

**Aclaración de Identidad:** WPC-SutilBox no copia productos ni adopta filosofías ajenas. Todas las ideas adaptadas de ASERE se filtran estrictamente bajo la identidad de WPC-SutilBox:
* **Transparente y Explicable:** Sin ejecuciones a ciegas.
* **Sin Placebo:** Basado exclusivamente en evidencia y cambios verificables.
* **Ligero y Reversible:** Respetando la estabilidad del sistema y permitiendo deshacer cambios.
* **Open Source / Gratuito:** Fiel al modelo de distribución y principios del proyecto.

### Mapeo de Ideas Inspiradas:
1. **Reaplicación tras Windows Update** $\rightarrow$ Convertido en módulo de auditoría de estado esperado vs. actual.
2. **Presets por Niveles** $\rightarrow$ Implementados como perfiles transparentes con previa vista de cambios y riesgo.
3. **Overlay / Monitor Flotante** $\rightarrow$ Adaptado como widget ultraligero de bajo impacto para telemetría.
4. **Optimización de Memoria** $\rightarrow$ Transformado en gestión de presión de RAM libre de trucos o vaciados de caché destructivos.