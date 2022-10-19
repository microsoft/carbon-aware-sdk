#!/bin/bash
set -x

DOTNET_PROJECT="$1"
PACKAGE_SRC="$2"

if [ -z $DOTNET_PROJECT ] || [ -z $PACKAGE_SRC ]
then
    printf "Missing parameters. Usage: $0 DOTNET_PROJECT PACKAGE_SRC\n"
    printf "Example: $0 myapp.csprj /mypackages"
    exit 1
fi

declare -a packages=(
    "GSF.CarbonIntensity"
    "CarbonAware"
    "CarbonAware.Aggregators"
    "CarbonAware.DataSources.Json"
    "CarbonAware.DataSources.Registration"
    "CarbonAware.DataSources.WattTime"
    "CarbonAware.LocationSources.Azure"
    "CarbonAware.Tools.WattTimeClient"
)

# Remove packages from project
for pname in "${packages[@]}"
do
    dotnet remove $DOTNET_PROJECT package $pname
done

rm -rf ~/.nuget/packages/carbonaware*
rm -rf ~/.nuget/packages/gsf.carbonintensity*

# Add packages to project
for pname in "${packages[@]}"
do
    dotnet add $DOTNET_PROJECT package $pname -s $PACKAGE_SRC --prerelease
done
