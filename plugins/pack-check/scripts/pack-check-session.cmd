@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0pack-check-session.ps1" %*
