# Historial de Cambios — WPC-SutilBox

## [Unreleased] - Beta 1 (Fase de Estabilización)
### En Proceso
* Reestructuración completa de la documentación técnica hacia el modelo de Fuente Única de Verdad.
* Corrección de bindings en la UI del Dashboard y optimización del hilo principal (UI Thread).
* Aislamiento de llamadas WMI en servicios asíncronos.

---

## [0.2.0-beta.1] - 2026-03 (⚠️ PENDIENTE DE VERIFICACIÓN HISTÓRICA)
### Añadido
* Prototipo inicial de Dashboard en WPF con métricas básicas de CPU y RAM.
* Estructura modular de servicios para lectura de Registro.
* Módulo preliminar de análisis de componentes de hardware.

### Corregido
* Excepciones al intentar consultar claves de Registro restringidas sin elevación UAC.

---

## [0.1.0-alpha] - Versión Inicial
* Creación de la solución base en C# / WPF.
* Definición de la arquitectura MVVM inicial.