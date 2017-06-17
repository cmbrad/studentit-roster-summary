#!/bin/bash
set -e

# Set up dependencies
dotnet restore
dotnet publish -c release

# Create deployment package
pushd bin/release/netcoreapp1.0/publish
zip -r ./deploy-package.zip ./*
popd
