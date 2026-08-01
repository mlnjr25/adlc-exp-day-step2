#!/bin/sh
set -eu

INDEX_HTML="/usr/share/nginx/html/index.html"

if [ -f "$INDEX_HTML" ]; then
  # Replace placeholder in the already-built index.html at container startup.
  # Use | as delimiter so URLs with / do not break the sed expression.
  repl=$(printf '%s' "${VITE_API_URL:-}" | sed 's/[&]/\\&/g')
  sed -i "s|__VITE_API_URL__|$repl|g" "$INDEX_HTML"
fi

exec nginx -g 'daemon off;'
