using System;
using System.Collections.Generic;
using Letmein.Core;
using Letmein.Core.Services;
using Letmein.Tests.Unit.MocksAndStubs;
using Letmein.Web.Controllers;
using Letmein.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Letmein.Tests.Unit.Web
{
	[TestFixture]
	public class HomeControllerTests
	{
		private HomeController _controller;
		private Mock<ITextEncryptionService> _encryptionService;
		private ConfigurationStub _configuration;

		[SetUp]
		public void Setup()
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

		[Test]
		[TestCase(31, "31 minutes")]
		[TestCase(60, "1 hour")]
		[TestCase(600, "10 hours")]
		[TestCase(61, "1 hour 1 minute")]
		[TestCase(62, "1 hour 2 minutes")]
		[TestCase(60*26, "1 day 2 hours")]
		public void Index_should_return_view_and_model_with_formatted_expiry_times(int expiry, string displayText)
		{
			// Arrange
			_configuration.AddExpiryTime(expiry);

			// Act
			ViewResult result = _controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			Dictionary<int, string> model = result.Model as Dictionary<int, string>;
			Assert.That(model, Is.Not.Null);
			Assert.That(model[expiry], Is.EqualTo(displayText));
		}

		[Test]
		public void Store_should_Store_and_return_model_with_new_friendlyid()
		{
			// Arrange
			int expiresInMinutes = 90;
			_configuration.AddExpiryTime(expiresInMinutes);

			string json = "{json}";
			_encryptionService.Setup(x => x.StoredEncryptedJson(json, "", expiresInMinutes)).Returns("the-friendlyid");

			// Act
			ViewResult result = _controller.Store(json, expiresInMinutes) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			EncryptedItemViewModel model = result.Model as EncryptedItemViewModel;
			Assert.That(model, Is.Not.Null);
			Assert.That(model.FriendlyId, Is.EqualTo("the-friendlyid"));
			Assert.That(_controller.ViewData["BaseUrl"].ToString(), Is.EqualTo("localhost"));
			Assert.That(_controller.ViewData["ExpiresIn"].ToString(), Is.EqualTo("1 hour 30 minutes"));
		}

		[Test]
		[TestCase(90, "1 hour 30 minutes")]
		[TestCase(58, "58 minutes")]
		public void Store_should_Store_fill_expiresin_view_data(int minutes, string expectedViewData)
		{
			// Arrange
			_configuration.AddExpiryTime(minutes);

			// Act
			ViewResult result = _controller.Store("do you know jason?", minutes) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(_controller.ViewData["ExpiresIn"].ToString(), Is.EqualTo(expectedViewData));
		}

		[Test]
		public void Store_should_set_modelstate_errors_and_return_index_when_cipherJson_is_empty()
		{
			// Arrange
			_configuration.AddExpiryTime(10);

			// Act
			ViewResult result = _controller.Store("", 1) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(_controller.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Store_should_return_index_when_expirytime_is_in_configuration()
		{
			// Arrange
			_configuration.AddExpiryTime(10);

			// Act
			ViewResult result = _controller.Store("{ some: json }", 1) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ViewName, Is.EqualTo(nameof(HomeController.Index)));
		}

		[Test]
		public void Load_should_return_model_using_service()
		{
			// Arrange
			var expectedItem = new EncryptedItem()
			{
				FriendlyId = "the-friendlyid",
				CipherJson = "{json}",
				ExpiresOn = DateTime.Today.AddDays(1)
			};

			_encryptionService.Setup(x => x.LoadEncryptedJson("the-friendlyid")).Returns(expectedItem);

			// Act
			ViewResult result = _controller.Load("the-friendlyid") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			EncryptedItemViewModel model = result.Model as EncryptedItemViewModel;
			Assert.That(model, Is.Not.Null);
			Assert.That(model.FriendlyId, Is.EqualTo(expectedItem.FriendlyId));
			Assert.That(model.CipherJson, Is.EqualTo(expectedItem.CipherJson));
		}

		[Test]
		public void Load_should_return_redirectresult_when_friendlyid_is_empty()
		{
			// Arrange + Act
			RedirectToActionResult result = _controller.Load("") as RedirectToActionResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ActionName, Is.EqualTo(nameof(HomeController.Index)));
		}

		[Test]
		public void Load_should_set_modelstate_errors_and_return_null_model_when_service_returns_null()
		{
			// Arrange
			_encryptionService.Setup(x => x.LoadEncryptedJson("the-friendlyid")).Returns<EncryptedItem>(null);

			// Act
			ViewResult result = _controller.Load("the-friendlyid") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			EncryptedItemViewModel model = result.Model as EncryptedItemViewModel;
			Assert.That(model, Is.Null);
			Assert.That(_controller.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Load_should_set_modelstate_errors_and_return_null_model_when_item_is_expired()
		{
			// Arrange
			var expectedItem = new EncryptedItem()
			{
				FriendlyId = "the-friendlyid",
				CipherJson = "{json}",
				ExpiresOn = DateTime.Today.AddYears(-1)
			};

			_encryptionService.Setup(x => x.LoadEncryptedJson("the-friendlyid")).Returns(expectedItem);

			// Act
			ViewResult result = _controller.Load("the-friendlyid") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			EncryptedItemViewModel model = result.Model as EncryptedItemViewModel;
			Assert.That(model, Is.Null);
			Assert.That(_controller.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Delete_should_remove_item_and_redirect_to_index()
		{
			// Arrange
			_encryptionService.Setup(x => x.Delete("the-friendlyid")).Returns(true);

			// Act
			ViewResult result = _controller.Delete("the-friendlyid") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ViewName, Is.EqualTo("Deleted"));
			_encryptionService.Verify(x => x.Delete("the-friendlyid"));
		}

		[Test]
		public void Delete_should_redirect_to_load_page_when_service_fails()
		{
			// Arrange
			_encryptionService.Setup(x => x.Delete("the-friendlyid")).Returns(false);

			// Act
			RedirectToActionResult result = _controller.Delete("the-friendlyid") as RedirectToActionResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ActionName, Is.EqualTo(nameof(HomeController.Load)));
			Assert.That(result.RouteValues["friendlyId"], Is.EqualTo("the-friendlyid"));
		}

		[Test]
		public void Delete_should_redirect_when_friendlyid_is_empty()
		{
			// Arrange + Act
			RedirectToActionResult result = _controller.Delete("") as RedirectToActionResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ActionName, Is.EqualTo(nameof(HomeController.Index)));
		}
	}
}