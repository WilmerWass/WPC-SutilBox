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

