param ([string] $ApiKey, [string]$Suffix = "")

# version/file info
$VERSION = "$(Get-Date -UFormat "%Y.%m%d").$($env:GITHUB_RUN_NUMBER)$($Suffix)"

$WORKINGDIR = Get-Location
$PROJECTFILE = Join-Path -Path $WORKINGDIR -ChildPath "DragonFruit.Common.Data\DragonFruit.Common.Data.csproj"

$PRODUCTNAME = "DragonFruit.Common"

# build files

Write-Output "Building $($PRODUCTNAME) Version $VERSION"
dotnet restore
dotnet build $PROJECTFILE --no-restore -c Release /p:Version=$VERSION /p:PackageVersion=$VERSION

# pack into nuget files with the suffix if we have one

Write-Output "Publishing $($PRODUCTNAME) Version $VERSION"
dotnet pack $PROJECTFILE -o $WORKINGDIR -c Release -p:PackageVersion=$VERSION -p:Version=$VERSION

# recursively push all nuget files created

Get-ChildItem -Path $WORKINGDIR -Filter *.nupkg -Recurse -File -Name | ForEach-Object {
    dotnet nuget push $_ --api-key $ApiKey --source https://api.nuget.org/v3/index.json --force-english-output
}
