# Run this manually when the JS updates on developments.
# Webpack is TODO instead of this

pushd wwwroot
node_modules/.bin/babel js/letmein.js -o js/prod/letmein.js
cat js/libraries/jquery-3.1.1.min.js, js/libraries/bootbox.min.js, js/libraries/bootstrap.min.js, js/libraries/clipboard.min.js, js/libraries/jquery.countdown.min.js, js/libraries/jquery.toast.js, js/libraries/sjcl.js >  js/prod/letmein.min.js
node_modules/.bin/uglifyjs js/prod/letmein.js >> js/prod/letmein.min.js
popd