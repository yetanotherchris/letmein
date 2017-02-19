./node_modules/.bin/babel ./src/Letmein.Web/wwwroot/js/letmein.js -o ./src/Letmein.Web/wwwroot/js/prod/letmein.js
./node_modules/.bin/uglifyjs ./src/Letmein.Web/wwwroot/js/prod/letmein.js ./src/Letmein.Web/wwwroot/js/libraries/*.js > ./src/Letmein.Web/wwwroot/js/prod/letmein.min.js

rm -rf ./src/Letmein.Web/wwwroot/js/libraries
rm ./src/Letmein.Web/wwwroot/js/letmein.js
rm ./src/Letmein.Web/wwwroot/.eslintrc.json
rm ./src/Letmein.Web/wwwroot/_references.js
rm ./src/Letmein.Web/wwwroot/.babelrc
rm ./src/Letmein.Web/wwwroot/package.json