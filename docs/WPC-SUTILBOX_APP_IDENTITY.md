# WPC-SUTILBOX — APP IDENTITY

**Documento:** Identidad maestra de la aplicación  
**Versión:** 0.1  
**Estado:** Borrador aprobado para revisión  
**Producto:** WPC-SutilBox  
**Prioridad actual:** SUTILBOX General  
**Última actualización:** 2026-08-12

---

## 1. Propósito de este documento

Este documento define la identidad, propósito, filosofía, límites y principios fundamentales de WPC-SutilBox.

No es un documento de arquitectura ni un plan de desarrollo. Su función es establecer la **fuente de verdad del producto** para evitar que el proyecto pierda su dirección a medida que crezca, se incorporen colaboradores o se utilicen herramientas de IA durante su desarrollo.

Toda nueva función, cambio de diseño o decisión de producto debe ser compatible con esta identidad.

### Jerarquía documental

- **APP IDENTITY** → quién es SUTILBOX, por qué existe, para quién, principios, límites y visión.
- **PRODUCT BLUEPRINT** → qué producto se está construyendo.
- **ARCHITECTURE** → cómo está construido técnicamente.
- **PLAN** → qué se está haciendo ahora, qué está hecho y qué queda pendiente.

La identidad define la dirección. La arquitectura define la implementación. El plan define el presente del desarrollo.

---

# 2. ¿Qué es WPC-SutilBox?

**WPC-SutilBox es una plataforma integral para el cuidado del PC.**

No es únicamente un optimizador de Windows, un limpiador, un administrador de procesos ni una colección de trucos.

SUTILBOX busca permitir que cualquier usuario pueda:

- conocer mejor su PC;
- entender qué está ocurriendo en su sistema;
- mantener Windows;
- optimizar el uso de sus recursos;
- detectar problemas;
- tomar decisiones informadas;
- ejecutar acciones de mantenimiento y optimización;
- y prolongar la vida útil de su equipo.

La aplicación debe servir tanto a PCs antiguos o de pocos recursos como a equipos modernos y potentes.

---

# 3. ¿Por qué existe SUTILBOX?

La idea nace de una experiencia práctica con herramientas y aplicaciones que prometen mejorar el rendimiento u optimizar Windows, pero presentan problemas recurrentes:

1. Exigen pagos para acceder a funciones importantes.
2. Instalan más software del necesario para realizar tareas que deberían poder hacerse directamente.
3. Son complicadas de utilizar o no explican adecuadamente lo que hacen.
4. En muchos casos no queda claro para el usuario si realmente se produjo una mejora.

SUTILBOX nace como una alternativa basada en una premisa diferente:

> **Una herramienta de optimización debe aportar una solución real, no simplemente ocupar espacio, cobrar por funciones básicas o ejecutar acciones cuyo resultado el usuario no puede comprender.**

---

# 4. ¿Para qué existe?

SUTILBOX existe para que los usuarios de PC puedan **conocer, mantener y optimizar su equipo sin tener que pagar por una colección de herramientas ni buscar una aplicación diferente para cada necesidad**.

Su propósito no es hacer modificaciones por el simple hecho de modificar.

Su propósito es ayudar a que el PC:

- funcione de forma más eficiente;
- utilice sus recursos de manera adecuada;
- permanezca mantenible;
- sea comprensible para su propietario;
- y pueda seguir siendo útil durante más tiempo.

---

# 5. La promesa fundamental

SUTILBOX no debe prometer resultados que no pueda demostrar.

Su principio central es:

> **Si SUTILBOX hace algo, el usuario debe poder entender qué hizo, por qué lo hizo y cuál fue el resultado.**

SUTILBOX debe favorecer resultados verificables sobre mensajes de marketing o métricas artificiales.

---

# 6. ¿Qué significa “Sutil”?

“Sutil” no significa que la aplicación haga poco.

Significa que hace mucho **sin convertirse en una carga para el sistema ni exigir atención constante al usuario**.

La idea puede resumirse mediante la analogía del agua:

> **Como el agua: sabes que está ahí, la utilizas cuando la necesitas y no tienes que pensar constantemente en ella. Pero cuando falta, notas inmediatamente su importancia.**

SUTILBOX debe aspirar a una presencia similar:

**Potente por dentro.  
Simple por fuera.  
Transparente en sus acciones.  
Ligera en su presencia.**

La aplicación debe integrarse progresivamente con Windows sin convertirse en una molestia.

---

# 7. ¿Qué significa optimizar?

Definición oficial:

> **Optimizar es conseguir que el PC utilice sus recursos de forma más eficiente, eliminando cargas innecesarias sin sacrificar estabilidad, funcionalidad ni seguridad.**

Por tanto:

**Optimizar no significa simplemente desactivar cosas.**

Una modificación solo debe considerarse una optimización cuando existe una razón válida y un beneficio razonable o verificable.

Si una modificación no aporta un beneficio real, SUTILBOX debe poder recomendar no realizarla.

---

# 8. Principio operativo universal

Toda función que interactúe con el sistema debe intentar seguir este ciclo:

> **DETECTAR → ANALIZAR → EXPLICAR → RECOMENDAR → CONFIRMAR → ACTUAR → VERIFICAR → INFORMAR**

- **DETECTAR:** identificar un problema, oportunidad de mantenimiento o situación relevante.
- **ANALIZAR:** determinar qué está ocurriendo y evitar conclusiones basadas únicamente en una señal aislada.
- **EXPLICAR:** comunicar al usuario qué se detectó y por qué importa.
- **RECOMENDAR:** proponer una acción apropiada, incluyendo cuando corresponda la recomendación de no hacer nada.
- **CONFIRMAR:** solicitar conocimiento y consentimiento del usuario cuando la acción lo requiera.
- **ACTUAR:** ejecutar la operación solicitada.
- **VERIFICAR:** comprobar, cuando sea posible, que la operación produjo el resultado esperado.
- **INFORMAR:** comunicar claramente qué ocurrió y cuál fue el resultado.

Este ciclo es un principio transversal de diseño de SUTILBOX.

---

# 9. Usuario y niveles de experiencia

SUTILBOX está dirigido principalmente al usuario cotidiano de PC, pero no debe excluir a usuarios avanzados ni técnicos.

### Usuario normal
Debe poder entender qué está ocurriendo, recibir recomendaciones claras y ejecutar acciones seguras sin conocimientos especializados.

### Usuario avanzado
Debe disponer de mayor información, mayor control y opciones adicionales.

### Técnico
Debe poder acceder a información y operaciones avanzadas sin una simplificación innecesaria.

Principio:

> **No ocultar la complejidad: presentarla en el nivel adecuado.**

---

# 10. Profundidad sobre Windows

SUTILBOX puede trabajar profundamente con Windows cuando exista una razón válida. Esto puede incluir servicios, registro, tareas programadas, procesos, almacenamiento, memoria, CPU, configuraciones avanzadas, componentes del sistema y otras áreas.

Sin embargo:

> **Profundidad técnica no significa opacidad para el usuario.**

Las operaciones importantes deben realizarse con conocimiento y consentimiento del usuario.

---

# 11. Relación con Windows

SUTILBOX no existe para reemplazar Windows.

Su relación con Windows es complementaria y de mejora:

> **SUTILBOX ayuda al usuario a comprender, mantener y optimizar Windows y su PC.**

Cuando Windows ya ofrece una solución adecuada, SUTILBOX no debe reinventarla innecesariamente.

Cuando una función de Windows es difícil de comprender o administrar, SUTILBOX puede convertirla en una experiencia más clara.

---

# 12. PCs antiguos y PCs modernos

Uno de los motivos principales del proyecto es ofrecer soluciones reales a usuarios que trabajan con PCs antiguos o de pocos recursos.

Sin embargo, SUTILBOX no debe convertirse en una aplicación exclusivamente orientada a equipos antiguos.

También debe aportar valor a equipos modernos y de alto rendimiento.

> **Mejorar la eficiencia del equipo que el usuario tiene, no asumir que todos los equipos tienen el mismo problema.**

---

# 13. Rendimiento y alertas

SUTILBOX debe poder detectar situaciones de rendimiento inusuales.

Por ejemplo, cuando un proceso consume una cantidad anormal de CPU, memoria u otros recursos, la aplicación puede informar al usuario del proceso responsable y ofrecer acciones apropiadas.

El objetivo no es matar procesos indiscriminadamente.

El objetivo es ayudar al usuario a **entender qué está consumiendo sus recursos y decidir qué hacer**.

---

# 14. Seguridad de las acciones

La capacidad técnica de realizar una operación no significa que deba realizarse automáticamente.

SUTILBOX debe distinguir entre acciones seguras, sensibles, avanzadas y potencialmente destructivas.

Principio:

> **Cuanto mayor sea el impacto potencial de una acción, mayor debe ser la claridad y el control entregados al usuario.**

Cuando sea técnicamente viable, las acciones sensibles deben contemplar mecanismos de verificación y/o reversión.

---

# 15. Transparencia

SUTILBOX debe explicar sus acciones mediante textos pequeños, claros y contextualizados.

El usuario debe poder entender:

- qué está haciendo la aplicación;
- por qué lo está haciendo;
- qué puede cambiar;
- y qué resultado obtuvo.

La interfaz no debe esconder el trabajo real detrás de animaciones, porcentajes arbitrarios o mensajes vacíos.

---

# 16. El enemigo conceptual

SUTILBOX existe, en parte, como respuesta a patrones perjudiciales:

1. **No hacer lo que promete.**
2. **Instalar más cosas para hacer lo mismo.**
3. **Hacer cosas sin que el usuario sepa si funcionaron.**
4. **Optimización placebo.**
5. **Consumo innecesario de recursos.**

Una aplicación destinada a mejorar el rendimiento no debe convertirse ella misma en una carga relevante.

---

# 17. Filosofía frente a la obsolescencia

SUTILBOX busca contribuir a prolongar la vida útil de los PCs y reducir la obsolescencia innecesaria mediante mantenimiento, optimización, diagnóstico, conocimiento del sistema y utilización eficiente de los recursos existentes.

La visión no consiste en afirmar que cualquier PC puede mantenerse indefinidamente.

Consiste en defender la idea de que:

> **Un equipo no debería considerarse inútil simplemente porque es antiguo si todavía puede cumplir adecuadamente su función.**

Una aspiración representativa del proyecto es:

> **“Gracias a esta aplicación, mi PC puede seguir funcionando bien aunque tenga muchos años.”**

---

# 18. Qué SUTILBOX NO debe convertirse

SUTILBOX no debe convertirse en:

- un antivirus;
- un limpiador agresivo del registro;
- un “tweaker” lleno de trucos placebo;
- una aplicación llena de publicidad;
- un producto freemium;
- una suite que instala otras suites;
- un programa que consume más recursos de los que ahorra;
- un software que promete mejoras que no puede demostrar;
- un programa que modifica el sistema sin explicar lo que hace;
- un software que obliga al usuario a instalar componentes innecesarios;
- una colección de botones que ejecutan comandos sin contexto;
- una aplicación que trata todos los PCs como si fueran iguales;
- una aplicación que considera “más cambios” igual a “más optimización”.

Principio adicional:

> **SUTILBOX nunca debe optimizar por optimizar.**

Si el sistema funciona correctamente y una modificación no aporta un beneficio real, la aplicación debe poder recomendar **no cambiarla**.

---

# 19. Modelo de distribución y filosofía económica

SUTILBOX es concebido como un proyecto **gratuito** y con filosofía **open source**.

La licencia open source definitiva todavía debe decidirse.

La preocupación de que terceros puedan tomar el proyecto y venderlo como propio queda registrada como una decisión pendiente de licencia. No debe inventarse ni asumirse una licencia en otros documentos hasta que sea aprobada.

---

# 20. Producto prioritario

Actualmente el foco es exclusivamente:

> **WPC-SutilBox / SUTILBOX General**

Hasta que SUTILBOX salga de la etapa Beta y alcance una versión estable, las demás ideas no deben desplazar el foco principal.

---

# 21. Líneas futuras y conceptos separados

Han surgido otros conceptos relacionados con WPC:

- **WPC CLI** — concepto orientado a usuarios avanzados que utilizan directamente la terminal.
- **WPC BOOTBOX OS** — concepto de entorno de mantenimiento/recuperación.
- **WPC OPTIMOS PRO** — concepto relacionado con una posible aplicación comercial o evolución.

Estos conceptos son **ideas documentadas, no productos actuales del código de SUTILBOX**.

No deben tratarse como funcionalidades existentes.

---

# 22. Instalación y modalidad autocontenida

Actualmente existen variantes portable/autocontenida.

La modalidad autocontenida se utilizará como referencia para probar SUTILBOX sin requerir una instalación tradicional.

La versión instalada tendrá como objetivo proporcionar una integración más profunda y sutil con Windows cuando existan funciones que se beneficien de una presencia persistente.

La instalación no debe convertirse en una excusa para agregar software innecesario.

---

# 23. Éxito

El éxito de SUTILBOX no se define únicamente por ingresos.

La visión de éxito es:

- adopción masiva;
- reconocimiento del proyecto;
- usuarios de diferentes países;
- colaboradores internacionales;
- traducciones a diferentes idiomas;
- pruebas realizadas por comunidades;
- utilización real por personas con diferentes tipos de PC.

La aspiración es que SUTILBOX pueda ser utilizado y comprendido independientemente del idioma del usuario.

---

# 24. Comunidad y colaboradores

SUTILBOX debe poder ser entendido por usuarios, técnicos, desarrolladores, traductores, colaboradores y herramientas de IA utilizadas durante su desarrollo.

La documentación debe permitir que alguien que llegue al proyecto entienda:

1. qué es;
2. por qué existe;
3. qué problema intenta resolver;
4. qué principios debe respetar;
5. qué no debe hacer;
6. hacia dónde se dirige.

---

# 25. Regla para futuras decisiones

Ante cualquier nueva función, diseño, optimización o propuesta, debe preguntarse:

- ¿Aporta una solución real?
- ¿El usuario puede entender qué hace?
- ¿Puede entender por qué se recomienda?
- ¿Existe consentimiento cuando corresponde?
- ¿Podemos verificar el resultado?
- ¿La función aporta valor a un PC real?
- ¿Consume recursos de manera razonable?
- ¿Es coherente con el concepto de SUTIL?
- ¿Está intentando hacer demasiado sin una necesidad real?
- ¿Convierte SUTILBOX en algo que explícitamente decidimos no ser?

Si una propuesta contradice los principios fundamentales de esta identidad, debe revisarse antes de incorporarse.

---

# 26. Principio maestro

> ## **SUTILBOX no existe para hacer más cambios en tu PC. Existe para hacer que tu PC funcione mejor y que tú entiendas por qué.**

Y su filosofía operativa:

> **Detectar → Analizar → Explicar → Recomendar → Confirmar → Actuar → Verificar → Informar.**

---

# 27. Visión

SUTILBOX aspira a convertirse en una herramienta de referencia para el cuidado y mantenimiento de PCs, accesible para usuarios cotidianos y suficientemente profunda para técnicos.

La visión de largo plazo es que una persona pueda utilizar SUTILBOX en un PC nuevo, antiguo, potente o limitado y encontrar una herramienta que:

- no le cobre por lo básico;
- no le instale basura;
- no le prometa milagros;
- no esconda sus acciones;
- no lo trate como si no entendiera su propio equipo;
- y realmente le ayude a mantenerlo útil.

> **SUTILBOX debe hacer mucho, pero sentirse poco.**

Ese es el significado de SUTIL.
