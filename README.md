# Grano de Arroz — Back-end

## Descripción
API REST para la gestión de baterías y reportes de mediciones, construida con .NET 9 y C# 13. Proporciona endpoints para creación y consulta de reportes, actualización de estados de mediciones y métricas históricas.

## Tecnologías
- .NET 9
- C# 13
- Entity Framework Core
- MYSQL (u otro proveedor ADO.NET compatible)
- Visual Studio 2026

## Requisitos
- SDK .NET 9 instalado
- Visual Studio 2026 (recomendado) o `dotnet` CLI
- Cadena de conexión a una base de datos MYSQL

## Instalación
1. Clonar el repositorio:


2. Restaurar paquetes y compilar:


3. Configurar la cadena de conexión en `appsettings.Development.json` o mediante variables de entorno que se encuentran en: https://github.com/V4l3n73/Environment-Backend-GDA


## Ejecutar
- Desde Visual Studio 2026:
  - Abrir la solución
  - Ejecutar __Build Solution__
  - Iniciar con __Debug > Start Debugging__ o __Debug > Start Without Debugging__

- Desde `dotnet` CLI:


## Migraciones y base de datos
Crear/actualizar la base de datos con EF Core:


## Pruebas
Si hay proyectos de pruebas:


## Contribuir
1. Crear una rama descriptiva: `feature/mi-cambio` o `fix/descripcion`
2. Asegurarse de que las pruebas pasan y el formato cumple con ` .editorconfig `
3. Abrir un Pull Request hacia la rama principal indicada en el flujo del proyecto


## Resumen de Endpoints principales– API
🔋 Módulo Baterías
Endpoint	                                            Método	          Autorización	                    Descripción	                          Respuestas
/api/battery/registrybattery	                         POST	    AccessScheme + Rol Sucursal	    Registra una batería a un cliente.	      200 · 400 · 401 · 403
/api/battery/batteriessearch	                         GET	          AccessScheme	            Obtiene todas las baterías.	                    200 · 401
/api/battery/batterysearchwithid?id={id}	             GET	          AccessScheme	            Busca una batería por Id.	                   200 · 404 · 401
/api/battery/batterysearchbyclientid?ClientId={id}	   GET	          AccessScheme	            Baterías asociadas a un cliente.	           200 · 404 · 401


🔐 Módulo Acceso (Usuarios & Login)
Endpoint                          Método          Autorización                    Descripción                            Respuestas
/api/access/registry	             POST	     AccessScheme + Rol Admin	         Registrar usuario.	                  200 · 400 · 401 · 403
/api/access/login		               POST            Público	             Autentica usuario y devuelve token.	            200 · 400
/api/access/userssearch	           GET	     AccessScheme + Admin	          Obtener todos los usuarios.	               200 · 401 · 403
/api/access/usersearch?id={id}	   GET	     AccessScheme + Admin	            Buscar usuario por Id.	              200 · 404 · 401 · 403
/api/access/roleupdate	           PUT	     AccessScheme + Admin	          Actualizar rol de usuario.	               200 · 400 · 404
/api/access/rolessearch	           GET	     AccessScheme + Admin	               Listar roles.	                             200


👤 Módulo Cliente
Endpoint	                        Método	        Autorización	                  Descripción	                            Respuestas
/api/client/registryclient	       POST	     AccessScheme + Rol Sucursal	     Registrar cliente.	                   200 · 400 · 401 · 403
/api/client/clientssearch	         GET	     AccessScheme + Rol Sucursal	  Obtener todos los clientes.	                200 · 401 · 403
/api/client/clientsearch?id={id}	 GET	     AccessScheme + Rol Sucursal	    Buscar cliente por Id.	               200 . 404 · 401 · 403


📄 Módulo Reportes
Endpoint	                                Método	        Autorización	                          Descripción	                          Respuestas
/api/report/createreport	                 POST	     AccessScheme + Rol Sucursal	       Crear reporte asociado a batería.	    200 · 400 · 401 · 403 · 404
/api/report/reportssearch	                 POST	     AccessScheme + Admin/Sucursal/Lab	    Buscar reportes con filtros.	         200 · 404 · 401 · 403
/api/report/updatemeasurementreport	       PUT	     AccessScheme + Rol Laboratorio	      Actualizar mediciones del reporte.	  200 · 400 · 404 · 401 · 403
/api/report/reportgetbyid?reportId={id}	   GET	     AccessScheme + Admin/Sucursal/Lab	      Obtener reporte por Id.	              200 · 404 · 401 · 403

## Contacto
Para dudas o soporte, abre un issue en el repositorio o contacta al mantenedor principal.
