#!/bin/bash
set -x

PREFIX="0.0.10"
DEST_PACKAGES=$1
DOTNET_SOLUTION=$2
if [ -z $DEST_PACKAGES ]  || [ -z $DOTNET_SOLUTION ]
then
    printf "Missing parameters. Usage: $0 DEST_PACKAGES DOTNET_SOLUTION\n"
    printf "Example: $0 /mypackages src/CarbonAwareSDK.sln"
    exit 1
fi

mkdir -p $DEST_PACKAGES
# Remove existing packages with PREFIX
find $DEST_PACKAGES -name "*.nupkg" -name "*.snupkg" -exec rm {} \;
revision=$(git rev-parse HEAD)
branch=$(git symbolic-ref --short HEAD)
description="Green Software Foundation SDK. Allows to retreive Carbon Emissions data from different data sources like WattTime, ElectricityMap or a static json file."
authors="GSF"
company="GSF"
title="Green Software Foundation SDK"
tags="Green-Software-Foundation GSF Microsoft"
repurl="https://github.com/Green-Software-Foundation/carbon-aware-sdk"
license="MIT"
dotnet pack $DOTNET_SOLUTION -o $DEST_PACKAGES -c Debug \
    -p:VersionPrefix=$PREFIX \
    -p:VersionSuffix=beta \
    -p:Authors=$authors \
    -p:Company=$company \
    -p:Title=$title \
    -p:PackageTags=$tags \
    -p:RepositoryUrl= $repurl \
    -p:RepositoryType=git \
    -p:RepositoryBranch=$branch \
    -p:SourceRevisionId=$revision \
    -p:Description=$description \
    -p:PackageLicenseExpression=$license \
    -p:IncludeSymbols=true \
    -p:SymbolPackageFormat=snupkg
