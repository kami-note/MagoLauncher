VERSION = 1.0.1
API_URL = https://mago-launcher-server.vercel.app

deploy: build pack upload

build:
	dotnet publish -c Release --self-contained -r win-x64 -o publish

pack:
	vpk pack -u "MagoLauncher" -v "$(VERSION)" -p publish -e "MagoLauncher.Presentation.exe"

list-releases:
	curl -X GET "$(API_URL)/releases"

upload:
	curl -X POST "$(API_URL)/releases" -F "file=@Releases/MagoLauncher-win-Setup.exe"
	curl -X POST "$(API_URL)/releases" -F "file=@Releases/MagoLauncher-$(VERSION)-full.nupkg"
	curl -X POST "$(API_URL)/releases" -F "file=@Releases/RELEASES"