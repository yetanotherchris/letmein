using Letmein.Core;
using Letmein.Core.Services;
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

		[SetUp]
		public void Setup()
		{
			var httpcontext = new DefaultHttpContext();
			var request = new DefaultHttpRequest(httpcontext);
			request.Host = new HostString("localhost");

			_encryptionService = new Mock<ITextEncryptionService>();
			_controller = new HomeController(_encryptionService.Object);
			_controller.ControllerContext = new ControllerContext();
			_controller.ControllerContext.HttpContext = httpcontext;
		}

		[Test]
		public void Index_should_return_view()
		{
			// Arrange + Act
			ViewResult result = _controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Store_should_Store_and_return_model_with_new_friendlyid()
		{
			// Arrange
			string json = "{json}";
			_encryptionService.Setup(x => x.StoredEncryptedJson(json, "")).Returns("the-friendlyid");

			// Act
			ViewResult result = _controller.Store(json) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			EncryptedItemViewModel model = result.Model as EncryptedItemViewModel;
			Assert.That(model, Is.Not.Null);
			Assert.That(model.FriendlyId, Is.EqualTo("the-friendlyid"));
		}

		[Test]
		public void Store_should_set_modelstate_errors_and_return_index_when_cipherJson_is_empty()
		{
			// Arrange + Act
			ViewResult result = _controller.Store("") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(_controller.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Load_should_return_model_using_service()
		{
			// Arrange
			var expectedItem = new EncryptedItem()
			{
				FriendlyId = "the-friendlyid",
				CipherJson = "{json}"
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
	}
}