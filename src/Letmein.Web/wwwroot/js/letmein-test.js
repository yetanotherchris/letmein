class IndexView {

	constructor($, window) {
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

	showError(text) {
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
}