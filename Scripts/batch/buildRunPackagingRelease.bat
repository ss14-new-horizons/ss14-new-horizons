@echo off
cd ../../
call git submodule update --init --recursive
call dotnet build Content.Packaging -c Release
call dotnet run --project Content.Packaging server --hybrid-acz %*
pause
