$ErrorActionPreference = "Stop"

$Version = "1.0.5"
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
        "Releases/RELEASES",
        "Releases/releases.win.json",
        "Releases/assets.win.json"
    )

    foreach ($File in $Files) {
        if (Test-Path $File) {
            $FileName = [System.IO.Path]::GetFileName($File)
            Write-Host "➡️ Processing $FileName" -ForegroundColor Yellow

            Write-Host "   1. Requesting Upload URL..."
            
            $BodyObj = @{
                filename    = $FileName
                contentType = "application/octet-stream"
            } 
            $BodyJson = $BodyObj | ConvertTo-Json -Compress
            
            # Use local file to avoid temp path issues and ensure encoding
            $PayloadFile = "$PWD\payload.json"
            [System.IO.File]::WriteAllText($PayloadFile, $BodyJson, [System.Text.Encoding]::ASCII)
            
            $MaxRetries = 3
            $RetryDelaySeconds = 2
            $UploadUrl = $null
            $Success = $false

            for ($i = 0; $i -lt $MaxRetries; $i++) {
                try {
                    if ($i -gt 0) { Write-Host " (Retry $($i+1))..." -NoNewline }
                    
                    # Use curl.exe to get the URL
                    $UploadUrlResponse = & curl.exe -s -S -X POST "$ApiUrl/releases/upload-url" -H "Content-Type: application/json" -d "@$PayloadFile"
                    
                    if ($LASTEXITCODE -ne 0) { 
                        throw "Curl failed (Exit code $LASTEXITCODE)" 
                    }

                    $TrimmedResponse = $UploadUrlResponse.Trim()

                    # Basic JSON validation (Robust against non-JSON errors like "Server Error")
                    if (-not ($TrimmedResponse.StartsWith("{") -or $TrimmedResponse.StartsWith("["))) {
                        throw "Server returned non-JSON response: $TrimmedResponse"
                    }

                    $Response = $TrimmedResponse | ConvertFrom-Json
                    
                    # Handle if response is array or object
                    if ($Response -is [array]) {
                        $UploadUrl = $Response[0].url
                    }
                    else {
                        $UploadUrl = $Response.url
                    }
                    
                    if ([string]::IsNullOrWhiteSpace($UploadUrl)) {
                        throw "API returned empty URL or invalid structure."
                    }
                    
                    $Success = $true
                    break
                }
                catch {
                    if ($i -eq $MaxRetries - 1) {
                        Write-Host " Failed." -ForegroundColor Red
                        Write-Host "Last Error: $_" -ForegroundColor DarkRed
                        throw "Failed to get upload URL after $MaxRetries attempts: $_"
                    }
                    Start-Sleep -Seconds $RetryDelaySeconds
                }
            }
            
            if (Test-Path $PayloadFile) { Remove-Item $PayloadFile -ErrorAction SilentlyContinue }
            
            if ($Success) {
                Write-Host " Done." -ForegroundColor Green
            }

            # Step 2: Upload File content to the Signed URL
            Write-Host "   2. Uploading content..." -NoNewline
            
            # Using curl.exe with direct call operator '&' to handle argument quoting correctly
            & curl.exe -X "PUT" "$UploadUrl" -H "Content-Type: application/octet-stream" --data-binary "@$File" --fail --silent --show-error
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host " Done." -ForegroundColor Green
            }
            else {
                Write-Host " Failed (Exit Code: $LASTEXITCODE)." -ForegroundColor Red
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
    # Build-App
    # Pack-App
    
    # Restoring original flow
    Build-App
    Pack-App

    Upload-Files
    Write-Host "🎉 Deployment finished successfully!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
}
