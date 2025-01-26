@echo off
if "%OS%" == "Windows_NT" setlocal

setlocal enabledelayedexpansion

set GCLOUD_SECRET_MANAGER_CLI_HOME="%~dp0"

setlocal DISABLEDELAYEDEXPANSION

%GCLOUD_SECRET_MANAGER_CLI_HOME%gcloud-secret-manager-cli.exe %*