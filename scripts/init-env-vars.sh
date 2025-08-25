#!/bin/sh
set -e

echo "Iniciando verificación de variables de entorno..."
until [ -f /secrets_extracted/env-ready ] && [ -f /secrets_extracted/dotnet-env-ready ]; do
  echo "Esperando que Vault genere los archivos de variables..."
  sleep 2
done

if [ ! -s /secrets_extracted/env-vars ] || [ ! -s /secrets_extracted/dotnet-vars.env ]; then
  echo "ERROR: Los archivos de variables están vacíos"
  exit 1
fi

echo "Todas las variables de entorno están listas y verificadas"
touch /secrets_extracted/ALL_SECRETS_READY