@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0loop-guard.ps1" %*
