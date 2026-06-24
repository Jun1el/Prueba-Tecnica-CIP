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

- [x] Issue 0: README inicial y planificacion del repositorio.
- [x] Issue 1: Bootstrap del proyecto .NET Razor Pages.
- [x] Issue 2: Docker Compose base con app, SQL Server y API mock.
- [x] Issue 3: EF Core, entidades, migracion inicial y seed del evento.
- [x] Issue 4: Cliente HTTP para API mock de colegiados.
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

Levantar el entorno completo:

```bash
docker-compose up --build
```

Ejecutar pruebas:

```bash
dotnet test
```

Aplicar migraciones EF Core en el SQL Server local:

```bash
dotnet ef database update --project src/PruebaTecnicaCip.Web --startup-project src/PruebaTecnicaCip.Web
```

## Servicios Docker

- Web: `http://localhost:8080`
- API mock de colegiados: `http://localhost:3001/colegiados`
- SQL Server: `localhost,1433`

Configuracion local del cliente de colegiados:

- `ColegiadosApi__BaseUrl=http://colegiados-api:3000` dentro de Docker Compose.
- `ColegiadosApi:BaseUrl=http://localhost:3001` para ejecucion local fuera de Docker.

Credenciales locales de SQL Server:

- Usuario: `sa`
- Password: `Cip_StrongPassword123!`
- Base de datos esperada: `PruebaTecnicaCip`

## Estado actual

Repositorio inicializado con la planificacion tecnica, estructura base Razor Pages, Docker Compose inicial, modelo EF Core con migracion inicial y cliente HTTP para la API mock de colegiados. El siguiente paso sera implementar el Issue 5 cuando se solicite explicitamente.
