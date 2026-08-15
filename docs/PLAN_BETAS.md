# Plan Maestro de Desarrollo (PLAN_BETAS)

Este documento es la **ÚNICA fuente oficial de planificación** de WPC-SutilBox. Reemplaza todos los roadmaps y planes anteriores.

---

## Leyenda de Estados
* 🟢 **IMPLEMENTADO:** Verificado en código y funcional.
* 🟡 **PARCIAL:** Funcionalidad parcialmente escrita o con bugs conocidos.
* 🔴 **BLOQUEANTE / CRÍTICO:** Falla prioritaria que impide cierre de versión.
* ⚪ **PLANIFICADO:** Diseñado e incluido en el scope de una Beta específica.
* 🔵 **FUTURO:** Idea o concepto para fases posteriores.
* 🧪 **EXPERIMENTAL:** En pruebas de concepto, no apto para producción.
* ⚠️ **PENDIENTE DE VERIFICACIÓN:** Requiere auditoría en código o pruebas físicas.

---

## ESTADO ACTUAL DEL PROYECTO
Nos encontramos oficialmente en **BETA 1 (Fase de Estabilización y Calidad)**.

---

## 🚀 BETA 1: Estabilización, Robustez y Calidad Visual

### Objetivo
Llegar a un binario 100% estable, libre de excepciones no controladas, con bindings corregidos en la interfaz y sin bloqueos de rendimiento.

### Alcance & Tareas Integradas (Fuente: Auditoría PC_REVIEW 1.2)
1. **Binding & UI Stability**
   * 🔴 Corrección de DataBindings caídos en Vistas del Dashboard (`ANOSUBIR/BETA_1/1.2_PC_REVIEW/LOG_CORRECCIONES_PC2.md`).
   * 🔴 Garantizar sincronización fluida entre ViewModels y controles XAML sin parpadeos ni "freezes".
2. **Procesos Asíncronos & Thread Safety**
   * 🔴 Mover todas las llamadas intensivas de WMI y Registry fuera del hilo de UI (`Task.Run` / `async-await`).
   * 🟡 Normalizar manejo de excepciones durante fallos de permisos UAC.
3. **Diagnóstico e Inspección del Sistema**
   * 🟢 Lectura básica de CPU, RAM y discos.
   * 🟡 Corrección de fallos en lectura de temperaturas en hardware específico (⚠️ PENDIENTE DE VERIFICACIÓN).
4. **Limpieza de Proyecto**
   * 🟡 Eliminación de Warnings de compilación en Visual Studio / .NET CLI.
   * 🟢 Eliminación de dependencias obsoletas o duplicadas.

### Criterios de Cierre de Beta 1
* Cero errores críticos de binding en runtime.
* Cero congelamientos del hilo principal comprobados en pruebas de carga.
* Tasa de advertencias (warnings) de compilación = 0.

---

## 🛠️ BETA 2: Tweaks, Presets y Reversibilidad Avanzada

### Objetivo
Expandir el catálogo de optimizaciones asegurando perfiles transparentes y re-aplicación automática.

* ⚪ **Presets de Optimización (SEGURO / MEDIO / EXTREMO):** Perfiles de configuración predefinidos pero auditables.
* ⚪ **Módulo Reapply Tweaks:** Detección de cambios provocados por Windows Update y re-aplicación a demanda del usuario.
* ⚪ **Gestión Ampliada de Servicios:** Limpieza automatizada y segura de servicios innecesarios.

---

## 🎮 BETA 3: Monitorización Avanzada y Perfiles

### Objetivo
Proporcionar métricas avanzadas orientadas a rendimiento e integración de periféricos/juegos.

* 🔵 **Monitor Flotante / Overlay Gaming:** Widget sutil con consumo de recursos durante ejecución de juegos.
* 🔵 **Perfiles Automáticos por Juego:** Cambio de perfil de energía y servicios según el ejecutable activo.
* 🧪 **Gestión Inteligente de Presión RAM:** Algoritmo no agresivo para liberación de memoria en picos de consumo.

---

## 🔮 FUTURO / CONCEPTOS EN EVALUACIÓN

* 🔵 **WPC CLI:** Interfaz de línea de comandos para administradores de sistemas.
* 🔵 **Captura Sutil:** Herramienta ligera de reporte de estado del sistema.
* 🔵 **WPC BOOTBOX OS / WPC OPTIMOS PRO:** Conceptos de entornos especializados fuera del alcance del ejecutable estándar.