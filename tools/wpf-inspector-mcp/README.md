# WPF Inspector MCP

[MCP server para inspección en tiempo real de aplicaciones WPF](https://github.com/komandio-labs/wpf-inspector-mcp)

## Versión instalada

- **wpfinspectmcp.exe** v0.2.0 (~63 MB)

## Instalación

Descargado desde:
```
https://github.com/komandio-labs/wpf-inspector-mcp/releases/download/v0.2.0/wpfinspectmcp.exe
```

## Configuración

Registrado en `~/.junie/mcp/mcp.json` como MCP server `wpf-inspector` con transporte `stdio`.

## Uso

Permite a Junie inspeccionar la interfaz WPF de WPC-SutilBox en tiempo real:
- Árboles visuales y lógicos
- Bindings y DataContext
- Captura de screenshots
- Interacción semántica con controles
- Diagnóstico de errores de binding

## Notas

- El ejecutable está en `.gitignore` (binario de 63 MB)
- Para reinstalar, descargar desde [GitHub Releases](https://github.com/komandio-labs/wpf-inspector-mcp/releases)
