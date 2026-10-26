# Uploads the Windows build to Steam App 4798760 / depot 4798761
# and sets it live on the existing beta branch "update1.3".
#
# 1. In Unity: File > Build Settings > Windows > Build
#    into Steamworks\ContentBuilder\content\windows
#    (must contain ProjectTrash.exe)
# 2. Run this script. It will prompt for your Steamworks login.
#    Do not put your password in this file.
# 3. After upload, confirm on:
#    https://partner.steamgames.com/apps/builds/4798760
#    Promoting update1.3 to the public default branch is done on that page
#    (SetLive cannot target "default").

param(
    [string]$SteamUser = $env:STEAM_BUILD_USER,
    [switch]$Preview
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$builder = Join-Path $root "builder"
$steamcmd = Join-Path $builder "steamcmd.exe"
$content = Join-Path $root "content\windows"
$appVdf = Join-Path $root "scripts\app_build_4798760.vdf"

New-Item -ItemType Directory -Force -Path $builder, $content, (Join-Path $root "output") | Out-Null

if (-not (Test-Path $steamcmd)) {
    Write-Host "Downloading SteamCMD..."
    $zip = Join-Path (Split-Path $root -Parent) "steamcmd.zip"
    Invoke-WebRequest -Uri "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip" -OutFile $zip
    Expand-Archive -Path $zip -DestinationPath $builder -Force
}

$exe = Join-Path $content "ProjectTrash.exe"
if (-not (Test-Path $exe)) {
    Write-Error @"
No Windows build found at:
  $content\ProjectTrash.exe

Build a Windows Standalone player from Unity into that folder, then run this script again.
"@
}

if (-not $SteamUser) {
    $SteamUser = Read-Host "Steamworks account name (needs Edit App Metadata + Publish)"
}

$buildScript = $appVdf
if ($Preview) {
    $buildScript = Join-Path $root "scripts\app_build_4798760_preview.vdf"
    $text = Get-Content -Raw $appVdf
    if ($text -notmatch '"Preview"') {
        $text = $text -replace '("AppID" "4798760")', "`$1`r`n`t`"Preview`" `"1`""
    }
    Set-Content -Path $buildScript -Value $text -NoNewline
    Write-Host "Preview mode: nothing will be uploaded."
}

Write-Host "Logging into Steam as $SteamUser and uploading App 4798760 to branch update1.3..."
Push-Location $builder
try {
    & $steamcmd +login $SteamUser +run_app_build $buildScript +quit
    $code = $LASTEXITCODE
} finally {
    Pop-Location
}

if ($code -ne 0) {
    Write-Host "steamcmd exited with code $code. Check logs in $root\output and $builder\logs"
    exit $code
}

Write-Host @"
Upload finished. Open Your Builds and confirm the new BuildID is live on update1.3:
https://partner.steamgames.com/apps/builds/4798760

Players on the update1.3 branch will get 1.3. To ship to everyone, set that BuildID live on default from the same page.
"@
