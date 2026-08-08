\# WPC SUTILBOX — Plan de Trabajo v1.2.0-beta.1



\## 1. Objetivo



La versión `v1.2.0-beta.1` tiene como objetivo mejorar la experiencia general de WPC SUTILBOX, priorizando:



1\. Orden y organización visual.

2\. Facilidad de uso.

3\. Claridad de las acciones y estados.

4\. Rendimiento y fluidez.

5\. Seguridad al ejecutar acciones sensibles.

6\. Consistencia visual entre las diferentes secciones.



Los cambios se realizarán de forma incremental, evitando modificaciones innecesarias de la lógica existente.



\## 2. Punto de partida



\- Rama: `main`

\- Commit base: `239b17a`

\- Última versión estable etiquetada: `v1.1.8`

\- Estado inicial: `working tree clean`



\## 3. Reglas de desarrollo



\### 3.1 Cambios pequeños



Cada cambio deberá representar una unidad lógica de trabajo.



\### 3.2 Verificación



Después de cada cambio se deberá:



1\. Revisar el diff.

2\. Compilar el proyecto.

3\. Ejecutar las pruebas disponibles.

4\. Comprobar visualmente el cambio cuando corresponda.



\### 3.3 Commits



Cada unidad terminada tendrá su propio commit descriptivo.



\### 3.4 Seguridad



No se modificará código funcional sin necesidad.



Las acciones potencialmente destructivas deberán conservar o mejorar sus mecanismos de protección.



\### 3.5 Registro



Los cambios realizados deberán quedar registrados para mantener trazabilidad durante toda la beta.



\## 4. Criterio general de aceptación



La versión `v1.2.0-beta.1` deberá presentar una interfaz más ordenada, consistente y fácil de utilizar, manteniendo las funciones existentes que ya funcionan correctamente y evitando regresiones.

## 5. Fases de trabajo

### 5.1 Sistema visual

#### 5.1.1 Tokens visuales
- Normalizar colores.
- Normalizar superficies.
- Normalizar textos.
- Normalizar bordes.
- Definir estados: información, éxito, advertencia, error y sin datos.
- Reducir valores visuales definidos directamente dentro de las vistas.

#### 5.1.2 Espaciado y alineación
- Establecer una escala consistente de espaciado.
- Normalizar márgenes.
- Normalizar padding.
- Alinear correctamente títulos, contenido y acciones.
- Evitar elementos innecesariamente pegados a los bordes.

#### 5.1.3 Tipografía
- Establecer jerarquía entre título, sección, tarjeta, contenido y texto secundario.
- Reducir tamaños definidos directamente en las vistas cuando exista un estilo equivalente.

#### 5.1.4 Componentes
- Unificar tarjetas.
- Unificar encabezados de sección.
- Unificar botones.
- Unificar indicadores de estado.
- Mantener los componentes existentes cuando puedan reutilizarse.

### 5.2 Navegación y estructura

#### 5.2.1 Navegación principal
- Revisar la estructura actual.
- Compararla con la estructura definida en el Blueprint.
- Mejorar la jerarquía visual.
- Eliminar elementos heredados de la identidad anterior cuando corresponda.

#### 5.2.2 Shell de la aplicación
- Revisar encabezado.
- Revisar navegación lateral.
- Revisar área de contenido.
- Mantener una estructura visual consistente entre páginas.

### 5.3 Configuración

#### 5.3.1 Organización
- Agrupar opciones relacionadas.
- Separar configuración general, apariencia, comportamiento y opciones avanzadas.
- Reducir la sensación de lista vertical desordenada.

#### 5.3.2 Distribución
- Mejorar alineación entre etiquetas, descripciones y controles.
- Normalizar espacios.
- Mantener acciones y controles en posiciones previsibles.

#### 5.3.3 Usabilidad
- Hacer evidente qué modifica cada opción.
- Mantener los controles existentes.
- Evitar cambios funcionales innecesarios.

### 5.4 Inicio

#### 5.4.1 Jerarquía
- Separar información del sistema, recomendaciones y acciones.
- Reducir la sobrecarga visual.
- Priorizar la información más importante.

#### 5.4.2 Acciones
- Diferenciar acción principal, acciones secundarias y acciones sensibles.
- Evitar presentar demasiadas acciones con el mismo peso visual.

### 5.5 Rendimiento

#### 5.5.1 Métricas
- Reorganizar las métricas principales.
- Mejorar alineación y distribución.

#### 5.5.2 Procesos
- Mejorar la organización de la lista de procesos.
- Mejorar búsqueda y lectura.
- Revisar acciones disponibles.
- Proteger procesos críticos.

#### 5.5.3 Servicios
- Separar visualmente servicios y procesos.
- Mantener las funciones existentes.

#### 5.5.4 Fluidez
- Revisar operaciones que puedan bloquear la interfaz.
- Evitar actualizaciones innecesarias de la UI.
- Mantener la aplicación responsiva.

### 5.6 Almacenamiento

#### 5.6.1 Discos
- Mejorar presentación de unidades.
- Mejorar jerarquía de información.

#### 5.6.2 SMART
- Diferenciar datos disponibles, datos no disponibles y errores.
- Mantener el análisis existente.

#### 5.6.3 Limpieza
- Organizar las acciones de limpieza.
- Evitar acciones destructivas ambiguas.

### 5.7 Aplicaciones

- Mejorar organización visual.
- Revisar distribución de acciones.
- Mantener las funciones existentes.
- Evitar acciones destructivas sin confirmación adecuada.

### 5.8 Protección y seguridad

- Revisar acciones sensibles.
- Mejorar advertencias.
- Mantener mecanismos de protección existentes.
- Mantener reversión/restauración cuando esté disponible.
- Diferenciar advertencia de error.

### 5.9 Herramientas avanzadas

- Separar claramente herramientas avanzadas de acciones normales.
- Mejorar advertencias.
- Evitar ejecuciones accidentales.

### 5.10 Hardware

- Mejorar organización de la información.
- Mantener diagnóstico existente.
- Mejorar presentación de datos no disponibles.
- Mantener privacidad de información sensible.

### 5.11 Historial y registros

- Mejorar organización visual.
- Facilitar lectura de acciones realizadas.
- Mantener trazabilidad.
- Facilitar identificación de errores y acciones reversibles.

### 5.12 QA y estabilización

- Compilar después de cada bloque importante.
- Revisar navegación completa.
- Revisar regresiones.
- Probar acciones modificadas.
- Revisar rendimiento.
- Revisar estados de error y ausencia de datos.
- Preparar el estado candidato para `v1.2.0-beta.1`.

## 6. Reglas de commits

Los commits deberán representar cambios lógicos independientes.

Ejemplos:

- `docs: define v1.2.0-beta.1 work plan`
- `ui: normalize design tokens`
- `ui: improve settings layout`
- `ui: reorganize dashboard`
- `perf: improve process view`
- `fix: protect critical process actions`

No se deberán mezclar cambios funcionales no relacionados dentro de un mismo commit.

## 7. Estado de la versión

### v1.2.0-beta.1

Estado inicial: planificación.

Objetivo de cierre:

- Interfaz reorganizada.
- Navegación consistente.
- Configuración ordenada.
- Inicio simplificado.
- Rendimiento reorganizado.
- Acciones sensibles protegidas.
- Sin regresiones conocidas.
- Proyecto compilable y funcional.
