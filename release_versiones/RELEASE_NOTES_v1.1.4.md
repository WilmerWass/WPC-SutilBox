# Notas de versión — WPC-SutilBox Beta 1.1.4

**Producto:** WPC-SutilBox  
**Versión:** `1.1.4-beta`  
**Ensamblado / archivo:** `1.1.4.0`  
**Fecha:** 2026-10-15  
**Plataforma:** Windows 10 / 11, x64  
**Runtime:** .NET 8 Desktop Runtime (este paquete no es autocontenido)

> Estas notas describen la Beta 1.1.4 del código actual. No deben confundirse con las notas históricas de WassControlSys que usaban el mismo número de versión.

## Resumen

La Beta 1.1.4 no añade perfiles nuevos. Hace explícito y verificable el flujo que ya existía en el Dashboard: el usuario confirma, el sistema intenta crear un punto de restauración y el perfil solo se comunica como aplicado si el flujo termina sin fallo.

## Qué cambia

- Confirmación antes de aplicar Gaming, Productividad, Desarrollo, Personalizado o volver a Equilibrado.
- Si el servicio de puntos de restauración no está disponible, o el punto no se crea, el perfil se cancela.
- Una selección que no corresponda a `PerformanceMode` se rechaza.
- El plan de energía se comprueba antes de continuar. Si falla o se cancela UAC, no se aplican servicios ni acciones secundarias.
- El snapshot del estado original se toma solo antes del primer perfil y se conserva si la restauración del plan o de algún servicio falla.
- Equilibrado restaura el plan de energía y los servicios guardados. No revierte todo el Registro ni cada ajuste secundario.
- Los servicios duplicados se procesan una sola vez.
- La etiqueta visible del perfil solo cambia tras un resultado correcto.

## Qué no promete esta versión

- No es una transacción reversible completa.
- `powercfg` se lanza elevado, pero no se espera ni se valida su código de salida.
- Ajustes heredados de Windows Update, telemetría, indexación y throttling de red pueden no informar fielmente su resultado.
- La prueba en un equipo Windows elevado (aplicar, cancelar UAC, restaurar e inspeccionar el sistema) sigue pendiente.

## Paquetes

| Elemento | Dependiente del runtime | Autocontenido (recomendado sin .NET) |
|---|---|---|
| Archivo | `WPC-SutilBox-1.1.4-beta-win-x64.zip` | `WPC-SutilBox-1.1.4-beta-selfcontained.zip` |
| Ubicación | `bin/Release/` | `bin/Release/` |
| Ejecutable | `SutilBox.exe` | `SutilBox.exe` (single-file) |
| Tipo | Requiere .NET 8 Desktop Runtime | Incluye el runtime; no requiere instalar .NET |
| Tamaño aprox. | ~1.5 MB | ~64 MB (zip) / ~156 MB (exe) |
| Elevación | El manifiesto solicita administrador | Igual |

## Comprobación de esta entrega

- `dotnet build WPC-SutilBox.csproj -c Release`: 0 errores, 0 advertencias.
- No hay proyecto de pruebas automatizadas en el repositorio.

## Documentación relacionada

- [Beta 1.1.4](../docs/BETA_1.1.4.md)
- [Historial de cambios](../docs/CHANGELOG.md)
- [Cambios implementados](CAMBIOS_IMPLEMENTADOS_v1.1.4.md)
