param(
    [string]$Path = ".",
    [string]$Find = "Vanigam.Accounting",
    [string]$Replace = "Vanigam.CRM"
)

# 1. Replace text inside file contents (.cs & .csproj)
Get-ChildItem -Path $Path -Recurse -Include *.cs, *.csproj | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -match [regex]::Escape($Find)) {
        $content -replace [regex]::Escape($Find), $Replace | 
            Set-Content -Path $_.FullName -Encoding utf8
        Write-Host "Updated contents in $($_.FullName)"
    }
}

# 2. Rename files
Get-ChildItem -Path $Path -Recurse -Include *.cs, *.csproj | Where-Object {
    $_.Name -like "*$Find*"
} | ForEach-Object {
    $newName = $_.Name -replace [regex]::Escape($Find), $Replace
    $newPath = Join-Path $_.DirectoryName $newName
    Rename-Item -Path $_.FullName -NewName $newName
    Write-Host "Renamed file: $($_.FullName) -> $newPath"
}

# 3. Rename folders (do this last, and deepest first)
Get-ChildItem -Path $Path -Recurse -Directory | Sort-Object FullName -Descending | Where-Object {
    $_.Name -like "*$Find*"
} | ForEach-Object {
    $newName = $_.Name -replace [regex]::Escape($Find), $Replace
    Rename-Item -Path $_.FullName -NewName $newName
    Write-Host "Renamed folder: $($_.FullName) -> $newName"
}
