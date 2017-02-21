class IndexView {

	constructor($, window) {

		$(document).ready(() => {
			$("#text-textarea").focus();

			$("#encrypt-form").submit(() => {
				if ($("#text-textarea").val() === "") {
					this.showError($, "Please enter some text.");
					return false;
				}

				if ($("#password-input").val().length < 5) {
					this.showError($, "Please enter a password 5 or more characters long.");
					return false;
				}

				var text = $("#text-textarea").val();
				var password = $("#password-input").val();

				// Clear so they"re not POST"d
				$("#text-textarea").val("");
				$("#password-input").val("");

				// Encrypt
				var json = window.sjcl.encrypt(password, text);
				$("#cipherJson").val(json);

				return true;
			});
		});
	}

	showError($, text) {
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

class StoreView {

	constructor($) {
		$(document).ready(() => {
			var clipboard = new Clipboard("#copy-link");
			clipboard.on("success", (e) => {
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
	}
}

class LoadView {

	constructor(window, $, expiry) {
		$(document).ready(() => {
			$("#password-input").focus();

			$("#decrypt-form").submit(() => {
				return false;
			});

			$("#delete-button").click(() => {
				bootbox.confirm({
					message: "Are you sure you want to delete this paste?",
					buttons: {
						confirm: {
							label: "Yes",
							className: "btn-success"
						},
						cancel: {
							label: "No",
							className: "btn-danger"
						}
					},
					callback: (result) => {
						if (result) {
							$("#delete-form").submit();
						}
					}
				});
			});

			$("#decrypt-button").click(() => {
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
								event.strftime("Expires in %D days %H hours %M mins %S secs")
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

					$("#load-help").hide();
					$("#delete-button").removeClass("hidden");
					$("#cipher-textarea").fadeIn(500);
					$("#password-input").hide();
					$("#decrypt-button").hide();

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
	}
}