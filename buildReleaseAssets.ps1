function CheckDotnet {
    Write-Output "Checking for .NET SDK 10 or newer..."

    try {
        $sdks = dotnet --list-sdks 2>$null

        $net10Plus = $sdks | Where-Object {
            $_ -match '^(\d+)\.' -and [int]$Matches[1] -ge 10
        }

        if ($net10Plus) {
            Write-Output ".NET 10 or newer found."
        }
        else {
            Write-Error ".NET 10 or newer not found."
            exit 1
        }
    }
    catch {
        Write-Error ".NET SDK not found."
        exit 1
    }
}

function Get-MSBuildPath {
    $msbuild = Get-Command msbuild.exe -ErrorAction SilentlyContinue

    if (-not $msbuild) {
        Write-Error "Cannot find MSBuild"
        exit 1
    }

    return $msbuild.Source
}

function CheckDeps {
    CheckDotnet
    Get-MSBuildPath
}


function Invoke-MSBuild {
    param(
        [Parameter(Mandatory)]
        [string]$Solution,

        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    $msbuild = Get-MSBuildPath

    if (-not $msbuild) {
        throw "MSBuild could not be located."
    }

    Write-Host "Running MSBuild: $Solution"

    & $msbuild $Solution @Arguments
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        throw "MSBuild failed for '$Solution' with exit code $exitCode."
    }
}

function Restore {
    $solutions = @(
        "swgoh-ae-gui.sln"
        "swgoh-ae-api.sln"
        "swgoh-ae-console.sln"
    )

    foreach ($solution in $solutions) {
        Invoke-MSBuild -Solution $solution -Arguments @(
            "/t:Restore"
            "/verbosity:minimal"
        )
    }
}


function Compile {
    $projects = @(
        @{
            Solution  = "swgoh-ae-gui.sln"
            Target    = "AssetGUI:publish"
            Name      = "AssetGUI-Windows"
            ExtraArgs = @(
                "/p:Configuration=Release"
                "/p:SelfContained=false"
                "/p:PublishReadyToRun=false"
                "/verbosity:minimal"
            )
        }
        @{
            Solution  = "swgoh-ae-api.sln"
            Target    = "AssetWebApi:publish"
            Name      = "AssetWebAPI"
            ExtraArgs = @(
                "/p:UseAppHost=false"
                "/p:Configuration=Release"
                "/p:SelfContained=false"
                "/verbosity:minimal"
            )
        }
        @{
            Solution  = "swgoh-ae-console.sln"
            Target    = "SwgohAssetGetterConsoleStudio:publish"
            Name      = "SwgohAssetGetterConsoleStudio"
            ExtraArgs = @(
                "/p:UseAppHost=false"
                "/p:Configuration=Release"
                "/p:SelfContained=false"
                "/verbosity:minimal"
            )
        }
    )

    $publishDir = [System.IO.Path]::GetFullPath("publish")
    $zipDir = Join-Path $publishDir "zippedFiles"

    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }

    New-Item -ItemType Directory -Path $zipDir -Force | Out-Null

    foreach ($project in $projects) {
        $name = $project.Name
        $outputDir = Join-Path $publishDir $name
        $zipPath = Join-Path $zipDir "$name.zip"

        $arguments = @(
            "/t:$($project.Target)"
            "/p:PublishDir=$outputDir/"
        ) + $project.ExtraArgs

        Invoke-MSBuild -Solution $project.Solution -Arguments $arguments

        Compress-Archive `
            -Path "$outputDir/*" `
            -DestinationPath $zipPath `
            -CompressionLevel Optimal `
            -Force

        Write-Output "Created: $zipPath"
    }
}

function Main {
    Write-Output "Checking for dependencies..."
    CheckDeps
    Write-Output "Dependencies all found, restoring..."
    Restore
    Write-Output "Restore complete! Starting build..."
    Compile
    Write-Output "Finished build!"
}

Main