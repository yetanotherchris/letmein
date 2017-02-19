"use strict";

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

// npm install eslint --save-dev
// ./node_modules/.bin/eslint js/letmein-babel.js

// npm install --save-dev uglify-js
// npm install --save-dev babel-cli
// npm install babel-preset-env --save-dev
// ./node_modules/.bin/babel js/letmein.js -o js/prod/letmein.js
// ./node_modules/.bin/uglifyjs js/prod/letmein.js > js/prod/letmein.min.js
var IndexView = function () {
	function IndexView($, window) {
		var _this = this;

		_classCallCheck(this, IndexView);

		$(document).ready(function () {
			$("#text-textarea").focus();

			$("#encrypt-form").submit(function () {
				if ($("#text-textarea").val() === "") {
					_this.showError($, "Please enter some text.");
					return false;
				}

				if ($("#password-input").val().length < 5) {
					_this.showError($, "Please enter a password 5 or more characters long.");
					return false;
				}

				var text = $("#text-textarea").val();
				var password = $("#password-input").val();

				// Clear so they're not POST'd
				$("#text-textarea").val("");
				$("#password-input").val("");

				// Encrypt
				var json = window.sjcl.encrypt(password, text);
				$("#cipherJson").val(json);

				return true;
			});
		});
	}

	_createClass(IndexView, [{
		key: "showError",
		value: function showError($, text) {
			$.toast({
				heading: "Error",
				text: text,
				icon: "error",
				showHideTransition: "fade",
				allowToastClose: true,
				hideAfter: 2000,
				stack: 5,
				position: "top-center",
				textAlign: "left",
				loader: false
			});
		}
	}]);

	return IndexView;
}();

var StoreView = function StoreView($) {
	_classCallCheck(this, StoreView);

	$(document).ready(function () {
		var clipboard = new Clipboard("#copy-link");
		clipboard.on("success", function (e) {
			$.toast({
				text: "Copied to clipboard",
				heading: "Success",
				icon: "information",
				showHideTransition: "fade",
				allowToastClose: true,
				hideAfter: 2000,
				stack: 5,
				position: "top-center",
				textAlign: "left",
				loader: false
			});
		});
	});
};

var LoadView = function LoadView(window, $, expiry) {
	_classCallCheck(this, LoadView);

	$(document).ready(function () {
		$("#password-input").focus();

		$("#decrypt-form").submit(function () {
			return false;
		});

		$("#decrypt-button").click(function () {
			var cipherJson = $("#cipherJson").val();
			var password = $("#password-input").val();

			try {
				var text = window.sjcl.decrypt(password, cipherJson);
				$("#cipher-textarea").val(text);

				$("#expiry-date").show();
				$("#expiry-date").on("finish.countdown", function () {
					document.location.reload();
				}).countdown(expiry, function (event) {
					$(this).text(event.strftime("Expires in %D days %H hours %M mins %S secs"));
				});

				$.toast({
					heading: "Success",
					text: "Your note was decrypted.",
					icon: "success",
					showHideTransition: "fade",
					allowToastClose: true,
					hideAfter: 2000,
					stack: 5,
					position: "top-center",
					textAlign: "left",
					loader: false
				});
			} catch (err) {
				$.toast({
					heading: "Failure",
					text: "Unable to decrypt the note.",
					icon: "error",
					showHideTransition: "fade",
					allowToastClose: true,
					hideAfter: 2000,
					stack: 5,
					position: "top-center",
					textAlign: "left",
					loader: false
				});

				console.log(err);
			}
		});
	});
};
