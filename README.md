## Pre-reqs

- Install IIS for ASP.NET on Windows

## Prepare IIS

Publish to a folder and as a website to IIS

## Prepare SQL in Docker

Followed steps from https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver17&tabs=cli&pivots=cs1-powershell

docker pull mcr.microsoft.com/mssql/server:2025-latest

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Password1!" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2025-latest

docker exec -it sql1 "bash"

/opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "Password1!" -No

CREATE DATABASE TestDB;
GO

USE TestDB;
CREATE TABLE Inventory
(
id INT,
name NVARCHAR (50),
quantity INT,
)
GO

INSERT INTO Inventory
VALUES (1, 'banana', 150);
GO

CREATE PROCEDURE ProcedureName
AS
SELECT * FROM TestDB;
GO;

## Install OTel Auto instrumentation

Follow steps as per https://opentelemetry.io/docs/zero-code/dotnet/#instrument-an-aspnet-application-deployed-on-iis

In summary, in an admin terminal use:

Set-ExecutionPolicy RemoteSigned                                                               
$module_url = "https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/latest/download/OpenTelemetry.DotNet.Auto.psm1"                                                                        
$download_path = Join-Path $env:temp "OpenTelemetry.DotNet.Auto.psm1"
Invoke-WebRequest -Uri $module_url -OutFile $download_path -UseBasicParsing
Import-Module $download_path
Install-OpenTelemetryCore
Register-OpenTelemetryForIIS

## Add the following ENV Vars to the app pool

COR_ENABLE_PROFILING  = 1
OTEL_SERVICE_NAME = SqlClientApp
OTEL_EXPORTER_OTLP_ENDPOINT = <YOURCLOUDURL>
OTEL_EXPORTER_OTLP_HEADERS = Authorization=ApiKey <YOURAPIKEY>
OTEL_LOG_LEVEL = debug

OTEL_DOTNET_AUTO_LOG_DIRECTORY = C:\Logs\SqlClientApp

## Testing autoinstrumentation code

Build using `dotnet nuke`

Stop IIS

Replace the instrumentation files in "C:\Program Files\OpenTelemetry .NET AutoInstrumentation" for the outputfrom bin/tracer-home after building the autointrumentation repo.

You'll also need to copy OpenTelemetry.AutoInstrumentation.dll from the newly built version to the subdirectory C:\Windows\Microsoft.NET\assembly\GAC_MSIL\OpenTelemetry.AutoInstrumentation\v4.0_1.0.0.0__c0db600a13f60b51

Restart IIS

Run the website
