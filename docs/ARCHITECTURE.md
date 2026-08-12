# Arquitectura de WPC-SutilBox

**Estado:** arquitectura de referencia de la Beta  
**Stack:** C# / .NET 8 / WPF / XAML / MVVM

## 1. Principios arquitectónicos

La arquitectura debe permitir que SUTILBOX sea:

- mantenible;
- verificable;
- modular;
- responsivo;
- seguro frente a operaciones administrativas;
- y coherente con la identidad del producto.

La UI no debe contener lógica pesada de sistema. Las operaciones de Windows deben concentrarse en servicios y componentes de Core.

## 2. Capas

```text
Views (XAML)
    ↓ bindings / commands
ViewModels
    ↓ interfaces
Core / Services
    ↓
Windows APIs / WMI / Registry / Process / PowerStatus
```

### Views

Responsables de presentación e interacción.

Ubicación principal:

- `MainWindow.xaml`
- `Views/`

Los estilos y recursos globales se centralizan en `App.xaml` y diccionarios de tema.

### ViewModels

Gestionan estado de presentación, comandos y comunicación con servicios.

Ubicación:

- `ViewModels/`

Componentes relevantes incluyen `MainViewModel` y `ProfileEditorViewModel`.

### Models

Representan datos del dominio y del sistema.

Ubicación:

- `Models/`

### Core / Services

Contiene lógica de negocio, integración con Windows y operaciones del sistema.

Ubicación:

- `Core/`

## 3. Servicios

La solución utiliza servicios especializados para separar responsabilidades, entre ellas:

| Área | Responsabilidad |
|---|---|
| Monitorización | CPU, RAM, red, disco y métricas del sistema |
| Temperatura | Lectura de sensores disponibles |
| Batería | Estado de batería y alimentación |
| Procesos | Consulta, clasificación y acciones sobre procesos |
| Perfiles | Aplicación de configuraciones de rendimiento |
| Restauración | Puntos de restauración |
| Inicio | Aplicaciones de inicio |
| Bloatware | Detección y desinstalación |
| Winget | Consulta y actualización de aplicaciones |
| Ajustes | Persistencia de configuración |
| Logs | Registro de operaciones y resultados |

La lista exacta de servicios debe mantenerse sincronizada con el código real.

## 4. Navegación

`MainWindow` mantiene la estructura principal de la aplicación y una región de contenido.

El estado de navegación se centraliza mediante `CurrentSection` y los convertidores correspondientes.

Las secciones se muestran/ocultan según el estado de navegación, manteniendo una estructura única y consistente.

## 5. Flujo de datos

```text
Usuario
  ↓
View
  ↓ ICommand
ViewModel
  ↓
Service / Core
  ↓
Windows API
  ↓
Resultado
  ↓
ViewModel
  ↓
Binding
  ↓
Usuario
```

Las operaciones potencialmente largas deben ejecutarse de forma asíncrona para mantener la UI responsiva.

## 6. Recursos y temas

`App.xaml` funciona como punto de composición de recursos globales.

Los diccionarios de tema definen el contrato visual común. Los controles deben consumir recursos mediante `DynamicResource` cuando corresponda.

El sistema visual debe evitar duplicar colores, tipografías y estados en cada vista.

La Beta incluye la evolución de:

- `Theme.Dark.xaml`
- `Theme.Light.xaml`
- recursos globales de `App.xaml`

## 7. Instalación y portable

La versión autocontenida/portable permite ejecutar SUTILBOX sin una instalación tradicional.

La versión instalada puede habilitar integración persistente, historial continuo, automatizaciones y actualizaciones controladas.

La instalación no debe introducir software ajeno innecesario.

## 8. Seguridad técnica

Las operaciones que requieren privilegios utilizan los mecanismos de elevación de Windows.

Las acciones sensibles deben tener protección en la capa de servicio y no depender únicamente de ocultarlas en la interfaz.

Los procesos críticos no deben poder finalizarse mediante una acción normal.

Las acciones de configuración sensibles deben considerar restauración y registro.

## 9. Estado de la arquitectura

La arquitectura existente funciona como base de la Beta, pero continuará evolucionando.

No se debe introducir una nueva capa o abstracción solamente por estilo. Cada cambio arquitectónico debe resolver una necesidad real de mantenimiento, seguridad, testabilidad o evolución.
