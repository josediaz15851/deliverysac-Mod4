<!--
Sync Impact Report
==================
Version change: (unratified scaffold) → 1.0.0
Bump rationale: Initial ratification of the DeliverySac constitution. No prior
ratified version existed, so the template scaffold is replaced by the first
adopted version (1.0.0).

Modified principles: none (initial adoption)
Added principles:
  - I. Spec-First & Test-First (TDD) (NON-NEGOTIABLE)
  - II. Aislamiento Absoluto de IA
  - III. Inmutabilidad y Zona Horaria (America/Lima)
  - IV. Cero Secretos en el Código
  - V. Alineación Estricta al PRD
Added sections:
  - Restricciones Adicionales y Estándares Técnicos
  - Flujo de Desarrollo y Puertas de Calidad
Removed sections: none

Deferred TODOs: none
-->

# DeliverySac Constitution

## Core Principles

### I. Spec-First & Test-First (TDD) (NON-NEGOTIABLE)

Ninguna subtarea o componente de código se implementa sin su especificación previa
(`spec.md`) y sin seguir estrictamente el ciclo rojo-verde-refactor. Las pruebas
automatizadas (`dotnet test`) se escriben antes que la implementación, deben fallar
primero (rojo), luego pasar (verde) y después se refactoriza sin alterar el
comportamiento verificado. No se acepta código sin cobertura que respalde el
comportamiento especificado.

**Rationale**: Garantiza que cada comportamiento responde a un requisito verificable
y previene regresiones, haciendo demostrable la correspondencia entre especificación,
prueba e implementación.

### II. Aislamiento Absoluto de IA

Toda la lógica de interacción con modelos de IA vive exclusivamente en un módulo
dedicado, jamás mezclada con la lógica de negocio. El sistema nunca inventa datos que
no estén en su fuente de verdad; ante la duda, deriva obligatoriamente a revisión
humana. Ningún componente de negocio depende directamente de un proveedor de IA.

**Rationale**: Aísla el riesgo de alucinaciones, mantiene la trazabilidad de las
respuestas y permite reemplazar o auditar el módulo de IA sin afectar la lógica de
negocio.

### III. Inmutabilidad y Zona Horaria (America/Lima)

Las operaciones de fecha y hora que rigen la logística de reparto operan bajo la zona
horaria local de Perú (`America/Lima`). Los registros de trazabilidad, estados y marcas
de tiempo son estrictamente inmutables: queda prohibido modificar o eliminar trazas
históricas. Los cambios de estado registran automáticamente la hora y el ID de usuario,
y estos valores son inalterables.

**Rationale**: Preserva la integridad de la auditoría y la confiabilidad de la
trazabilidad de cada pedido, evitando discrepancias de tiempo entre operaciones.

### IV. Cero Secretos en el Código

Queda prohibido hardcodear credenciales, cadenas de conexión, tokens o URLs de
servicios. Toda configuración sensible o dependiente del entorno se inyecta
exclusivamente mediante variables de entorno o archivos protegidos no versionados. Los
secretos nunca se agregan al control de versiones.

**Rationale**: Protege las credenciales del sistema y evita la exposición de secretos en
el repositorio, cumpliendo las prácticas de seguridad del proyecto.

### V. Alineación Estricta al PRD

No se diseñan, programan ni despliegan funcionalidades, endpoints ni componentes de
UI/UX que queden fuera del alcance o de las reglas funcionales descritas en el PRD
vigente. Cualquier necesidad no cubierta se trata primero como enmienda al PRD, nunca
como implementación directa.

**Rationale**: Evita la expansión no controlada del alcance (scope creep) y mantiene la
coherencia entre lo especificado, lo construido y lo desplegado.

## Restricciones Adicionales y Estándares Técnicos

- Backend: .NET 8 (LTS) + EF Core (comandos) + Dapper (consultas) + SQL Server Express;
  pruebas con xUnit.
- Frontend: Angular (última versión estable soportada) con PrimeNG compatible, PWA
  mobile-first (ancho mínimo 320px) con cola offline (RF-11), locale `es-PE`.
- UI/UX: usar exclusivamente componentes de PrimeNG (tablas paginadas `page`/`size`,
  diálogos, calendarios, etc.), tema PrimeNG propio, iconos PrimeIcons y feedback
  uniforme (Toast/ConfirmDialog). Una opción de menú por función/CRUD.
- No depender de pasarelas ni servicios pagos de terceros (RNF-05).
- Toda configuración de entorno se resuelve por variables de entorno o archivos
  locales no versionados (ver Principio IV).

## Flujo de Desarrollo y Puertas de Calidad

- Una subtarea compilable equivale a un commit; los cambios se documentan con
  Conventional Commits (`tipo(scope): descripción`).
- Antes de commitear: `dotnet build` y `dotnet test` en `backend/` deben pasar; en
  `frontend/`, `npm run build` (y `npm test`) deben pasar. Si algo falla, se corrige y
  no se continúa.
- Stagear únicamente los archivos de la subtarea (nunca `git add .`; nunca `bin/`,
  `obj/`, `dist/`, `node_modules/`).
- Cada tarea completada se marca `[ ]` → `[x]` en `specs/<feature>/tasks.md`.
- Las revisiones de PR verifican el cumplimiento de estos principios.

## Governance

Esta constitución prevalece sobre cualquier otra práctica, plantilla o convención del
proyecto. Toda enmienda requiere: (1) documentar el cambio y su justificación, (2)
aprobación explícita, (3) actualización de la versión según el esquema siguiente y (4)
un plan de migración cuando el cambio afecte trabajo ya realizado.

Versionado semántico de la constitución:
- MAJOR: eliminación o redefinición incompatible de principios o reglas de gobernanza.
- MINOR: incorporación de un nuevo principio o expansión material de la guía.
- PATCH: aclaraciones, correcciones de redacción o refinamientos no semánticos.

Revisión de cumplimiento: todas las contribuciones (PRs, commits y revisiones) deben
verificar el cumplimiento de los principios aquí definidos. La complejidad o las
excepciones deben justificarse explícitamente. Para la guía operativa de runtime del
día a día, regir por `AGENTS.md` y `PRD.md`.

**Version**: 1.0.0 | **Ratified**: 2026-10-14 | **Last Amended**: 2026-10-14
