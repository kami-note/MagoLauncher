$ErrorActionPreference = "Stop"

$Version = "1.0.1"
$ApiUrl = "https://mago-launcher-server.vercel.app"

function Build-App {
    Write-Host "🚧 Building Application..." -ForegroundColor Cyan
    dotnet publish -c Release --self-contained -r win-x64 -o publish
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    Write-Host "✅ Build Complete." -ForegroundColor Green
}

function Pack-App {
    Write-Host "📦 Packing Application..." -ForegroundColor Cyan
    # Check if vpk is installed or accessible
    if ((Get-Command "vpk" -ErrorAction SilentlyContinue) -eq $null) { 
        throw "vpk command not found. Please ensure Velopack/vpk is installed and in your PATH." 
    }
    
    vpk pack -u "MagoLauncher" -v "$Version" -p publish -e "MagoLauncher.Presentation.exe"
    if ($LASTEXITCODE -ne 0) { throw "Pack failed" }
    Write-Host "✅ Pack Complete." -ForegroundColor Green
}

function Upload-Files {
    Write-Host "🚀 Uploading Files to $ApiUrl..." -ForegroundColor Cyan
    
    $Files = @(
        "Releases/MagoLauncher-win-Setup.exe",
        "Releases/MagoLauncher-$Version-full.nupkg",
        "Releases/RELEASES"
    )

    foreach ($File in $Files) {
        if (Test-Path $File) {
            Write-Host "  Uploading $File..." -NoNewline
            
            # Using curl.exe for reliability with multipart/form-data across different PS versions
            # PowerShell's native Invoke-RestMethod for multipart is simpler in PS 7+ but complex in 5.1
            & curl.exe -X POST "$ApiUrl/releases" -F "file=@$File"
            
            if ($LASTEXITCODE -ne 0) { 
                Write-Host " ❌ Failed" -ForegroundColor Red
                throw "Upload failed for $File" 
            } else {
                Write-Host "" # Newline after curl output
            }
        } else {
            Write-Warning "File not found: $File"
        }
    }
    Write-Host "✅ Upload Complete." -ForegroundColor Green
}

# Main execution flow
try {
    Build-App
    Pack-App
    Upload-Files
    Write-Host "🎉 Deployment finished successfully!" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
}
