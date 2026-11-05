#!/usr/bin/env sh

# make sure to start from script dir
if [ "$(dirname $0)" != "." ]; then
    cd "$(dirname $0)"
fi

cd ../../
dotnet test Content.Tests/Content.Tests.csproj -c DebugOpt "$@"
read -p "Tests complete. Press enter to continue."
