Write-Host "You need NPM to develop letmein.io. To install on Windows, use choco install nodejs"

pushd src/Letmein.web/wwwroot
npm install uglify-js
npm install babel-cli
npm install babel-preset-env
popd