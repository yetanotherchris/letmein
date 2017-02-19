./node_modules/.bin/babel js/letmein.js -o js/prod/letmein.js
./node_modules/.bin/uglifyjs js/prod/letmein.js js/libraries/*.js > js/prod/letmein.min.js

rm -rf src/Letmein.Web/wwwroot/js/libraries
rm src/Letmein.Web/wwwroot/js/letmein.js
rm src/Letmein.Web/wwwroot/.eslintrc.json
rm src/Letmein.Web/wwwroot/js/_references.js
rm src/Letmein.Web/wwwroot/js/.babelrc
rm src/Letmein.Web/wwwroot/js/package.json