@echo off

if not exist node_modules call :init-node
if not exist config.json call :init-config
goto main

:main
cls
call node ./bot.js
timeout /t 5 /nobreak>nul
goto main

:init-node
echo Installing required files...
call npm i
goto :EOF

:init-config
for /f %%i in ('.\bin\InputBox.exe "Enter the desired command prefix" "Prefix"') do set prefix=%%i
for /f %%i in ('.\bin\InputBox.exe "Enter your bot's token" "Token"') do set token=%%i
if "%prefix%"=="" set prefix=/
if "%token%"=="" set token=secret
echo {>config.json
echo   "prefix": "%prefix%",>>config.json
echo   "token": "%token%">>config.json
echo }>>config.json
goto :EOF