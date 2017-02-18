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

var StoreView = (function ($) {
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
})();

var LoadView = (function ($, expiry) {
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
				$("#expiry-date")
					.on("finish.countdown",
						function () {
							document.location.reload();
						})
					.countdown(expiry, function (event) {
						$(this).text(
							event.strftime('Expires in %D days %H hours %M mins %S secs')
						);
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
})();
