@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0git-guard.ps1" %*
