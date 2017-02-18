"use strict";

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

var IndexView = function () {
	function IndexView($, window) {
		_classCallCheck(this, IndexView);

		console.log($);
		$(document).ready(function () {
			$("#text-textarea").focus();

			$("#encrypt-form").submit(function () {
				if ($("#text-textarea").val() === "") {
					showError("Please enter some text.");
					return false;
				}

				if ($("#password-input").val().length < 5) {
					showError("Please enter a password 5 or more characters long.");
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
		value: function showError(text) {
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