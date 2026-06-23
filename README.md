# Prueba Tecnica CIP - Sistema de Inscripcion y Validacion de Eventos

MVP para gestionar inscripciones al evento institucional "Dia del Padre".

El objetivo del repositorio es avanzar de forma progresiva, por issues pequenos y commits atomicos, priorizando reglas de negocio, persistencia, concurrencia en cupos y despliegue reproducible con Docker Compose.

## Stack propuesto

- .NET / ASP.NET Core Razor Pages
- Entity Framework Core
- SQL Server
- Docker Compose
- API mock de colegiados usando `colegiados.json`

## Alcance del MVP

- Portal publico de inscripcion con DNI, nombre y carga de imagen del DNI del menor.
- Validacion inmediata contra API mock de colegiados.
- Registro de solicitudes validas en estado `PENDIENTE`.
- Rechazo automatico cuando no se cumplan reglas obligatorias.
- Dashboard administrador con metricas y solicitudes pendientes.
- Aprobacion consumiendo cupo del evento.
- Rechazo con observacion obligatoria.
- Notificaciones e invitaciones simuladas mediante logs.
- Levantamiento completo con `docker-compose up`.

## Reglas de negocio principales

- El colegiado debe estar habilitado.
- El colegiado debe pertenecer al Consejo Departamental de Lima.
- Personal administrativo no puede inscribirse.
- La aprobacion consume cupo del evento.
- Cuando el aforo esta lleno, se bloquean nuevas inscripciones y nuevas aprobaciones.
- El rechazo administrativo requiere una observacion.

## Estrategia de concurrencia

La aprobacion de solicitudes debera proteger el cupo del evento con una transaccion en base de datos y control de concurrencia optimista usando `RowVersion` en la entidad del evento. El contador de aprobados se actualizara en la misma unidad de trabajo que cambia la solicitud a `APROBADO`.

## Backlog inicial

- [ ] Issue 0: README inicial y planificacion del repositorio.
- [ ] Issue 1: Bootstrap del proyecto .NET Razor Pages.
- [ ] Issue 2: Docker Compose base con app, SQL Server y API mock.
- [ ] Issue 3: EF Core, entidades, migracion inicial y seed del evento.
- [ ] Issue 4: Cliente HTTP para API mock de colegiados.
- [ ] Issue 5: Reglas de elegibilidad del colegiado.
- [ ] Issue 6: Portal de inscripcion.
- [ ] Issue 7: Dashboard administrador.
- [ ] Issue 8: Rechazo administrativo con observacion y log.
- [ ] Issue 9: Aprobacion con consumo de cupo y concurrencia.
- [ ] Issue 10: Bloqueo de nuevas inscripciones por aforo lleno.
- [ ] Issue 11: Pruebas de integracion minimas.
- [ ] Issue 12: Documentacion final y verificacion con Docker Compose.

## Plan de trabajo

El avance se realizara issue por issue. No se implementara todo de golpe. Cada issue debera tener commits pequenos, descriptivos y orientados a una pieza logica del sistema.

## Comandos esperados

Estos comandos se completaran cuando exista la estructura del proyecto:

```bash
docker-compose up --build
```

```bash
dotnet test
```

## Estado actual

Repositorio inicializado con la planificacion tecnica y backlog base. El siguiente paso sera implementar el Issue 1 cuando se solicite explicitamente.
