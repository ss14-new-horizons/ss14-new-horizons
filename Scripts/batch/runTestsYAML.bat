@echo off
cd ../../
call dotnet run --project Content.YAMLLinter/Content.YAMLLinter.csproj -c DebugOpt %*
pause
