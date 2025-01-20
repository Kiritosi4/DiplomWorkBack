@echo off
set /p name=enter migration name:
dotnet ef migrations add %name% -s DiplomWork.WebApi -p DiplomWork.Persistance
pause