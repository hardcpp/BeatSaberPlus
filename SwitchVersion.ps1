$l_Versions = (
    "D:\SteamLibrary\steamapps\common\Beat Saber 1.18.3",
    "D:\SteamLibrary\steamapps\common\Beat Saber 1.19.0",
    "D:\SteamLibrary\steamapps\common\Beat Saber 1.20.0",
    "D:\SteamLibrary\steamapps\common\Beat Saber",
    "C:\Program Files (x86)\Steam\steamapps\common\Beat Saber"
);


write-host “------------------------VERSION-------------------------";
$l_I = 1;
foreach ($l_Version in $l_Versions)
{
    $l_I.ToString() + ". " + $l_Version;
    $l_I++;
}
write-host "--------------------------------------------------------";

$l_Choice = (read-host “Version:”) - 1;
$l_Files  = @(Get-ChildItem *.csproj -Recurse);

$l_NewUserFile = "";
$l_NewUserFile += '<?xml version="1.0" encoding="utf-8"?>' + [System.Environment]::NewLine;
$l_NewUserFile += '<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">' + [System.Environment]::NewLine;
$l_NewUserFile += '  <PropertyGroup>' + [System.Environment]::NewLine;
$l_NewUserFile += '    <BeatSaberDir>' + $l_Versions[$l_Choice] + '</BeatSaberDir>' + [System.Environment]::NewLine;
$l_NewUserFile += '    <ProjectView>ProjectFiles</ProjectView>' + [System.Environment]::NewLine;
$l_NewUserFile += '  </PropertyGroup>' + [System.Environment]::NewLine;
$l_NewUserFile += '</Project>' + [System.Environment]::NewLine;
$l_NewUserFile;

foreach ($file in $l_Files)
{
    $l_NewUserFile | Out-File -FilePath $($file.FullName + ".user")
}