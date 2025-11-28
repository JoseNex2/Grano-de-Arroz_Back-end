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


## Estándares de código
Este repositorio incluye ` .editorconfig ` y `CONTRIBUTING.md` con reglas obligatorias de estilo y flujo de contribución. Cumple estrictamente con esas reglas antes de enviar PRs.

## Contribuir
1. Crear una rama descriptiva: `feature/mi-cambio` o `fix/descripcion`
2. Asegurarse de que las pruebas pasan y el formato cumple con ` .editorconfig `
3. Abrir un Pull Request hacia la rama principal indicada en el flujo del proyecto

## Licencia
Añade aquí la licencia del proyecto (por ejemplo, MIT) o crea un `LICENSE.md` en el repositorio si aún no existe.

## Contacto
Para dudas o soporte, abre un issue en el repositorio o contacta al mantenedor principal.
