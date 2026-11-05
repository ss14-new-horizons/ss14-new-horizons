@echo off
cd ../../
call dotnet test Content.IntegrationTests/Content.IntegrationTests.csproj -c DebugOpt %*
pause
