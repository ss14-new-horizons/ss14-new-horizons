@echo off
cd ../../
call dotnet test Content.Tests/Content.Tests.csproj -c DebugOpt %*
pause
