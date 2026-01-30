# TwiceTasks

TwiceTasks es una aplicación web de productividad construida con **ASP.NET Core 8 (MVC + Razor Views)**. El objetivo es tener un “hub” personal donde puedas **organizar notas por espacios de trabajo**, **adjuntar y ordenar archivos**, **planificar eventos en un calendario** y **guardar snippets de código**.

## Funcionalidades

- **Autenticación y usuarios**
  - Registro / inicio de sesión con **ASP.NET Core Identity**.
  - Datos segregados por usuario (cada usuario ve únicamente su contenido).

- **Workspaces (espacios de trabajo)**
  - Crear, editar y eliminar workspaces.
  - Cada workspace agrupa tus páginas/notas.

- **Páginas / Notas**
  - Crear y editar notas con título y contenido.
  - Notas “sueltas” (sin workspace) y notas dentro de un workspace.
  - Mover notas entre workspaces o dejarlas sin workspace.
  - **Etiquetas (tags)**: añadir y eliminar tags por nota.

- **Búsqueda**
  - Buscar páginas por **título** y por **tags**.

- **Gestión de archivos**
  - Subida de archivos a tu biblioteca.
  - Organización por **colecciones** (carpetas lógicas).
  - Mover archivos entre colecciones y eliminar archivos.
  - Subir/adjuntar archivos directamente a una página.
  - Los archivos se guardan bajo `wwwroot/uploads/<userId>/...`.

- **Calendario**
  - Calendario mensual con **FullCalendar (CDN)**.
  - Eventos de día completo o con hora, con fin opcional.
  - Crear/editar/mover/redimensionar eventos (AJAX) y listado de próximos eventos.

- **Desarrollo (snippets de código)**
  - Crear, editar y eliminar snippets.
  - Guardado rápido tipo “bloc de notas” para código.

## Stack técnico

- **.NET 8** (ASP.NET Core MVC + Razor Pages)
- **Entity Framework Core 8**
- **SQL Server** (por defecto: **LocalDB**)
- **ASP.NET Core Identity** (UI incluida)
- **Bootstrap/jQuery** (estáticos en `wwwroot/lib`)
- **FullCalendar** vía CDN para el módulo de calendario

## Requisitos

- **.NET SDK 8.0**
- **SQL Server LocalDB** (recomendado para desarrollo en Windows) o una instancia de SQL Server

## Ejecutar en local

1. Restaurar dependencias:
   - `dotnet restore`
   - `dotnet tool restore`

2. Configurar la base de datos:
   - Revisa `appsettings.json` → `ConnectionStrings:DefaultConnection`.
   - Crear/actualizar la BD con migraciones:
     - `dotnet ef database update`

3. Levantar la aplicación:
   - `dotnet run`

La app quedará disponible en la URL que indique la consola (y/o según `Properties/launchSettings.json`).

## Rutas principales (módulos)

- `/Workspaces` → gestión de espacios de trabajo
- `/Pages` → notas (y notas sin workspace)
- `/Files` → biblioteca de archivos + colecciones
- `/Calendar` → calendario y eventos
- `/Search` → búsqueda de páginas
- `/Desarrollo` → snippets de código

## Notas

- Este proyecto usa migraciones en `Migrations/`. Si cambias modelos, crea una nueva migración con `dotnet ef migrations add <Nombre>` y actualiza la BD.
