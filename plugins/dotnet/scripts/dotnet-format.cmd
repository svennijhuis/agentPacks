@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0dotnet-format.ps1" %*
