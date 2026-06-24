# Prueba Tecnica CIP - Sistema de Inscripcion y Validacion de Eventos

MVP para gestionar inscripciones al evento institucional "Dia del Padre".

El repositorio fue desarrollado de forma progresiva por issues pequenos y commits atomicos, priorizando reglas de negocio, persistencia, concurrencia en cupos, pruebas automatizadas y despliegue reproducible con Docker Compose.

## Stack

- .NET 8 / ASP.NET Core Razor Pages
- Entity Framework Core
- SQL Server
- Docker Compose
- API mock de colegiados con json-server
- xUnit
- GitHub Actions

## Funcionalidades

- Portal publico de inscripcion con DNI, nombre y carga de imagen del DNI del menor.
- Validacion inmediata contra API mock de colegiados.
- Reglas de elegibilidad del colegiado.
- Registro de solicitudes validas en estado `Pending`.
- Rechazo automatico de solicitudes no elegibles.
- Dashboard administrador con metricas y solicitudes pendientes.
- Aprobacion administrativa con consumo de cupo.
- Rechazo administrativo con observacion obligatoria y log.
- Bloqueo de nuevas inscripciones cuando el aforo esta lleno.
- Pruebas unitarias e integracion minima.
- CI para `develop` y `main`.

## Reglas de negocio

- El colegiado debe estar habilitado.
- El colegiado debe pertenecer al Consejo Departamental de Lima.
- Personal administrativo no puede inscribirse.
- La aprobacion consume cupo del evento.
- Cuando el aforo esta lleno, se bloquean nuevas inscripciones y nuevas aprobaciones.
- El rechazo administrativo requiere una observacion.

## Concurrencia

La aprobacion protege el cupo del evento con una transaccion en base de datos y control de concurrencia optimista usando `RowVersion` en la entidad del evento. El contador de aprobados se actualiza en la misma unidad de trabajo que cambia la solicitud a `Approved`.

## Servicios

- Portal publico: `http://localhost:8080`
- Dashboard admin: `http://localhost:8080/Admin`
- API mock de colegiados: `http://localhost:3001/colegiados`
- SQL Server: `localhost,1433`

Credenciales locales de SQL Server:

- Usuario: `sa`
- Password: `Cip_StrongPassword123!`
- Base de datos: `PruebaTecnicaCip`

## Ejecucion local

Levantar servicios:

```bash
docker compose up --build -d
```

Aplicar migraciones EF Core si la base esta vacia:

```bash
dotnet ef database update --project src/PruebaTecnicaCip.Web --startup-project src/PruebaTecnicaCip.Web
```

Ejecutar pruebas:

```bash
dotnet test PruebaTecnicaCip.sln
```

Guia detallada de verificacion Docker Compose:

- [docs/DOCKER-COMPOSE.md](docs/DOCKER-COMPOSE.md)

## Workflow Git

- `main`: rama estable.
- `develop`: rama de integracion.
- `codex/issue-*`: ramas por issue.

GitHub Actions ejecuta restore, build y tests en pushes y pull requests contra `develop` y `main`.

## Backlog

- [x] Issue 0: README inicial y planificacion del repositorio.
- [x] Issue 1: Bootstrap del proyecto .NET Razor Pages.
- [x] Issue 2: Docker Compose base con app, SQL Server y API mock.
- [x] Issue 3: EF Core, entidades, migracion inicial y seed del evento.
- [x] Issue 4: Cliente HTTP para API mock de colegiados.
- [x] Issue 5: Reglas de elegibilidad del colegiado.
- [x] Issue 6: Portal de inscripcion.
- [x] Issue 7: Dashboard administrador.
- [x] Issue 8: Rechazo administrativo con observacion y log.
- [x] Issue 9: Aprobacion con consumo de cupo y concurrencia.
- [x] Issue 10: Bloqueo de nuevas inscripciones por aforo lleno.
- [x] Issue 11: Pruebas de integracion minimas.
- [x] Issue 12: Documentacion final y verificacion con Docker Compose.
