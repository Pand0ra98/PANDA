$ErrorActionPreference = 'Stop'
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$project = Split-Path -Parent $MyInvocation.MyCommand.Path
$program = Join-Path $project 'Program.cs'
$tests = Join-Path $project 'Tests.cs'
$installerSource = Join-Path $project 'Installer.cs'
$uninstallerSource = Join-Path $project 'Uninstaller.cs'
$icon = Join-Path $project 'PANDA.ico'
$testExe = Join-Path $project 'PANDA.Tests.exe'
$appExe = Join-Path $project 'PANDA-Portable.exe'
$uninstallerExe = Join-Path $project 'PANDA-Uninstall.Payload.exe'
$setupExe = Join-Path $project 'PANDA-Setup.exe'
$iconArgument = "/win32icon:$icon"

if (-not (Test-Path -LiteralPath $icon)) { throw 'PANDA.ico fehlt.' }

& $compiler /nologo /target:exe /main:Panda.Tests /out:$testExe /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll $program $tests
if ($LASTEXITCODE -ne 0) { throw 'Test-Build fehlgeschlagen.' }

& $testExe
if ($LASTEXITCODE -ne 0) { throw 'Tests fehlgeschlagen.' }

& $compiler /nologo /target:winexe /main:Panda.Program /optimize+ /platform:anycpu /out:$appExe $iconArgument /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll $program
if ($LASTEXITCODE -ne 0) { throw 'Programm-Build fehlgeschlagen.' }

& $compiler /nologo /target:winexe /main:PandaUninstall.Program /optimize+ /platform:anycpu /out:$uninstallerExe $iconArgument /reference:System.dll /reference:System.Core.dll /reference:System.Windows.Forms.dll $uninstallerSource
if ($LASTEXITCODE -ne 0) { throw 'Uninstaller-Build fehlgeschlagen.' }

$appResource = "/resource:$appExe,PANDA.Application.exe"
$uninstallerResource = "/resource:$uninstallerExe,PANDA.Uninstaller.exe"
& $compiler /nologo /target:winexe /main:PandaSetup.Program /optimize+ /platform:anycpu /out:$setupExe $iconArgument /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll $appResource $uninstallerResource $installerSource
if ($LASTEXITCODE -ne 0) { throw 'Installer-Build fehlgeschlagen.' }

$verifyProcess = Start-Process -FilePath $setupExe -ArgumentList '--verify' -WindowStyle Hidden -Wait -PassThru
if ($verifyProcess.ExitCode -ne 0) { throw 'Installer-Payload-Prüfung fehlgeschlagen.' }

Remove-Item -LiteralPath $testExe -Force
Remove-Item -LiteralPath $uninstallerExe -Force
Write-Host "Portable Einzeldatei erstellt: $appExe"
Write-Host "Installer mit Uninstaller erstellt: $setupExe"
