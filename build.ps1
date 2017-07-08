$framework = "netcoreapp1.1"
$publishDirectory = "bin/release/$framework"
$packageName = "studentit-roster-summary.zip"
$fullPath = "$publishDirectory/$packageName"
$extraFolder = "extra"

dotnet restore
if (-not $?)
{
    throw "Failed to restore packages"
}

dotnet lambda package --configuration Release --framework $framework
if (-not $?)
{
    throw "Failed to create deployment package"
}

Write-Output "Adding extra files to deployment package"
# Append code from: https://stackoverflow.com/questions/14827716/adding-a-complete-directory-to-an-existing-zip-file-with-system-io-compression-f
$null = [Reflection.Assembly]::LoadWithPartialName( "System.IO.Compression.FileSystem" )
$packageFile=[System.IO.Compression.ZipFile]::Open($fullPath, "Update")
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
$in = Get-ChildItem $extraFolder -Recurse | where {!$_.PsisContainer}| select -expand fullName

[array]$files = $in
ForEach ($file In $files) 
{
    # The element after the last backslash will be the filename
	$fileName = $file.split('\')[-1]

	# the files in the zip should be in the extras folder
	$fileNameInZip = "$extraFolder\$fileName"

	# Add file to the zip. Use $null to hide output
    $null = [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($packageFile,$file,$fileNameInZip,$compressionlevel)
}
# Close off the zip file to actually write the new files
$packageFile.Dispose()

if (-not $?)
{
    throw "Failed to add extras to deployment package"
}

Write-Output "Build complete!"
