# set repo variables
$repo = "FunOrange/circle-tracker"
$releases = "https://api.github.com/repos/$repo/releases"

# find latest release
Write-Host Determining latest release
$tag = (Invoke-WebRequest $releases | ConvertFrom-Json)[0].tag_name

# set download variables
$file = "circle-tracker-" + $tag + ".rar"
$download = "https://github.com/$repo/releases/download/$tag/$file"
$name = $file.Split(".")[0]
$rar = "$name.rar"
$dir = "$name-$tag"
$folder = "$name"

Write-Host Dowloading latest release
Invoke-WebRequest $download -Out $rar

# extract downloaded rar release file
$download = Get-ChildItem -path '.' -filter "*.rar"  
$winrar = "C:\Program Files\WinRAR\RAR.exe"
foreach ($rar in $download)
{
&$winrar x -y $rar .
}

# overwrite old folder with new release
Write-Host Copying from download dir to old release dir
Copy-Item $folder\* -Destination . -Force

# delete downloaded rar and (empty) extaction-folder
Write-Host Cleaning up
Remove-Item $rar -Recurse -Force -ErrorAction SilentlyContinue 
Remove-Item $folder -Recurse -Force
