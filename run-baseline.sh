#!/bin/bash
set -a; source .env; set +a
k6 run -e API_EMAIL="$API_EMAIL" -e API_SENHA="$API_SENHA" baseline.js
