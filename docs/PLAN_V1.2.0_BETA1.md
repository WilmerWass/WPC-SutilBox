# WPC SUTILBOX — Plan de Trabajo v1.2.0-beta.1

**Estado:** Beta en estabilización y refactor visual  
**Objetivo:** mejorar claridad, consistencia, seguridad y experiencia sin romper las funciones existentes.

## 1. Objetivos

1. Orden y organización visual.
2. Facilidad de uso.
3. Claridad de acciones y estados.
4. Rendimiento y fluidez.
5. Seguridad de acciones sensibles.
6. Consistencia entre secciones.
7. Mantener el foco en SUTILBOX General.

## 2. Estado actual

### Verificado recientemente

- La ventana principal puede expandirse y contraerse correctamente.
- Se corrigió el problema en el que el logo impedía recuperar correctamente la vista expandida.
- Se retiró el logo de la zona problemática del shell.
- `dotnet build` termina correctamente.
- El build actual presenta **3 advertencias únicas**:
  - `CS0105` por `using` duplicado de `Wpc_SutilBox.Core`.
  - `CS0105` por `using` duplicado de `Wpc_SutilBox.Models`.
  - `CS4014` por una llamada no esperada en `ProfileEditorViewModel.cs`.
- `dotnet run` desde la consola puede requerir elevación en el entorno actual; esto no debe confundirse con un fallo de compilación.

## 3. Regla de desarrollo

Después de cada bloque importante:

1. revisar el diff;
2. compilar;
3. ejecutar las pruebas disponibles;
4. comprobar visualmente cuando corresponda;
5. registrar el resultado;
6. crear un commit lógico cuando el bloque esté terminado.

## 4. Refactor visual

### App.xaml

- reducir duplicación de recursos;
- centralizar tokens;
- mantener convertidores globales;
- separar responsabilidades entre recursos globales y temas;
- evitar referencias a recursos inexistentes.

### Theme.Dark.xaml / Theme.Light.xaml

- mantener el mismo contrato de recursos;
- centralizar colores y estados;
- evitar divergencias entre temas;
- usar nombres semánticos.

### Shell

- navegación clara;
- expansión/contracción estable;
- evitar que logos u otros elementos bloqueen el cambio de tamaño;
- mantener el contenido visible en ambos estados.

## 5. Rendimiento

- evitar bloqueos de UI;
- reducir actualizaciones innecesarias;
- mantener monitorización eficiente;
- no permitir que SUTILBOX consuma recursos de forma desproporcionada.

## 6. Seguridad

- proteger procesos críticos;
- explicar acciones sensibles;
- evitar operaciones destructivas ambiguas;
- conservar restauración/reversión cuando exista;
- registrar operaciones importantes.

## 7. Almacenamiento

- distinguir datos disponibles de datos no disponibles;
- mejorar presentación de discos;
- mantener diagnóstico SMART sin afirmar salud cuando la lectura sea incompleta;
- usar flujo analizar → revisar → confirmar → ejecutar.

## 8. Aplicaciones

- organizar inicio, bloatware y actualizaciones;
- revisar acciones antes de ejecutarlas;
- no instalar software adicional por defecto;
- mostrar origen y resultado de las operaciones.

## 9. Historial

- registrar acciones y resultados;
- facilitar diagnóstico;
- conservar trazabilidad;
- distinguir éxito, advertencia y error.

## 10. Criterio de cierre de Beta

La versión candidata debe:

- ser compilable;
- mantener navegación estable;
- conservar las funciones existentes que funcionan;
- evitar regresiones;
- explicar acciones sensibles;
- mantener una UI consistente;
- reducir las advertencias del compilador hasta llegar a cero como objetivo de calidad;
- pasar una revisión visual y funcional completa.

## 11. No hacer todavía

Mientras SUTILBOX General siga en Beta, no desplazar el foco para desarrollar WPC CLI, BOOTBOX OS u OPTIMOS PRO.

Esas ideas permanecen documentadas como futuro.
