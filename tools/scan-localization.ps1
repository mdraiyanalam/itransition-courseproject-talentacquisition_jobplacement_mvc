# Run from repository root (PowerShell)
# Extract keys from all SharedResources*.resx and search project for usage.
$resxFiles = Get-ChildItem -Path . -Filter "SharedResources*.resx" -Recurse
$keys = @{}
foreach ($f in $resxFiles) {
    [xml]$xml = Get-Content $f.FullName
    $dataNodes = $xml.root.data | ForEach-Object { $_.name }
    $keys[$f.FullName] = $dataNodes
}

"RESX files found:"
$resxFiles | ForEach-Object { $_.FullName }

"`nKeys found:"
foreach ($k in $keys.Keys) {
    "`nFile: $k"
    $keys[$k] | ForEach-Object { "  - $_" }
}

"`nSearching for usages of keys in repository..."
$allFiles = Get-ChildItem -Path . -Include *.cshtml,*.cs -Recurse | Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\' }
foreach ($file in $allFiles) {
    $text = Get-Content $file.FullName -Raw
    foreach ($resx in $keys.Values | Select-Object -Unique) {
        foreach ($key in $resx) {
            if ($text -match [regex]::Escape("@SharedLocalizer[`"$key`"]") -or $text -match [regex]::Escape("SharedLocalizer[`"$key`"]")) {
                "{0} -> uses -> {1}" -f $file.FullName, $key
            }
        }
    }
    # also show hard-coded occurrences of common terms
    foreach ($term in @("Login","Register","Logout","Positions")) {
        if ($text -match "\b$term\b") {
            "{0} -> contains literal -> {1}" -f $file.FullName, $term
        }
    }
}
@using talentacquisition_jobplacement_mvc.Resources
@inject Microsoft.Extensions.Localization.IStringLocalizer<SharedResources> SharedLocalizer