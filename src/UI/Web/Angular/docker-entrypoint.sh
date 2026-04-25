#!/bin/sh

# Substitui as variáveis de ambiente no template e gera o arquivo real
envsubst '${API_URL}' < /usr/share/nginx/html/env.template.js > /usr/share/nginx/html/env.js

# Inicia o Nginx
exec nginx -g 'daemon off;'
