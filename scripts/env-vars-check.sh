#!/bin/sh
set -e

echo "Esperando que todas las variables de entorno estén listas..."
until [ -f /secrets_extracted/ALL_SECRETS_READY ]; do
  echo "Esperando señal de inicialización completa..."
  sleep 2
done

echo "Variables de entorno verificadas y listas para usar."

if [ -f /secrets_extracted/env-vars ]; then
  echo "Cargando variables de entorno comunes..."
  . /secrets_extracted/env-vars
fi

if [ -f /secrets_extracted/dotnet-vars.env ]; then
  echo "Cargando variables específicas para .NET..."
fi

echo "Inicialización completada, continuando con el startup normal..."