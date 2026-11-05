#!/usr/bin/env sh

# make sure to start from script dir
if [ "$(dirname $0)" != "." ]; then
    cd "$(dirname $0)"
fi

cd ../../
python3 RUN_THIS.py
dotnet build -c Release "$@"
read -p "Press enter to continue"
