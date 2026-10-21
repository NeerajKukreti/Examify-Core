#!/bin/bash
set -e

# Render provides the public port in the PORT environment variable (default: 8080)
PORT="${PORT:-8080}"

echo "=========================================================="
echo "    Starting ExamiFy Production Suite (Unified Container)"
echo "    Public MVC Port : $PORT"
echo "    Internal API    : http://127.0.0.1:5001"
echo "    Database Target : 101.53.148.84:8649 (db_exam)"
echo "=========================================================="

# 1. Start ExamifyAPI in the background on internal port 5001
echo "[EXAMIFY] Booting ExamifyAPI in background..."
ASPNETCORE_URLS="http://127.0.0.1:5001" dotnet /app/api/ExamifyAPI.dll &
API_PID=$!

# Brief pause for API socket to bind
sleep 2

# 2. Start Examify MVC in the foreground binding to Render public port
echo "[EXAMIFY] Booting Examify MVC Web Portal on port $PORT..."
export ExamifyAPI__BaseUrl="http://127.0.0.1:5001/api/"
export AppSettings__Api="http://127.0.0.1:5001/api/"
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"

exec dotnet /app/mvc/Examify.dll
