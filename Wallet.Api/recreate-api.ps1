$curDir = $PSScriptRoot;
$solutionDir = (Split-Path $PSScriptRoot -Parent);

# variables
$projectFile="$solutionDir/Wallet.Server/Wallet.Server.csproj";
$namespace = "EWallet.Api";
$nswagFile = "$curDir/Api/Api.nswag";


# run
$nswagExe = "${Env:ProgramFiles(x86)}/Rico Suter/NSwagStudio/Net80/dotnet-nswag.exe";
$variables="/variables:namespace=$namespace,apiFile=Api.cs,projectFile=$projectFile";
& "$nswagExe" run $nswagFile $variables;