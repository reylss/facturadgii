# ApiCf - Facturacion Electronica para Servicios Medicos

## Resumen
ApiCf es una clonacion desacoplada de la solucion original enfocada en facturacion electronica para empresas de servicios medicos.

## Cambios aplicados
- Renombre de solucion/proyectos/namespaces base a `ApiCf`.
- Limpieza de configuraciones sensibles y reemplazo por valores parametrizables.
- Eliminacion de certificado heredado y endurecimiento de `.gitignore` para secretos.
- Soporte e-CF medico con tipos: `E31, E32, E33, E34, E41, E43, E44, E45, E46, E47, E48, E49`.
- Orquestador de flujo e-CF con pasos:
  - Generacion XML
  - Firma digital
  - Validacion fiscal
  - Envio
  - Consulta de estado
  - Reimpresion
  - Anulacion
  - Auditoria

## Escenarios medicos soportados
- CONSULTA
- PROCEDIMIENTO
- ESTUDIO
- LABORATORIO
- HONORARIOS
- SEGURO_MEDICO
- COBERTURA_ARS
- PACIENTE_PARTICULAR
- FACTURACION_CORPORATIVA

## Variables requeridas
Usar el archivo `.env.example` como referencia:
- `ASPNETCORE_ENVIRONMENT`
- `APICF_DB_CONNECTION`
- `APICF_JWT_SECURITY_KEY`
- `APICF_CERT_PFX_PATH`
- `APICF_CERT_PASSWORD`
- `APICF_DGII_QR_URL`
- `APICF_DGII_QR_SMALL_URL`
- `APICF_VOXEL_USERNAME`
- `APICF_VOXEL_PASSWORD`
- `APICF_VOXEL_SERVICE_URL`
- `APICF_SMTP_HOST`
- `APICF_SMTP_PORT`
- `APICF_SMTP_USER`
- `APICF_SMTP_PASSWORD`

## Configuracion por ambiente
- `src/ApiCf.Web.Host/appsettings.Development.json`
- `src/ApiCf.Web.Host/appsettings.QA.json`
- `src/ApiCf.Web.Host/appsettings.Production.json`

## Despliegue basico
1. Configurar secretos por ambiente (no versionar certificados ni passwords).
2. Ejecutar:

```powershell
./build/setup-apicf.ps1 -Environment Development
```

3. Levantar API:

```powershell
dotnet run --project ./src/ApiCf.Web.Host/ApiCf.Web.Host.csproj
```

## Nota de seguridad
No reutilizar certificados, tokens ni credenciales del proyecto original. Toda integracion externa debe configurarse mediante secretos externos por ambiente.
