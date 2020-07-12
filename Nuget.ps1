param ([string]$Suffix = "")

#versioning info
$DATE = Get-Date -UFormat "%Y.%m%d"
$VERSION = "$($DATE).$($env:GITHUB_RUN_NUMBER)$($Suffix)"

#directories
$WORKINGDIR = Get-Location

#pack into nuget files with the suffix if we have one
Write-Output "Publishing DragonFruit.Common.Data (Version $VERSION)"
dotnet pack ".\DragonFruit.Common.Data\DragonFruit.Common.Data.csproj" -o $WORKINGDIR -c Release -p:PackageVersion=$VERSION

#recursively push all nuget files created
Get-ChildItem -Path $WORKINGDIR -Filter *.nupkg -Recurse -File -Name | ForEach-Object {
    dotnet nuget push $_ --api-key $env:nuget_key --source https://api.nuget.org/v3/index.json --force-english-output
}
