$ErrorActionPreference = "Stop"

$Version = "1.0.2"
$ApiUrl = "https://mago-launcher-server.vercel.app"

function Build-App {
    Write-Host "🚧 Building Application..." -ForegroundColor Cyan
    dotnet publish -c Release --self-contained -r win-x64 -o publish
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    Write-Host "✅ Build Complete." -ForegroundColor Green
}

function Pack-App {
    Write-Host "📦 Packing Application..." -ForegroundColor Cyan
    if ((Get-Command "vpk" -ErrorAction SilentlyContinue) -eq $null) { 
        throw "vpk command not found. Please ensure Velopack/vpk is installed and in your PATH." 
    }
    
    vpk pack -u "MagoLauncher" -v "$Version" -p publish -e "MagoLauncher.Presentation.exe"
    if ($LASTEXITCODE -ne 0) { throw "Pack failed" }
    Write-Host "✅ Pack Complete." -ForegroundColor Green
}

function Upload-Files {
    Write-Host "🚀 Processing Uploads (Signed URL Method)..." -ForegroundColor Cyan
    
    $Files = @(
        "Releases/MagoLauncher-win-Setup.exe",
        "Releases/MagoLauncher-$Version-full.nupkg",
        "Releases/RELEASES"
    )

    foreach ($File in $Files) {
        if (Test-Path $File) {
            $FileName = [System.IO.Path]::GetFileName($File)
            Write-Host "➡️ Processing $FileName" -ForegroundColor Yellow

            # Step 1: Get Signed URL via API
            Write-Host "   1. Requesting Upload URL..." -NoNewline
            $Body = @{
                filename    = $FileName
                contentType = "application/octet-stream"
            } | ConvertTo-Json -Compress

            try {
                $Response = Invoke-RestMethod -Uri "$ApiUrl/releases/upload-url" -Method Post -Body $Body -ContentType "application/json"
                $UploadUrl = $Response.url
                
                if ([string]::IsNullOrWhiteSpace($UploadUrl)) {
                    throw "API returned empty URL."
                }
                Write-Host " Done." -ForegroundColor Green
            }
            catch {
                Write-Host " Failed." -ForegroundColor Red
                throw "Failed to get upload URL: $_"
            }

            # Step 2: Upload File content to the Signed URL
            Write-Host "   2. Uploading content..." -NoNewline
            
            # Using curl.exe ensures proper binary streaming and PUT method handling
            # -f (fail silently on server errors), -s (silent), -S (show error if failed), -L (follow redirects if any)
            $curlArgs = @(
                "-X", "PUT", 
                "$UploadUrl", 
                "-H", "Content-Type: application/octet-stream", 
                "--data-binary", "@$File",
                "--fail", "--silent", "--show-error"
            )
            
            $process = Start-Process -FilePath "curl.exe" -ArgumentList $curlArgs -Wait -NoNewWindow -PassThru
            
            if ($process.ExitCode -eq 0) {
                Write-Host " Done." -ForegroundColor Green
            }
            else {
                Write-Host " Failed (Exit Code: $($process.ExitCode))." -ForegroundColor Red
                throw "Upload failed for $File"
            }

        }
        else {
            Write-Warning "File not found: $File"
        }
    }
    Write-Host "✅ All Uploads Complete." -ForegroundColor Green
}

# Main execution flow
try {
    Build-App
    Pack-App
    Upload-Files
    Write-Host "🎉 Deployment finished successfully!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
}
