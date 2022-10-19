#!/bin/bash
set -x

PREFIX="0.0.10"
DEST_PACKAGES=$1
if [[ -z $DEST_PACKAGES ]]
then
    printf "Missing parameter. Usage: $0 DEST_PACKAGES\n"
    printf "Example: $0 /mypackages"
    exit 1
fi

# Remove existing packages with PREFIX
mkdir -p $DEST_PACKAGES
find $DEST_PACKAGES -name "*.nupkg" -exec rm {} \;
DOTNET_SOLUTION="../../src/CarbonAwareSDK.sln"
dotnet pack $DOTNET_SOLUTION -o $DEST_PACKAGES -c Debug \
    -p:VersionPrefix=$PREFIX \
    -p:VersionSuffix=beta \
    -p:Authors="GSF" \
    -p:Company="GSF" \
    -p:Title="Green Software Foundation SDK" \
    -p:PackageTags="Green-Software-Foundation GSF Microsoft" \
    -p:RepositoryUrl="https://github.com/Green-Software-Foundation/carbon-aware-sdk" \
    -p:RepositoryType=git \
    -p:RepositoryBranch=dev \
    -p:Description="Green Software Foundation SDK. Allows to get Carbon Emissions information from WattTime and ElectricityMap sources." \
    -p:PackageLicenseExpression=MIT


# ISSUE: Each package could have its own description.
# ISSUE: To have a very good dev starting guide on how to start
# using the service extension to import the aggregator, how to pass 
# configuration information (using envs, or using own configuration map)
# and how to construct the CarbonAwareParams in case we don't provide
# a better mechanism to deal with.
# reference: https://blog.tonysneed.com/2021/12/04/copy-nuget-content-files-to-output-directory-on-build/

