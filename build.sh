#!/bin/bash
set -e

if [ "$#" -ne 1 ]; then
    echo "Usage: $0 \"1.2.3\""
    exit 1
fi

VERSION=$1

echo "Build version: " $VERSION

echo "====================================="
echo "           Run tests                 "
echo "====================================="
dotnet test ./src/*.Tests/

echo "====================================="
echo "        Create nuget packet          "
echo "====================================="
dotnet pack ./src/ --configuration Release \
                   --no-build \
                   /property:Version=$VERSION \
                   /property:AssemblyVersion=$VERSION \
                   /property:FileVersion=$VERSION \
                   /property:PackageVersion=$VERSION