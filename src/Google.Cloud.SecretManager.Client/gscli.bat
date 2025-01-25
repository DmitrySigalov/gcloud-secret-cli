@echo off
if "%OS%" == "Windows_NT" setlocal

setlocal enabledelayedexpansion

set AWS_SSM_CLI_HOME="%~dp0"

setlocal DISABLEDELAYEDEXPANSION

%AWS_SSM_CLI_HOME%aws-ssm-cli.exe %*