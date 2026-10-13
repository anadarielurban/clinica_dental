
# Clínica Dental

Aplicación de escritorio para la gestión integral de una clínica dental. Centraliza la operación clínica, administrativa y financiera del consultorio en un solo sistema.

## Stack tecnológico

| Componente | Tecnología |
|---|---|
| Lenguaje | C# 12 |
| Framework | .NET 8 |
| Interfaz | WPF |
| Patrón de presentación | MVVM |
| ORM | Entity Framework Core 9 |
| Base de datos | SQLite |
| Gráficos | LiveCharts |
| PDF | iText7, PdfSharpCore, MigraDocCore |
| Excel | ClosedXML |
| Tema visual | MahApps.Metro |
| Iconografía | FontAwesome |
| MVVM helpers | CommunityToolkit.Mvvm |

## Arquitectura
src/
├── Data/ # DbContext, configuración EF Core
├── Models/ # Entidades del dominio
├── ViewModels/ # Lógica de presentación (MVVM)
├── Views/ # Vistas XAML por módulo
├── Converters/ # IValueConverter para bindings
├── Helpers/ # Utilidades
└── Styles/ # Estilos y recursos compartidos

text

- **Persistencia:** SQLite en archivo local (`clinica-dental.db`), creado automáticamente al primer arranque.
- **Acceso a datos:** DbContext único (`DentalClinicContext`) con `DbSet<>` por entidad.
- **Navegación:** sistema de pestañas dinámicas administrado por `MainViewModel`.
- **Validación:** implementación de `INotifyDataErrorInfo` en ViewModels de formularios.

## Módulos funcionales

**Clínicos**
- Pacientes con expediente completo y adjuntos
- Citas con agenda semanal por dentista
- Tratamientos y planes de tratamiento
- Recetas médicas
- Órdenes de laboratorio

**Administrativos**
- Dentistas, empleados y asignaciones
- Consultorios y equipos
- Horarios por dentista
- Inventario y proveedores

**Financieros**
- Facturación con generación de PDF
- Dashboard con estadísticas en tiempo real
- Ingresos por dentista (gráfico de columnas)
- Exportación de reportes a PDF y Excel

## Características técnicas

- Funcionamiento 100% local, sin dependencia de red
- Interfaz completamente en español
- Tema oscuro personalizado
- Base de datos portable en archivo único
- Navegación multi-pestaña con cierre individual
- Exportación a Excel (ClosedXML) y PDF (iText7 / PdfSharpCore)
- Gráficos analíticos con LiveCharts

## Requisitos

- Windows 10 o superior
- .NET 8 Desktop Runtime (o SDK para desarrollo)

## Estado del proyecto

- Migración de SQL Server a SQLite: completada
- Traducción completa al español: completada
- 16 módulos operativos
- Módulo POS: en planeación

ntent -Path C:\Users\Ana\DentalClinic\README.md -Value $readme -Encoding UTF8
Write-Host "README corregido."
