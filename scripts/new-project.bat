@echo off
REM Script wrapper para criar novo projeto a partir do template
REM Delega para o script PowerShell (new-project.ps1)
REM Uso: new-project.bat [NomeProjeto] [opcoes]
REM   Sem argumentos: modo interativo (pergunta nome e configuracoes)
REM   Com argumentos: encaminha ao PowerShell

where powershell >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo Erro: PowerShell nao encontrado.
    echo Instale o PowerShell: https://learn.microsoft.com/powershell/scripting/install/installing-powershell
    exit /b 1
)

set SCRIPT_DIR=%~dp0

REM Forward all arguments to the PowerShell script
if "%1"=="" (
    powershell -ExecutionPolicy Bypass -File "%SCRIPT_DIR%new-project.ps1"
) else (
    powershell -ExecutionPolicy Bypass -File "%SCRIPT_DIR%new-project.ps1" -ProjectName %*
)
