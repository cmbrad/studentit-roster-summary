# from http://stackoverflow.com/questions/1153126/how-to-create-a-zip-archive-with-powershell#answer-13302548
function ZipFiles( $zipfilename, $sourcedir )
{
   Add-Type -Assembly System.IO.Compression.FileSystem
   $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
   [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir,
        $zipfilename, $compressionLevel, $false)
}

$framework = "netcoreapp1.1"
$publishDirectory = "bin/release/$framework"
$packageName = "studentit-roster-summary.zip"

dotnet restore
dotnet lambda package --configuration Release --framework $framework

Exit $LASTEXITCODE