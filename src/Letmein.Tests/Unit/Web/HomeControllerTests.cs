using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Letmein.Core;
using Letmein.Core.Services;
using Letmein.Tests.Unit.MocksAndStubs;
using Letmein.Web.Controllers;
using Letmein.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;

namespace Letmein.Tests.Unit.Web
{
	public class HomeControllerTests
	{
		private HomeController _controller;
		private Mock<ITextEncryptionService> _encryptionService;
		private ConfigurationStub _configuration;

		public HomeControllerTests()
		{
			var httpcontext = new DefaultHttpContext();
			var request = new DefaultHttpRequest(httpcontext);
			request.Host = new HostString("localhost");

			_configuration = new ConfigurationStub();
			_encryptionService = new Mock<ITextEncryptionService>();
			_controller = new HomeController(_encryptionService.Object, _configuration);
			_controller.ControllerContext = new ControllerContext();
			_controller.ControllerContext.HttpContext = httpcontext;
		}

		[Theory]
		[InlineData(31, "31 minutes")]
		[InlineData(60, "1 hour")]
		[InlineData(600, "10 hours")]
		[InlineData(61, "1 hour 1 minute")]
		[InlineData(62, "1 hour 2 minutes")]
		[InlineData(60 * 26, "1 day 2 hours")]
		public void Index_should_return_view_and_model_with_formatted_expiry_times(int expiry, string displayText)
		{
			// Arrange
			_configuration.AddExpiryTime(expiry);

			// Act
			ViewResult result = _controller.Index() as ViewResult;

			// Assert
			result.ShouldNotBeNull();

			Dictionary<int, string> model = result.Model as Dictionary<int, string>;
			model.ShouldNotBeNull();
			model[expiry].ShouldBe(displayText);
		}

		[Fact]
		public async Task Store_should_Store_and_return_model_with_new_friendlyid()
		{
			// Arrange
			int expiresInMinutes = 90;
			_configuration.AddExpiryTime(expiresInMinutes);

			string json = "{json}";
			_encryptionService.Setup(x => x.StoredEncryptedJson(json, "", expiresInMinutes))
							  .ReturnsAsync("the-friendlyid");

			// Act
			ViewResult result = await _controller.Store(json, expiresInMinutes) as ViewResult;

			// Assert
			result.ShouldNotBeNull();

			EncryptedItemViewModel model = result.Model as EncryptedItemViewModel;
			model.ShouldNotBeNull();
			model.FriendlyId.ShouldBe("the-friendlyid");
			_controller.ViewData["BaseUrl"].ToString().ShouldBe("localhost");
			_controller.ViewData["ExpiresIn"].ToString().ShouldBe("1 hour 30 minutes");
		}

		[Theory]
		[InlineData(90, "1 hour 30 minutes")]
		[InlineData(58, "58 minutes")]
		public async Task Store_should_Store_fill_expiresin_view_data(int minutes, string expectedViewData)
		{
			// Arrange
			_configuration.AddExpiryTime(minutes);

			// Act
			ViewResult result = await _controller.Store("do you know jason?", minutes) as ViewResult;

			// Assert
			result.ShouldNotBeNull();
			_controller.ViewData["ExpiresIn"].ToString().ShouldBe(expectedViewData);
		}

		[Fact]
		public async Task Store_should_set_modelstate_errors_and_return_error_view_when_cipherJson_is_empty()
		{
			// Arrange
			_configuration.AddExpiryTime(10);

			// Act
			ViewResult result = await _controller.Store("", 1) as ViewResult;

			// Assert
			result.ShouldNotBeNull();
			_controller.ModelState.Count.ShouldBe(1);
		}

		[Fact]
		public async Task Store_should_set_modelstate_errors_and_return_error_view_when_expirytime_is_not_in_configuration()
		{
			// Arrange
			int badExpiryTime = 1;
			_configuration.AddExpiryTime(10);

			// Act
			ViewResult result = await _controller.Store("{ some: json }", badExpiryTime) as ViewResult;

			// Assert
			result.ShouldNotBeNull();
			result.ViewName.ShouldBe(nameof(HomeController.Error));
			_controller.ModelState.Count.ShouldBe(1);
		}

		[Fact]
		public async Task Load_should_return_model_using_service()
		{
			// Arrange
			var expectedItem = new EncryptedItem()
			{
				FriendlyId = "the-friendlyid",
				CipherJson = "{json}",
				ExpiresOn = DateTime.Today.AddDays(1)
			};

			_encryptionService.Setup(x => x.LoadEncryptedJson("the-friendlyid")).ReturnsAsync(expectedItem);

			// Act
			ViewResult result = await _controller.Load("the-friendlyid") as ViewResult;

			// Assert
			result.ShouldNotBeNull();

			EncryptedItemViewModel model = result.Model as EncryptedItemViewModel;
			model.ShouldNotBeNull();
			model.FriendlyId.ShouldBe(expectedItem.FriendlyId);
			model.CipherJson.ShouldBe(expectedItem.CipherJson);
		}

		[Fact]
		public async Task Load_should_return_redirectresult_when_friendlyid_is_empty()
		{
			// Arrange + Act
			RedirectToActionResult result = await _controller.Load("") as RedirectToActionResult;

			// Assert
			result.ShouldNotBeNull();
			result.ActionName.ShouldBe(nameof(HomeController.Index));
		}

		[Fact]
		public async Task Load_should_set_modelstate_errors_and_return_null_model_when_service_returns_null()
		{
			// Arrange
			_encryptionService.Setup(x => x.LoadEncryptedJson("the-friendlyid"))
							  .ReturnsAsync((EncryptedItem)null);

			// Act
			ViewResult result = await _controller.Load("the-friendlyid") as ViewResult;

			// Assert
			result.ShouldNotBeNull();

			EncryptedItemViewModel model = result.Model as EncryptedItemViewModel;
			model.ShouldBeNull();
			_controller.ModelState.Count.ShouldBe(1);
		}

		[Fact]
		public async Task Load_should_set_modelstate_errors_and_return_null_model_when_item_is_expired()
		{
			// Arrange
			var expectedItem = new EncryptedItem()
			{
				FriendlyId = "the-friendlyid",
				CipherJson = "{json}",
				ExpiresOn = DateTime.Today.AddYears(-1)
			};

			_encryptionService.Setup(x => x.LoadEncryptedJson("the-friendlyid")).ReturnsAsync(expectedItem);

			// Act
			ViewResult result = await _controller.Load("the-friendlyid") as ViewResult;

			// Assert
			result.ShouldNotBeNull();

			EncryptedItemViewModel model = result.Model as EncryptedItemViewModel;
			model.ShouldBeNull();
			_controller.ModelState.Count.ShouldBe(1);
		}

		[Fact]
		public async Task Delete_should_remove_item_and_redirect_to_index()
		{
			// Arrange
			_encryptionService.Setup(x => x.Delete("the-friendlyid")).ReturnsAsync(true);

			// Act
			ViewResult result = await _controller.Delete("the-friendlyid") as ViewResult;

			// Assert
			result.ShouldNotBeNull();
			result.ViewName.ShouldBe("Deleted");
			_encryptionService.Verify(x => x.Delete("the-friendlyid"));
		}

		[Fact]
		public async Task Delete_should_redirect_to_load_page_when_service_fails()
		{
			// Arrange
			_encryptionService.Setup(x => x.Delete("the-friendlyid")).ReturnsAsync(false);

			// Act
			RedirectToActionResult result = await _controller.Delete("the-friendlyid") as RedirectToActionResult;

			// Assert
			result.ShouldNotBeNull();
			result.ActionName.ShouldBe(nameof(HomeController.Load));
			result.RouteValues["friendlyId"].ShouldBe("the-friendlyid");
		}

		[Fact]
		public async Task Delete_should_redirect_when_friendlyid_is_empty()
		{
			// Arrange + Act
			RedirectToActionResult result = await _controller.Delete("") as RedirectToActionResult;

			// Assert
			result.ShouldNotBeNull();
			result.ActionName.ShouldBe(nameof(HomeController.Index));
		}
	}
}