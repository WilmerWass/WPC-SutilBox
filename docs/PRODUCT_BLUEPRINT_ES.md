# Boceto de producto — Ecosistema WPC

> **Principio rector:** WPC explica cada acción, evita cambios aleatorios y permite revertir cambios. La prioridad es recuperar fluidez y espacio sin poner en riesgo los datos del usuario.

## 1. Productos y límites

| Producto | Público | Propósito | Estado inicial |
|---|---|---|---|
| **WPC SUTILBOX** | Usuario general | Diagnóstico, limpieza segura y preparación inicial de Windows | Producto prioritario y Open Source |
| **WPC BOOTBOX** | Técnico | Guía y menú técnico para USB con MediCat/Hiren | Documentación y estructura; no redistribuir software sin licencia |
| **WPC OPTIMOS PRO** | Técnico avanzado | Automatización, perfiles y reportes avanzados | Fase futura comercial/freemium |
| **AllWass Suite** | Taller | Recepción, historial, presupuesto y reportes | Producto web local independiente, fase futura |

## 2. WPC SUTILBOX: navegación propuesta

```text
Inicio
├── Revisar mi PC
│   ├── Resumen de salud
│   ├── Espacio y almacenamiento
│   ├── Hardware y temperaturas
│   └── Procesos de alto consumo
├── Liberar espacio
│   ├── Limpieza básica
│   ├── Archivos grandes
│   └── Resultados y elementos excluidos
├── Aplicaciones
│   ├── Desinstalar bloatware
│   ├── Instalador Sutil (winget)
│   └── Packs de software
├── Herramientas avanzadas
│   ├── Reparación de Windows
│   ├── Procesos y servicios
│   └── Perfiles (instalado)
├── Historial y seguridad
│   ├── Cambios realizados
│   ├── Restauración / reversión
│   └── Exportar informe
└── Ajustes
    ├── Idioma y apariencia
    ├── Frecuencia de monitoreo
    └── Instalación / actualización
```

## 3. Módulos de SUTILBOX

### Sistema transversal: perfiles y acciones individuales

Los perfiles son **planes de acciones preseleccionadas**, no modos misteriosos que cambian Windows sin explicación. Cada perfil muestra qué activa, qué deja fuera, el nivel de riesgo y si requiere reinicio. El usuario puede desmarcar o añadir acciones antes de aplicarlo.

| Perfil | Objetivo | Acciones iniciales permitidas | Límites |
|---|---|---|---|
| **Revisión segura** | Entender el estado del PC sin modificarlo | Diagnóstico, espacio, SMART, inicio y procesos | Solo lectura |
| **Liberar espacio** | Recuperar almacenamiento de bajo riesgo | Temporales seguros, papelera y cachés seleccionadas | Siempre con vista previa |
| **PC nuevo** | Preparar un equipo recién instalado | Actualizaciones, instalador Sutil y pack elegido | No instala nada sin selección |
| **Oficina** | Priorizar estabilidad y trabajo cotidiano | Apps de oficina elegidas, inicio conservador y limpieza segura | No toca servicios ni seguridad |
| **Developer básico** | Preparar el entorno de desarrollo | VS Code, Git, Node.js LTS y Terminal | Muestra licencias y origen |
| **Gamer (futuro)** | Preparar una sesión de juego de forma reversible | Energía, tareas no críticas y medición antes/después | Sin promesas de FPS ni cambios permanentes |
| **Personalizado** | Guardar una selección del usuario | Acciones elegidas expresamente | Indica incompatibilidades y riesgo |

**Flujo de aplicación de un perfil**

1. Elegir perfil.
2. Revisar las acciones agrupadas por área: limpieza, aplicaciones, inicio, red y rendimiento.
3. Activar o desactivar cada acción; las bloqueadas explican el motivo.
4. Ver resumen: cambios, espacio estimado, reinicios y riesgos.
5. Crear punto de restauración si hay cambios sensibles.
6. Aplicar, registrar resultados y ofrecer revertir lo que sea reversible.

**Diseño de interfaz recomendado**

- La pantalla Inicio mantiene el resumen de salud actual y muestra una tarjeta “¿Qué quieres hacer hoy?”.
- La pantalla **Perfiles** usa tarjetas claras, no nombres como “medio” o “extremo”: `Revisión segura`, `Liberar espacio`, `PC nuevo`, `Oficina` y `Personalizado`.
- Al abrir una tarjeta se llega a una pantalla de revisión con interruptores o casillas por acción, una explicación breve y una etiqueta `Seguro`, `Requiere revisión` o `Avanzado`.
- El botón final dice **“Revisar y aplicar”**, no “Optimizar ahora”.
- La parte inferior conserva una bitácora compacta de acciones y un enlace a detalles, siguiendo la idea de progreso visible sin recargar la interfaz.

**Seguimiento**

- [ ] Diseñar el modelo de datos de una acción: identificador, categoría, descripción, riesgo, reversión, reinicio y requisito de administrador.
- [ ] Crear perfiles iniciales solo con acciones ya probadas.
- [ ] Guardar perfiles personalizados en un archivo exportable.
- [ ] Añadir pruebas para validar que una acción crítica nunca se incluya sin confirmación adicional.

### A. Inicio / resumen de salud

**Contiene**

- Estado general: Bien, Atención o Crítico.
- Espacio libre, uso de RAM/CPU, temperatura y estado del almacenamiento.
- Tres acciones recomendadas como máximo, con explicación concreta.
- Acceso a diagnóstico rápido y a instalar la versión completa.

**No debe hacer**

- Ejecutar una optimización automática al abrir la aplicación.
- Mostrar alarmas genéricas sin explicar la causa.

**Seguimiento**

- [ ] Definir indicadores y umbrales.
- [ ] Diseñar resumen visual.
- [ ] Validar resultados en equipos lentos y equipos sanos.

### B. Diagnóstico básico

**Contiene**

- Procesador, RAM, GPU, placa, BIOS, batería y red.
- Temperaturas, ventiladores y consumo cuando el hardware lo permita.
- Modelo, serial opcional y versión de Windows.
- Informe exportable en Markdown o texto.

**Pendiente crítico**

- Corregir detección de unidades SATA, NVMe/M.2 y lectura SMART. No marcar una unidad como sana o dañada cuando la lectura sea incompleta.

**Seguimiento**

- [ ] Crear matriz de prueba: HDD SATA, SSD SATA, NVMe, USB y equipos sin sensor SMART accesible.
- [ ] Distinguir “sin datos” de “fallo”.
- [ ] Añadir recomendaciones comprensibles para atributos SMART relevantes.

### C. Salud de almacenamiento

**Contiene**

- Capacidad, espacio libre, tipo de unidad y estado SMART.
- Alertas explicadas: sectores reasignados, pendientes y no corregibles.
- Recomendación de copia de datos antes de cualquier operación cuando haya alerta.
- Acceso a herramientas de diagnóstico de Windows, sin formateo ni borrado desde el modo básico.

**Reglas**

- No ejecutar formateos, “low-level format” ni correcciones destructivas.
- Nunca ocultar incertidumbre de la lectura.

### D. Limpieza básica

**Contiene**

- Escaneo de temporales de usuario y sistema, cachés conocidas, papelera y registros de instalación seguros.
- Vista previa por categoría: ruta, cantidad y espacio estimado.
- Exclusiones personalizables.
- Resultado y registro de elementos eliminados.

**Reglas**

- Nada se borra antes de ver y aprobar la selección.
- No borrar Prefetch por defecto.
- No presentar la liberación de RAM como beneficio principal.

**Seguimiento**

- [ ] Definir categorías seguras iniciales.
- [ ] Medir tamaño antes/después.
- [ ] Probar con perfiles de usuario estándar y administrador.

### E. Bloatware y desinstalación

**Contiene**

- Lista de aplicaciones con origen, editor, tamaño y nivel de riesgo.
- Etiquetas: recomendado para quitar, opcional, conservar y sistema.
- Desinstalación una a una o por selección.
- Registro de lo quitado y opción de reinstalar cuando winget/Microsoft Store lo permita.

**Reglas**

- Bloquear aplicaciones y componentes esenciales.
- No eliminar controladores, runtimes ni paquetes de Windows sin advertencia específica.

### F. Instalador Sutil

**Contiene**

- Buscador de aplicaciones verificadas mediante winget.
- Cola secuencial: una instalación a la vez.
- Packs: Esencial, Oficina, Navegación, Gamer y Developer básico.
- Resultado individual, versión instalada, fuente y errores.

**Primer pack Developer**

- Visual Studio Code, Git, Node.js LTS y Windows Terminal.
- Limpieza de caché de npm y VS Code solo como opción visible y confirmada.

**Reglas**

- Usar identificadores oficiales de winget.
- Mostrar licencia y procedencia antes de instalar.
- No instalar software adicional por defecto.

### G. Procesos y servicios — instalado/avanzado

**Contiene**

- Procesos de alto consumo, descripción y editor.
- Clasificación: seguro de cerrar, revisar primero o crítico del sistema.
- Administración conservadora de inicio de Windows.
- Servicios solo con explicación, perfil y reversión.

**Reglas**

- Impedir finalizar procesos críticos de Windows.
- No aplicar cambios masivos de servicios.

### H. Historial y seguridad

**Contiene**

- Bitácora local: acción, fecha, usuario, resultado y elementos afectados.
- Punto de restauración previo a cambios sensibles.
- Exportación/importación de configuración y lista de aplicaciones.
- Botón de revertir cuando sea técnicamente posible.

**Reglas de respaldo**

- Limpieza: vista previa y confirmación.
- Cambios de configuración: punto de restauración o bloquear si no se puede crear.
- Acciones de alto riesgo: confirmar respaldo externo de datos antes de continuar.

## 4. Portátil frente a instalado

| Función | Portable | Instalado |
|---|---|---|
| Resumen de salud y diagnóstico | Sí | Sí |
| Limpieza básica y bloatware | Sí, con vista previa | Sí |
| Informe local | Sí | Sí |
| Instalador Sutil | Botón para abrir/instalar | Sí, con packs y cola |
| Monitoreo en segundo plano | No | Opcional |
| Perfiles, automatizaciones y servicios | No | Sí, con seguridad |
| Actualizaciones y extensiones | Manual | Integradas y controladas |

El modo portátil debe invitar a instalar sin bloquear el diagnóstico: **“Instala WPC SUTILBOX para habilitar automatizaciones, perfiles, historial continuo y actualizaciones.”**

## 5. Módulos futuros

### Gamer

- Línea base: FPS, temperaturas, carga, red y latencia DPC cuando sea posible.
- Perfil reversible de energía y tareas no críticas.
- Comparación antes/después basada en datos.
- No prometer FPS ni “desbloquear núcleos” como efecto universal.

### Developer

- VS Code y Node.js primero.
- Limpieza opcional de cachés, diagnóstico de espacio de proyectos y variables de entorno.
- Añadir Docker, Android Studio y otros IDE según uso y pruebas reales.

### Plugins

- No cargar DLL arbitrarias en la primera versión.
- Primero definir contratos internos estables.
- Después: paquetes firmados con manifiesto, versión, permisos y compatibilidad.

## 6. AllWass Suite: módulos de taller

- Recepción: datos mínimos del cliente, equipo y autorización.
- Checklist: refrigeración, integridad física, puertos, conectividad y software.
- Estado por ítem: Óptimo, Regular o Falla; evidencia y observaciones.
- Historial de intervenciones, repuestos y garantías.
- Presupuesto: revisión, mano de obra y repuestos separados.
- Reporte: Markdown, texto y PDF; el cliente elige qué datos de contacto compartir.

## 7. Orden de entregas

### Hito 1 — Confianza y diagnóstico

1. Corregir almacenamiento SATA/NVMe/SMART.
2. Filtros de seguridad para procesos críticos.
3. Vista previa de limpieza, historial y reglas de respaldo.
4. Diagnóstico rápido portable.

### Hito 2 — Utilidad cotidiana

1. Limpieza básica validada.
2. Bloatware con etiquetas de riesgo.
3. Instalador Sutil con pack Esencial y Oficina.
4. Exportación de informe.

### Hito 3 — Versión instalada

1. Actualizaciones, historial continuo y configuración exportable.
2. Perfiles: Revisión segura, Liberar espacio, PC nuevo, Oficina y Personalizado.
3. Pack Developer básico y procesos/inicio conservadores.

### Hito 4 — Especialización

1. Gamer medible y reversible.
2. Developer ampliado.
3. Diseño de OPTIMOS PRO y AllWass Suite.

## 8. Métricas de éxito

- Diagnóstico de almacenamiento correcto en la matriz de equipos de prueba.
- Espacio liberado explicado y confirmado por el usuario.
- Cero cambios irreversibles sin confirmación explícita.
- Tiempo desde abrir la app hasta recomendación útil: menos de un minuto.
- Instalaciones completadas, fallidas y omitidas registradas individualmente.

## 9. Auditoría visual del diseño actual

La base visual actual es buena: modo oscuro, navegación lateral consistente, tarjetas legibles y un azul de acento reconocible. La evolución debe priorizar claridad para usuarios generales y controles de seguridad antes de añadir más botones.

### Navegación propuesta

| Navegación actual | Destino recomendado | Motivo |
|---|---|---|
| Inicio principal | **Inicio** | Resumen, recomendaciones y perfiles seguros |
| Protección | **Seguridad** | Estado comprensible y enlaces a Windows Security |
| Almacenamiento | **Liberar espacio** | Limpieza, archivos grandes y Descargas |
| Rendimiento | **Rendimiento** | Solo lectura por defecto; procesos y arranque con protección |
| Aplicaciones | **Aplicaciones** | Bloatware, actualizaciones e Instalador Sutil |
| Caja de herramientas | **Herramientas avanzadas** | Ocultar detrás de una advertencia técnica |
| Hardware | **Revisar mi PC** | Hardware, discos, controladores y privacidad como pestañas separadas |
| Configuración | **Ajustes** | Preferencias de aplicación, no optimizaciones automáticas |

### Hallazgos prioritarios

#### P0 — Corregir antes de ampliar funciones

1. **Procesos:** la lista permite detener `explorer` y la propia aplicación. Debe marcar y bloquear procesos esenciales; el botón debe pasar de `DETENER` a `Revisar` para elementos no clasificados. Solo tras una explicación y confirmación se permite finalizar procesos seguros.
2. **Discos/SMART:** el error de WMI mostrado al usuario debe convertirse en un estado no intrusivo: `No se pudo leer SMART en esta unidad` con detalles expandibles, tipo/modelo de unidad y alternativa de diagnóstico. No mostrar una ventana modal al cargar la pantalla.
3. **Limpieza:** `LIMPIAR AHORA`, `LIMPIEZA PROFUNDA` y `TODO` pueden resultar destructivos. Sustituir por `Analizar`, `Revisar selección` y `Eliminar X elementos (Y GB)`. La categoría y cada archivo grande deben requerir selección explícita.
4. **Optimizar en inactividad:** no debe estar activo por defecto. La limpieza automática puede borrar contenido que el usuario necesitaba o sorprenderle. Convertirlo en una programación visible, limitada a categorías elegidas y con historial.

#### P1 — Mejorar comprensión y confianza

1. **Protección:** indicar la causa del riesgo, por ejemplo: `Firewall desactivado — la red pública no está protegida`, y ofrecer una acción concreta: `Abrir Seguridad de Windows`. Evitar el estado ambiguo `RIESGO` junto a `Windows Defender` sin explicación.
2. **Actualizaciones:** cambiar `Actualizar Todo` por una revisión con casillas, origen, versión y resumen de compatibilidad. Tras actualizar, mostrar resultado por aplicación y el registro de winget.
3. **Hardware y privacidad:** separar claramente `Hardware`, `Almacenamiento`, `Controladores` y `Privacidad`. No mezclar la tarjeta de seguridad con datos de hardware; el usuario no sabrá dónde resolver cada problema.
4. **Herramientas avanzadas:** CMD, PowerShell, Registro, servicios y gpedit son útiles para técnicos, pero no deben ser la experiencia principal. Añadir descripción, requisito de administrador y advertencia de impacto antes de abrir Registro o Servicios.
5. **Unidades de medida:** `Impacto disco 87355 I/O` no dice si el valor es bueno o malo. Usar una medida entendible (`87 MB/s`, `alto/bajo`, periodo de medición) y una explicación breve.

#### P2 — Pulido de diseño

1. Sustituir el nombre, título de ventana y logo de WassControl por **WPC SUTILBOX**.
2. Reducir el uso de azul brillante a navegación, acciones principales y enlaces; reservar verde/ámbar/rojo para estados reales de salud.
3. Unificar mayúsculas: botones con texto normal (`Analizar archivos`, `Crear punto de restauración`) en vez de mayúsculas sostenidas.
4. Mantener una acción principal por tarjeta. La pantalla de almacenamiento tiene demasiadas decisiones simultáneas; separar análisis de eliminación.
5. En cuadros de diálogo, explicar la consecuencia y el siguiente paso; evitar modales informativos que corten el flujo si una notificación en la tarjeta basta.

### Rediseño específico por pantalla

| Pantalla actual | Conservar | Cambiar primero |
|---|---|---|
| Protección | Tarjeta de estado y accesos a Defender/red | Motivo del riesgo, acción de solución y estados semánticos |
| Almacenamiento | Categorías de limpieza y exploración de archivos grandes | Flujo analizar → seleccionar → revisar → eliminar; quitar acciones destructivas globales |
| Rendimiento | Métricas superiores y agrupación de procesos | Clasificación de riesgo, bloqueo de procesos críticos y medidas entendibles |
| Aplicaciones | Pestañas Inicio/Bloatware/Actualizaciones | Agregar Instalador Sutil; revisión antes de actualizar todo |
| Caja de herramientas | Accesos útiles y punto de restauración | Mover a modo avanzado y describir cada herramienta |
| Hardware | Pestañas de información y exportación de drivers | Manejo no modal de SMART y separación de Privacidad |
| Configuración | Tema, idioma y bandeja del sistema | Desactivar por defecto la optimización en inactividad y mover perfiles a su propia pantalla |
