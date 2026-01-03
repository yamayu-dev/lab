param(
    [switch]$Debug,
    [switch]$Release,
    [switch]$Help
)

function Show-Usage {
    Write-Host @"
Usage: .\extract_framework.ps1 [-Debug] [-Release]
  -Debug    Extract Debug xcframework only
  -Release  Extract Release xcframework only
  (No option extracts both)

Environment overrides:
  `$env:FRAMEWORK_NAME = "SpeechAnalyzerKit"
  `$env:SOURCE_DIR = Relative or absolute path to framework zip directory
  `$env:NATIVE_DIR = Relative or absolute path to Native directory
"@
}

if ($Help) {
    Show-Usage
    exit 0
}

# デフォルト設定
$frameworkName = if ($env:FRAMEWORK_NAME) { $env:FRAMEWORK_NAME } else { "SpeechAnalyzerKit" }
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceDir = if ($env:SOURCE_DIR) { $env:SOURCE_DIR } else { Join-Path $scriptDir "..\..\SpeechAnalyzerKit\framework" }
$nativeDir = if ($env:NATIVE_DIR) { $env:NATIVE_DIR } else { Join-Path $scriptDir "Native" }

# 絶対パスに変換
$sourceDir = [System.IO.Path]::GetFullPath($sourceDir)
$nativeDir = [System.IO.Path]::GetFullPath($nativeDir)

Write-Host "Framework: $frameworkName"
Write-Host "Source: $sourceDir"
Write-Host "Native: $nativeDir"

# Native ディレクトリを作成
if (-not (Test-Path $nativeDir)) {
    New-Item -ItemType Directory -Path $nativeDir | Out-Null
}

# 何も指定されていない場合は両方
$extractDebug = $Debug -or (-not $Debug -and -not $Release)
$extractRelease = $Release -or (-not $Debug -and -not $Release)

function Extract-Config {
    param(
        [string]$Config,
        [string]$FrameworkName,
        [string]$SourceDir,
        [string]$NativeDir
    )

    $zipFile = Join-Path $SourceDir "$FrameworkName-$Config.xcframework.zip"
    $targetXcframework = Join-Path $NativeDir "$FrameworkName-$Config.xcframework"

    if (-not (Test-Path $zipFile)) {
        Write-Error "Error: Zip file not found: $zipFile"
        exit 1
    }

    Write-Host "[$Config] Extracting $zipFile..."

    # 既存の xcframework を削除
    if (Test-Path $targetXcframework) {
        Write-Host "[$Config] Removing existing: $targetXcframework"
        Remove-Item -Path $targetXcframework -Recurse -Force
    }

    # 一時ディレクトリに展開
    $tempDir = Join-Path $NativeDir "temp_$Config"
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    # Zip を展開
    Expand-Archive -Path $zipFile -DestinationPath $tempDir -Force

    # xcframework を移動（Zipの中にはフレームワーク名.xcframeworkが含まれている）
    $extractedXcframework = Join-Path $tempDir "$FrameworkName.xcframework"
    if (Test-Path $extractedXcframework) {
        Move-Item -Path $extractedXcframework -Destination $targetXcframework -Force
    } else {
        Write-Error "Error: Expected xcframework not found after extraction: $extractedXcframework"
        Remove-Item -Path $tempDir -Recurse -Force
        exit 1
    }

    # 一時ディレクトリを削除
    Remove-Item -Path $tempDir -Recurse -Force

    if (-not (Test-Path $targetXcframework)) {
        Write-Error "Error: Target xcframework not found: $targetXcframework"
        exit 1
    }

    Write-Host "[$Config] Extracted to: $targetXcframework"
}

if ($extractDebug) {
    Extract-Config -Config "Debug" -FrameworkName $frameworkName -SourceDir $sourceDir -NativeDir $nativeDir
}

if ($extractRelease) {
    Extract-Config -Config "Release" -FrameworkName $frameworkName -SourceDir $sourceDir -NativeDir $nativeDir
}

Write-Host "Done."
