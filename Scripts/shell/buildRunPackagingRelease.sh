#!/usr/bin/env sh

# make sure to start from script dir
if [ "$(dirname $0)" != "." ]; then
    cd "$(dirname $0)"
fi

cd ../../
git submodule update --init --recursive
dotnet build Content.Packaging -c Release
dotnet run --project Content.Packaging server --hybrid-acz "$@"
read -p "Press enter to continue"
