#!/bin/bash
set -x

VPREFIX="0.0.10"
DEST_PACKAGES=$1
DOTNET_SOLUTION=$2
if [ -z $DEST_PACKAGES ]  || [ -z $DOTNET_SOLUTION ]
then
    printf "Missing parameters. Usage: $0 DEST_PACKAGES DOTNET_SOLUTION\n"
    printf "Example: $0 /mypackages src/CarbonAwareSDK.sln"
    exit 1
fi

mkdir -p $DEST_PACKAGES
# Remove existing packages
find $DEST_PACKAGES -name "*.nupkg" -name "*.snpukg" -exec rm {} \;
# Setup package metadata
VSUFFIX="beta"
REVISION=$(git rev-parse HEAD)
BRANCH=dev
DESCRIPTION="Green Software Foundation SDK. Allows to retreive Carbon Emissions data from different data sources like WattTime or ElectricityMap or a static json file."
AUTHORS="GSF"
COMPANY="GSF"
TITLE="Green-Software-Foundation-SDK"
TAGS="Green-Software-Foundation GSF Microsoft"
REPURL="https://github.com/Green-Software-Foundation/carbon-aware-sdk"
LICENSE="MIT"
dotnet pack $DOTNET_SOLUTION -o $DEST_PACKAGES -c Debug \
    -p:VersionPrefix=$VPREFIX \
    -p:VersionSuffix=$VSUFFIX \
    -p:Authors=$AUTHORS \
    -p:Company=$COMPANY \
    -p:Title=$TITLE \
    -p:PackageTags="$TAGS" \
    -p:RepositoryUrl=$REPURL \
    -p:RepositoryType=git \
    -p:RepositoryBranch=$BRANCH \
    -p:SourceRevisionId=$REVISION \
    -p:Description="$DESCRIPTION" \
    -p:PackageLicenseExpression=$LICENSE \
    -p:IncludeSymbols="true" \
    -p:SymbolPackageFormat="snupkg"
