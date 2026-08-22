@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0rules-context.ps1" %*
