# AGENTS.md

## Propósito
DeliverySac: sistema web de reparto y trazabilidad de pedidos. El administrador asigna pedidos a repartidores y unidades, los repartidores marcan estados desde el celular (con soporte offline/reintento) y el administrador/supervisor consultan el estado y la hora de cada cambio. Ver `PRD.md`.

## Stack
- Backend: .NET 8 (LTS) + EF Core(comandos) + dapper(consultas) + SQL Server Express
- Frontend: Angular 17/18/19 (última),borrar plantilla inicial de angular al crear el proyecto,PrimeNg compatible, npm, PWA/mobile-first (>= 320px) con cola offline (RF-11)
- Frontend: Usar primeng version compatible
- Tests: xUnit (`dotnet test`)

## UI/UX (convenciones)
Requisitos completos en `PRD.md` → "Diseño de Interfaz (UX)". Convenciones para el equipo:
- Base de layout: plantilla oficial **Sakai** (`primefaces/sakai-ng`) de PrimeNG con **tema PrimeNG personalizado** (no dejar el blanco por defecto). Paleta institucional, primario `#1976D2`.
- Shell: barra superior + **menú lateral colapsable**; el repartidor usa layout de **una columna**, sin menú lateral.
- **Una pantalla (y una opción de menú) por función/CRUD.** Prohibido concentrar varias funciones en una sola pantalla, siempre.
- Listados y formularios **separados**; el alta/edición va en su propia pantalla o en un modal dedicado.
- Selección con **mucha data** → modal con tabla paginada + búsqueda (no combos); combos solo cuando el conjunto de opciones es pequeño.
- Estados del pedido como **etiquetas de color** (una por estado).
- Todas las **tablas con el componente Table/DataTable de PrimeNG paginado** (`page`/`size`); nada de tablas HTML.
- Usar **exclusivamente componentes de PrimeNG** (botones, inputs, tablas, diálogos, calendarios, etc.); prohibido HTML nativo estilizado a mano. Iconos con PrimeIcons.
- **Tema PrimeNG propio** (paleta, tipografía, radios, espaciado como tokens); sin colores/estilos sueltos por componente.
- **Feedback uniforme**: Toast de PrimeNG para éxito/error y ConfirmDialog para acciones irreversibles.
- **Estados de carga y vacío** consistentes (Skeleton/ProgressSpinner + mensaje "sin datos").
- **Formularios consistentes**: labels visibles, validación con mensajes de error, agrupación en tarjetas y misma disposición de botones.
- **Accesibilidad**: label asociado, foco visible, contraste AA y navegación por teclado.
- **Navegación**: título de página + breadcrumb y sección activa resaltada en el menú.
- Fechas/números en *locale* `es-PE` (zona America/Lima).
- Login: pantalla dividida (*split screen*) tipo *glassmorphism*.

## Cómo correr
- Instalar: `dotnet restore` en la raíz/API y `npm install` en el frontend
- Levantar backend: `dotnet run` en la carpeta de la API
- Levantar frontend: `ng serve` en el frontend
- Tests: `dotnet test`

## Entorno de demo
- La conexión a la base de datos de demo se define por la variable de entorno `DELIVERYSAC_CONNECTION` o por `backend/src/DeliverySac.Api/appsettings.Development.local.json` (**no versionado**). **Nunca** commitear credenciales (constitución, Principio IV).
- API de desarrollo: `http://localhost:5210` (perfil `http` en `launchSettings.json`). Frontend: `http://localhost:4200` (`ng serve`).
- Sembrado de datos base (usuarios, unidades, cliente): se ejecuta en Development leyendo `DELIVERYSAC_ADMIN_PASSWORD`.
- CORS (demo): la política `Frontend` permite cualquier origen para facilitar pruebas desde cualquier navegador/host (`AllowAnyOrigin`); en producción debe endurecerse a los orígenes conocidos.
- Navegadores: si una pestaña queda cargando indefinidamente (p. ej. Opera con Service Worker/caché de un build previo), limpiar datos del sitio + Service Worker y recargar en duro; validar en Brave/Chrome. El PWA/Service Worker debe ser compatible con Opera.

## Commits (una subtarea = un commit)
Antes de cada commit, cargar con la herramienta de skills: `task-commit` y `conventional-commit`.
1. Verificar backend: `dotnet build` y `dotnet test` en `backend/` (si falla, arreglar y NO continuar).
2. Verificar frontend: `npm run build` en `frontend/` (y `npm test`).
3. Stagear solo los archivos de la subtarea: `git add -- <archivos>` (NUNCA `git add .`; NUNCA `bin/`, `obj/`, `dist/`, `node_modules/`).
4. Commit con `conventional-commit`: `tipo(scope): descripción` en imperativo, minúscula, sin punto.
5. Marcar la tarea `[ ]` → `[x]` en `specs/<feature>/tasks.md`.

## Qué NO hacer
- No editar pedidos; solo reasignar los que estén en `PENDIENTE` o `REPROGRAMADO`; rechazar el resto con HTTP 400 (RF-05).
- No alterar la hora ni el ID de usuario registrados en los cambios de estado: son automáticos e inalterables (RF-09).
- No depender de pasarelas o servicios pagos de terceros (RNF-05).