#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres <<-'EOSQL'
SELECT 'CREATE DATABASE estoque_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'estoque_db')\gexec

SELECT 'CREATE DATABASE faturamento_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'faturamento_db')\gexec
EOSQL
