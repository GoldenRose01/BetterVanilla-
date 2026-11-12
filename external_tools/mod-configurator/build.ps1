$ErrorActionPreference = 'Stop'
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$siblings = Split-Path $repo -Parent
$templates = Join-Path $PSScriptRoot 'templates'
New-Item -ItemType Directory -Force "$templates/bettervanilla", "$templates/eldritch", "$templates/enchlib" | Out-Null
Copy-Item "$repo/src/main/resources/config/bettervanilla/defaults.json" "$templates/bettervanilla/config.json" -Force
Copy-Item "$siblings/Eldrich-Surge/src/main/resources/config/eldrich-surge/enchantment_caps.json" "$templates/eldritch/enchantment_caps.json" -Force
Copy-Item "$siblings/enchlib/src/main/resources/config/*" "$templates/enchlib/" -Force
# Package the existing tool's item catalog so the unified EXE also works outside the checkout.
$ids = [System.Collections.Generic.SortedSet[string]]::new()
$names = @{}
foreach ($modRepo in @($repo, "$siblings/Eldrich-Surge")) {
    $assets = Join-Path $modRepo 'src/main/resources/assets'
    Get-ChildItem -LiteralPath $assets -Filter '*.json' -Recurse | ForEach-Object {
        $relative = [IO.Path]::GetRelativePath($assets, $_.FullName).Replace('\', '/')
        if ($relative -match '^([^/]+)/items/(.+)\.json$') { [void]$ids.Add("$($Matches[1]):$($Matches[2])") }
        if ($relative -match '/lang/it_it\.json$') {
            $language = Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json -AsHashtable
            foreach ($key in $language.Keys) { $names[$key] = $language[$key] }
        }
    }
}
$cache = Join-Path $env:USERPROFILE '.gradle/caches/fabric-loom/minecraftMaven/net/minecraft'
if (Test-Path -LiteralPath $cache) {
    $jar = Get-ChildItem -LiteralPath $cache -Filter 'minecraft-client*.jar' -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($jar) {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $archive = [IO.Compression.ZipFile]::OpenRead($jar.FullName)
        try { foreach ($entry in $archive.Entries) {
            if ($entry.FullName -match '^assets/minecraft/items/(.+)\.json$') { [void]$ids.Add("minecraft:$($Matches[1])") }
        } } finally { $archive.Dispose() }
    }
}
@{ items = @($ids); names = $names } | ConvertTo-Json -Depth 8 | Set-Content "$PSScriptRoot/catalog.json" -Encoding utf8

dotnet publish "$PSScriptRoot/src/ModConfigurator.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$PSScriptRoot/publish"
if ($LASTEXITCODE -ne 0) { throw 'Compilazione fallita' }
Copy-Item "$PSScriptRoot/publish/ModConfigurator.exe" "$PSScriptRoot/ModConfigurator-v2.exe" -Force
try { Copy-Item "$PSScriptRoot/publish/ModConfigurator.exe" "$PSScriptRoot/ModConfigurator.exe" -Force }
catch [System.IO.IOException] { Write-Host 'La versione precedente è aperta. Nuova versione disponibile in ModConfigurator-v2.exe.' }
