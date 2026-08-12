# Blueprint de producto — WPC-SutilBox

**Estado:** Beta / evolución activa  
**Producto prioritario:** WPC-SutilBox General

> **Principio rector:** SUTILBOX debe explicar cada acción, evitar cambios aleatorios y dar al usuario control sobre lo que modifica.

## 1. Definición del producto

WPC-SutilBox es una aplicación para Windows orientada al diagnóstico, mantenimiento, optimización y administración comprensible del PC.

La experiencia debe funcionar para usuarios cotidianos y, mediante áreas avanzadas, para usuarios técnicos.

La aplicación no debe depender de una colección de herramientas externas para realizar las funciones básicas que puede ejecutar directamente.

## 2. Experiencia principal

```text
Inicio
├── Estado general
├── Recomendaciones
└── Acciones principales

Revisar mi PC
├── Resumen de salud
├── Almacenamiento
├── Hardware y temperaturas
└── Procesos de alto consumo

Liberar espacio
├── Limpieza básica
├── Descargas
└── Archivos grandes

Aplicaciones
├── Inicio
├── Bloatware
└── Actualizaciones / Winget

Herramientas avanzadas
├── Reparación de Windows
├── Procesos
├── Servicios
└── Perfiles / utilidades

Historial y seguridad
├── Logs
├── Restauración / reversión
└── Diagnóstico

Ajustes
├── Idioma
├── Apariencia
├── Comportamiento
└── Instalación
```

La estructura exacta puede evolucionar durante la Beta, pero no debe contradecir la identidad del producto.

## 3. Principios funcionales

1. Cada botón debe ejecutar una acción real o navegar a una vista funcional.
2. Las acciones sensibles deben explicar su alcance.
3. El usuario debe conocer y consentir los cambios relevantes.
4. Las métricas deben representar datos reales, no valores decorativos.
5. Los resultados y errores deben quedar visibles.
6. Las operaciones deben ser asíncronas cuando puedan bloquear la interfaz.
7. La aplicación debe intentar verificar el resultado de las acciones.
8. Una recomendación válida puede ser no hacer nada.

## 4. Flujo de una acción

```text
Detectar
   ↓
Analizar
   ↓
Explicar
   ↓
Recomendar
   ↓
Confirmar
   ↓
Actuar
   ↓
Verificar
   ↓
Informar
```

## 5. Estado actual frente a visión

### Base Beta documentada

La Beta dispone de las áreas principales de monitorización, optimización, revisión del PC, almacenamiento, aplicaciones, herramientas avanzadas, historial/seguridad y ajustes.

### Evolución prevista

La dirección de producto contempla:

- diagnóstico más profundo;
- limpieza con vista previa y selección explícita;
- clasificación de procesos críticos;
- historial y reversión;
- perfiles explicables;
- Instalador Sutil;
- mayor integración de la versión instalada;
- alertas de rendimiento comprensibles;
- mejoras específicas para equipos antiguos y modernos.

Estas capacidades deben considerarse **objetivos de producto** hasta que el código y el plan las marquen como implementadas.

## 6. Portable y instalada

| Capacidad | Portable / autocontenida | Instalada |
|---|---|---|
| Diagnóstico | Sí | Sí |
| Mantenimiento básico | Sí | Sí |
| Informe local | Sí | Sí |
| Instalador Sutil | Acceso para instalar | Integrado |
| Monitoreo persistente | No por defecto | Opcional |
| Automatizaciones | Limitadas | Integradas y controladas |
| Historial continuo | Limitado | Sí |
| Actualizaciones integradas | Manuales | Controladas |

La versión portable debe permitir probar el producto. La versión instalada debe poder integrarse de forma más sutil con Windows.

## 7. Seguridad de producto

No se deben ejecutar cambios destructivos de forma silenciosa.

Para operaciones sensibles:

- explicar la consecuencia;
- mostrar el alcance;
- pedir confirmación;
- crear restauración cuando corresponda;
- registrar el resultado;
- ofrecer reversión cuando sea técnicamente posible.

## 8. Perfiles

Los perfiles son conjuntos de acciones preseleccionadas, no modos misteriosos.

Cada perfil debe indicar:

- objetivo;
- acciones incluidas;
- acciones excluidas;
- riesgo;
- necesidad de administrador;
- necesidad de reinicio;
- posibilidad de reversión.

Perfiles futuros pueden incluir escenarios como uso diario, oficina, desarrollo y gaming, pero ningún perfil debe prometer mejoras universales.

## 9. Rendimiento

SUTILBOX debe poder detectar consumo inusual de CPU, memoria u otros recursos.

La experiencia prevista es:

1. identificar el proceso;
2. explicar por qué genera la alerta;
3. permitir revisar el proceso;
4. ofrecer una acción segura cuando exista;
5. abrir herramientas de monitorización si el usuario necesita más contexto.

No se debe finalizar un proceso crítico simplemente porque consume recursos.

## 10. Fuera del foco actual

Durante la Beta, el foco principal es **SUTILBOX General**.

WPC CLI, WPC BOOTBOX OS y WPC OPTIMOS PRO son conceptos futuros/documentados y no deben confundirse con funcionalidades actuales.

## 11. Criterios de producto

Una función está realmente lista cuando:

- hace lo que promete;
- es comprensible;
- no añade software innecesario;
- no perjudica de forma injustificada al sistema;
- respeta consentimiento y seguridad;
- deja un resultado verificable o explica por qué no pudo verificarlo.
