param(
    [Parameter(Mandatory = $true)]
    [string]$MigrationSqlPath,

    [string]$MigrationsDirectory = "",

    [string]$MarkersFile = ""
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($MigrationsDirectory)) {
    $MigrationsDirectory = Join-Path $ScriptDir "..\Migrations"
}
if ([string]::IsNullOrWhiteSpace($MarkersFile)) {
    $MarkersFile = Join-Path $ScriptDir "verify-migration-markers.json"
}

if (-not (Test-Path $MigrationSqlPath)) {
    throw "Migration SQL file not found: $MigrationSqlPath"
}

if (-not (Test-Path $MarkersFile)) {
    throw "Markers file not found: $MarkersFile"
}

$markerConfig = Get-Content -Path $MarkersFile -Raw -Encoding UTF8 | ConvertFrom-Json
$requiredStrings = @($markerConfig.requiredStrings)

$sql = [System.IO.File]::ReadAllText($MigrationSqlPath, [System.Text.Encoding]::UTF8)

foreach ($required in $requiredStrings) {
    if ($sql.IndexOf($required, [System.StringComparison]::Ordinal) -lt 0) {
        throw "migration.sql is missing required content: $required"
    }
}

$migrationCsFiles = Get-ChildItem -Path $MigrationsDirectory -Filter "*.cs" |
    Where-Object { $_.Name -notlike "*.Designer.cs" -and $_.Name -ne "PayrollDbContextModelSnapshot.cs" }

foreach ($file in $migrationCsFiles) {
    $designerPath = Join-Path $MigrationsDirectory ($file.BaseName + ".Designer.cs")
    if (-not (Test-Path $designerPath)) {
        throw "Orphan migration without Designer file: $($file.Name)"
    }
}

$designerFiles = Get-ChildItem -Path $MigrationsDirectory -Filter "*.Designer.cs"
$migrationIds = @()
foreach ($designer in $designerFiles) {
    $content = [System.IO.File]::ReadAllText($designer.FullName, [System.Text.Encoding]::UTF8)
    if ($content -match '\[Migration\("([^"]+)"\)\]') {
        $migrationIds += $Matches[1]
    }
}

$migrationIds = $migrationIds | Sort-Object -Unique
foreach ($id in $migrationIds) {
    $needle = "MigrationId] = N'$id'"
    if ($sql.IndexOf($needle, [System.StringComparison]::Ordinal) -lt 0) {
        throw "migration.sql does not include EF migration: $id"
    }
}

Write-Host "Migration script verification passed."
Write-Host "  File: $MigrationSqlPath"
Write-Host "  Migrations checked: $($migrationIds.Count)"
Write-Host "  Required schema markers: $($requiredStrings.Count)"
