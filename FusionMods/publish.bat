@echo off
set PROJECT=FusionMods.csproj
set CONFIG=Release
set RUNTIME=win-x64
if exist publish rmdir /s /q publish
dotnet publish %PROJECT% -c %CONFIG% -r %RUNTIME% ^
    /p:PublishSingleFile=true ^
    /p:IncludeAllContentForSelfExtract=true ^
    /p:DebugType=None ^
    /p:DebugSymbols=false ^
    /p:PublishTrimmed=false ^
    -o publish

echo.
echo Fertig! Die fertige EXE liegt in:
echo   %cd%\publish
pause
