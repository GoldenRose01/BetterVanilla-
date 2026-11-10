$ErrorActionPreference = 'Stop'
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$siblings = Split-Path $repo -Parent
$templates = Join-Path $PSScriptRoot 'templates'
New-Item -ItemType Directory -Force "$templates/bettervanilla", "$templates/eldritch", "$templates/enchlib" | Out-Null
Copy-Item "$repo/src/main/resources/config/bettervanilla/defaults.json" "$templates/bettervanilla/config.json" -Force
Copy-Item "$siblings/Eldrich-Surge/src/main/resources/config/eldrich-surge/enchantment_caps.json" "$templates/eldritch/enchantment_caps.json" -Force
Copy-Item "$siblings/enchlib/src/main/resources/config/*" "$templates/enchlib/" -Force
dotnet publish "$PSScriptRoot/src/ModConfigurator.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$PSScriptRoot/publish"
if ($LASTEXITCODE -ne 0) { throw 'Compilazione fallita' }
Copy-Item "$PSScriptRoot/publish/ModConfigurator.exe" "$PSScriptRoot/ModConfigurator.exe" -Force
