# Visión y Especificación de Producto — WPC-SutilBox

## 1. Propósito
WPC-SutilBox es una herramienta de mantenimiento y optimización para sistemas operacionales Windows. Nace con el objetivo de eliminar el "bloatware", optimizar parámetros de rendimiento y ofrecer un diagnóstico claro del hardware y software sin recurrir a scripts destructivos a ciegas.

## 2. Filosofía "Sutil"
El principio fundamental de WPC-SutilBox es **no dañar la experiencia del usuario ni la integridad del sistema operativo**.

* **Transparencia Total:** Cada modificación realizada muestra exactamente qué claves del Registro o servicios modifica antes de ejecutarse.
* **Seguridad y Reversibilidad:** Ningún ajuste es destructivo sin opción de restauración. El usuario puede revertir cualquier tweak aplicado.
* **Ligereza Extrema:** Cero consumo residual. La aplicación no deja servicios en segundo plano consumiendo recursos cuando está cerrada.
* **Cero Placebos:** No se implementan "limpiadores de RAM" falsos ni modificaciones no documentadas que degraden el rendimiento general.

## 3. Modelo de Entrega: Portable vs Installed
* **Portable (Predeterminado):** Funciona como un ejecutable único sin dependencias de instalación. No escribe en carpetas del sistema salvo para guardar la configuración explícita elegida por el usuario.
* **Installed (Opcional):** Permite integración opcional en el menú contextual o inicio rápido del sistema, manteniendo la posibilidad de desinstalación limpia sin residuos.

## 4. Módulos Generales del Producto
1. **Dashboard & Diagnóstico:** Resumen de salud del sistema, consumo de recursos en tiempo real y telemetría local.
2. **Módulo de Mantenimiento / Tweaks:** Modificación de parámetros de red, servicios, energía y comportamiento visual.
3. **Módulo Desbloat / Aplicaciones:** Gestión y limpieza limpia de paquetes AppX y servicios secundarios de Windows.
4. **Módulo Hardware / Sensores:** Lectura directa de parámetros de componentes clave.

## 5. Límites Explícitos (Lo que WPC-SutilBox NO es)
* **NO es un antivirus ni un suite de seguridad.**
* **NO es un "script de optimización a ciegas" estilo batch/ps1 destructivo.**
* **NO modifica ejecutables del sistema operativo ni archivos de sistema protegidos.**
* **NO promete incrementos mágicos de FPS sin sustento técnico.**