#!/bin/bash
# Script de verificación: Anti-CLAU
# Verifica que no queden referencias a "CLAU" o "conocimiento" en el código

set -e

echo "🔍 Verificando referencias a CLAU en el código..."
echo ""

# Buscar en archivos de código (case-insensitive)
MATCHES=$(grep -r -i -n --include="*.cs" --include="*.ts" --include="*.tsx" --include="*.json" --include="*.csproj" \
  --exclude-dir=node_modules \
  --exclude-dir=bin \
  --exclude-dir=obj \
  --exclude-dir=dist \
  --exclude-dir=.git \
  -E "clau|conocimiento" \
  ../src/ 2>/dev/null || true)

if [ -n "$MATCHES" ]; then
  echo "❌ FALLÓ: Se encontraron referencias a CLAU/Conocimiento:"
  echo ""
  echo "$MATCHES"
  echo ""
  echo "Por favor elimina estas referencias antes de subir a GitHub."
  exit 1
else
  echo "✅ ÉXITO: No se encontraron referencias a CLAU/Conocimiento"
  echo ""
  echo "El repositorio está listo para GitHub como Vorluno.Contacto"
  exit 0
fi
