# Verificacion con Docker Compose

Esta guia valida el MVP completo usando los tres servicios definidos en `docker-compose.yml`.

## Servicios

- `web`: aplicacion ASP.NET Core Razor Pages en `http://localhost:8080`.
- `sqlserver`: SQL Server 2022 en `localhost,1433`.
- `colegiados-api`: API mock con json-server en `http://localhost:3001/colegiados`.

## Levantar el entorno

```bash
docker compose up --build -d
```

Verificar estado:

```bash
docker compose ps
```

Los servicios `cip-sqlserver` y `cip-colegiados-api` deben aparecer como `healthy`, y `cip-web` como `Up`.

## Aplicar base de datos

Si el volumen de SQL Server esta nuevo o vacio, aplicar migraciones:

```bash
dotnet ef database update --project src/PruebaTecnicaCip.Web --startup-project src/PruebaTecnicaCip.Web
```

Conexion local a SQL Server:

- Servidor: `localhost,1433`
- Usuario: `sa`
- Password: `Cip_StrongPassword123!`
- Base de datos: `PruebaTecnicaCip`

## Verificaciones manuales

Portal publico:

```bash
curl http://localhost:8080
```

Dashboard administrador:

```bash
curl http://localhost:8080/Admin
```

API mock:

```bash
curl "http://localhost:3001/colegiados?dni=12345678"
```

Consultar evento seed:

```bash
docker exec cip-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Cip_StrongPassword123! -C -d PruebaTecnicaCip -Q "SELECT Id, Name, Council, Capacity, ApprovedCount FROM InstitutionalEvents;"
```

## Flujo funcional esperado

1. Registrar un colegiado valido desde `http://localhost:8080`.
2. Confirmar que la solicitud aparece en `http://localhost:8080/Admin`.
3. Aprobar la solicitud para consumir un cupo.
4. Rechazar otra solicitud pendiente indicando observacion.
5. Cuando `ApprovedCount` alcance `Capacity`, el portal bloquea nuevas inscripciones.

## Detener servicios

Detener sin borrar datos:

```bash
docker compose stop
```

Remover contenedores sin borrar volumen:

```bash
docker compose down
```

Remover contenedores y borrar la base de datos local:

```bash
docker compose down -v
```
