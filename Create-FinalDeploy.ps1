$ErrorActionPreference = "Stop"

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Command,

        [Parameter(Mandatory = $true)]
        [string]$StepName
    )

    Write-Host ""
    Write-Host "=== $StepName ==="

    & $Command

    if ($LASTEXITCODE -ne 0) {
        throw "Step failed: $StepName"
    }
}

$Root = Get-Location
$ArtifactsDir = Join-Path $Root "artifacts"
$DeployDir = Join-Path $ArtifactsDir "final-deploy-clean"
$BackendOut = Join-Path $DeployDir "backend"
$DbOut = Join-Path $DeployDir "db"
$ZipPath = Join-Path $ArtifactsDir "AccountingProject-final-deploy-CLEAN.zip"

Write-Host "=== Cleaning old deploy artifacts ==="
Remove-Item -Recurse -Force ".\client\dist" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force ".\server\wwwroot" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force ".\server\bin" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force ".\server\obj" -ErrorAction SilentlyContinue
Remove-Item -Force ".\build.zip" -ErrorAction SilentlyContinue
Remove-Item -Force ".\AccountingProject-final-deploy*.zip" -ErrorAction SilentlyContinue

Remove-Item -Recurse -Force $DeployDir -ErrorAction SilentlyContinue
Remove-Item -Force $ZipPath -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Force $BackendOut | Out-Null
New-Item -ItemType Directory -Force $DbOut | Out-Null

Invoke-Checked {
    dotnet restore ".\AccountingProject.sln"
} "Restoring backend"

Invoke-Checked {
    dotnet test ".\AccountingProject.sln" -c Release
} "Running backend tests"

Write-Host ""
Write-Host "=== Cleaning frontend locked files ==="

Push-Location ".\client"

try {
    cmd /c "taskkill /F /IM node.exe 2>NUL"
    cmd /c "taskkill /F /IM npm.exe 2>NUL"

    Remove-Item -Recurse -Force ".\node_modules" -ErrorAction SilentlyContinue

    if (!(Test-Path ".\package-lock.json")) {
        throw "Missing client/package-lock.json. Cannot run reproducible npm ci build."
    }

    Invoke-Checked {
        npm ci
    } "Installing frontend dependencies"

    Invoke-Checked {
        npm test -- --run
    } "Running frontend tests"

    Invoke-Checked {
        npm run build
    } "Building frontend"
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "=== Copying frontend build into server/wwwroot ==="

Remove-Item -Recurse -Force ".\server\wwwroot\*" -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force ".\server\wwwroot" | Out-Null
Copy-Item -Recurse ".\client\dist\*" ".\server\wwwroot\"

Invoke-Checked {
    dotnet publish ".\server\AccountingProject.csproj" -c Release -o $BackendOut
} "Publishing backend"

Write-Host ""
Write-Host "=== Removing appsettings files from deploy package ==="

Remove-Item -Force (Join-Path $BackendOut "appsettings.json") -ErrorAction SilentlyContinue
Remove-Item -Force (Join-Path $BackendOut "appsettings.Development.json") -ErrorAction SilentlyContinue
Remove-Item -Force (Join-Path $BackendOut "appsettings.Production.json") -ErrorAction SilentlyContinue

Invoke-Checked {
    dotnet build ".\server\AccountingProject.csproj" -c Release
} "Building server before EF migration script"

Invoke-Checked {
    dotnet ef migrations script `
        --project ".\server\AccountingProject.csproj" `
        --startup-project ".\server\AccountingProject.csproj" `
        --configuration Release `
        --idempotent `
        -o (Join-Path $DbOut "migration.sql")
} "Creating database migration script"

$migrationSqlPath = Join-Path $DbOut "migration.sql"
Invoke-Checked {
    & ".\server\Scripts\Verify-MigrationScript.ps1" -MigrationSqlPath $migrationSqlPath
} "Verifying migration.sql includes all EF migrations and required schema"

Write-Host ""
Write-Host "=== Verifying deploy package content ==="

if (!(Test-Path (Join-Path $BackendOut "AccountingProject.dll"))) {
    throw "Missing backend/AccountingProject.dll"
}

if (!(Test-Path (Join-Path $BackendOut "wwwroot\index.html"))) {
    throw "Missing backend/wwwroot/index.html"
}

if (!(Test-Path (Join-Path $DbOut "migration.sql"))) {
    throw "Missing db/migration.sql"
}

if (Test-Path (Join-Path $BackendOut "appsettings.json")) {
    throw "appsettings.json should not be included"
}

if (Test-Path (Join-Path $BackendOut "appsettings.Development.json")) {
    throw "appsettings.Development.json should not be included"
}

if (Test-Path (Join-Path $BackendOut "appsettings.Production.json")) {
    throw "appsettings.Production.json should not be included"
}

Write-Host ""
Write-Host "=== Creating zip ==="

Compress-Archive -Path "$DeployDir\*" -DestinationPath $ZipPath -Force

Write-Host ""
Write-Host "DONE."
Write-Host "Deploy package created at:"
Write-Host $ZipPath
