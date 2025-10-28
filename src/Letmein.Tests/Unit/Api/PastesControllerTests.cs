using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Letmein.Api.Controllers;
using Letmein.Core;
using Letmein.Core.Services;
using Letmein.Tests.Unit.MocksAndStubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;

namespace Letmein.Tests.Unit.Api
{
	public class PastesControllerTests
	{
		private PastesController _controller;
		private Mock<ITextEncryptionService> _encryptionService;
		private ConfigurationStub _configuration;

		public PastesControllerTests()
		{
			_configuration = new ConfigurationStub();
			_encryptionService = new Mock<ITextEncryptionService>();
			_controller = new PastesController(_encryptionService.Object, _configuration);
		}

		[Theory]
		[InlineData(31, "31 minutes")]
		[InlineData(60, "1 hour")]
		[InlineData(600, "10 hours")]
		[InlineData(61, "1 hour 1 minute")]
		[InlineData(62, "1 hour 2 minutes")]
		[InlineData(60 * 26, "1 day 2 hours")]
		public void GetExpiryTimes_should_return_formatted_expiry_times(int expiry, string displayText)
		{
			// Arrange
			_configuration.AddExpiryTime(expiry);

			// Act
			IActionResult actionResult = _controller.GetExpiryTimes();

			// Assert
			actionResult.ShouldNotBeNull();
			var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
			var model = okResult.Value as Dictionary<int, string>;
			model.ShouldNotBeNull();
			model[expiry].ShouldBe(displayText);
		}

		[Fact]
		public async Task Store_should_return_ok_with_friendlyid()
		{
			// Arrange
			int expiresInMinutes = 90;
			_configuration.AddExpiryTime(expiresInMinutes);

			string json = "{json}";
			_encryptionService.Setup(x => x.StoredEncryptedJson(json, "", expiresInMinutes))
							  .ReturnsAsync("the-friendlyid");

			var request = new StoreRequest
			{
				CipherJson = json,
				ExpiryTime = expiresInMinutes
			};

			// Act
			IActionResult actionResult = await _controller.Store(request);

			// Assert
			actionResult.ShouldNotBeNull();
			var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
			var response = okResult.Value;

			var friendlyId = response.GetType().GetProperty("friendlyId").GetValue(response);
			friendlyId.ShouldBe("the-friendlyid");

			var expiresIn = response.GetType().GetProperty("expiresIn").GetValue(response);
			expiresIn.ShouldBe("1 hour 30 minutes");
		}

		[Theory]
		[InlineData(90, "1 hour 30 minutes")]
		[InlineData(58, "58 minutes")]
		public async Task Store_should_return_correct_expiresin_time(int minutes, string expectedExpiresIn)
		{
			// Arrange
			_configuration.AddExpiryTime(minutes);
			_encryptionService.Setup(x => x.StoredEncryptedJson(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
							  .ReturnsAsync("some-id");

			var request = new StoreRequest
			{
				CipherJson = "do you know jason?",
				ExpiryTime = minutes
			};

			// Act
			IActionResult actionResult = await _controller.Store(request);

			// Assert
			actionResult.ShouldNotBeNull();
			var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
			var response = okResult.Value;

			var expiresIn = response.GetType().GetProperty("expiresIn").GetValue(response);
			expiresIn.ShouldBe(expectedExpiresIn);
		}

		[Fact]
		public async Task Store_should_return_badrequest_when_cipherJson_is_empty()
		{
			// Arrange
			_configuration.AddExpiryTime(10);

			var request = new StoreRequest
			{
				CipherJson = "",
				ExpiryTime = 10
			};

			// Act
			IActionResult actionResult = await _controller.Store(request);

			// Assert
			actionResult.ShouldNotBeNull();
			actionResult.ShouldBeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Store_should_return_badrequest_when_expirytime_is_not_in_configuration()
		{
			// Arrange
			int badExpiryTime = 1;
			_configuration.AddExpiryTime(10);

			var request = new StoreRequest
			{
				CipherJson = "{ some: json }",
				ExpiryTime = badExpiryTime
			};

			// Act
			IActionResult actionResult = await _controller.Store(request);

			// Assert
			actionResult.ShouldNotBeNull();
			actionResult.ShouldBeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Load_should_return_ok_with_encrypted_item()
		{
			// Arrange
			var expectedItem = new EncryptedItem()
			{
				FriendlyId = "the-friendlyid",
				CipherJson = "{json}",
				ExpiresOn = DateTime.UtcNow.AddDays(1)
			};

			_encryptionService.Setup(x => x.LoadEncryptedJson("the-friendlyid")).ReturnsAsync(expectedItem);

			// Act
			IActionResult actionResult = await _controller.Load("the-friendlyid");

			// Assert
			actionResult.ShouldNotBeNull();
			var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
			var response = okResult.Value;

			var friendlyId = response.GetType().GetProperty("friendlyId").GetValue(response);
			friendlyId.ShouldBe(expectedItem.FriendlyId);

			var cipherJson = response.GetType().GetProperty("cipherJson").GetValue(response);
			cipherJson.ShouldBe(expectedItem.CipherJson);
		}

		[Fact]
		public async Task Load_should_return_badrequest_when_friendlyid_is_empty()
		{
			// Arrange + Act
			IActionResult actionResult = await _controller.Load("");

			// Assert
			actionResult.ShouldNotBeNull();
			actionResult.ShouldBeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Load_should_return_notfound_when_service_returns_null()
		{
			// Arrange
			_encryptionService.Setup(x => x.LoadEncryptedJson("the-friendlyid"))
							  .ReturnsAsync((EncryptedItem)null);

			// Act
			IActionResult actionResult = await _controller.Load("the-friendlyid");

			// Assert
			actionResult.ShouldNotBeNull();
			actionResult.ShouldBeOfType<NotFoundObjectResult>();
		}

		[Fact]
		public async Task Load_should_return_410_gone_when_item_is_expired()
		{
			// Arrange
			var expiredItem = new EncryptedItem()
			{
				FriendlyId = "the-friendlyid",
				CipherJson = "{json}",
				ExpiresOn = DateTime.UtcNow.AddYears(-1)
			};

			_encryptionService.Setup(x => x.LoadEncryptedJson("the-friendlyid")).ReturnsAsync(expiredItem);

			// Act
			IActionResult actionResult = await _controller.Load("the-friendlyid");

			// Assert
			actionResult.ShouldNotBeNull();
			var statusResult = actionResult.ShouldBeOfType<ObjectResult>();
			statusResult.StatusCode.ShouldBe(410);
		}

		[Fact]
		public async Task Delete_should_return_ok_when_successful()
		{
			// Arrange
			_encryptionService.Setup(x => x.Delete("the-friendlyid")).ReturnsAsync(true);

			// Act
			IActionResult actionResult = await _controller.Delete("the-friendlyid");

			// Assert
			actionResult.ShouldNotBeNull();
			actionResult.ShouldBeOfType<OkObjectResult>();
			_encryptionService.Verify(x => x.Delete("the-friendlyid"));
		}

		[Fact]
		public async Task Delete_should_return_notfound_when_service_fails()
		{
			// Arrange
			_encryptionService.Setup(x => x.Delete("the-friendlyid")).ReturnsAsync(false);

			// Act
			IActionResult actionResult = await _controller.Delete("the-friendlyid");

			// Assert
			actionResult.ShouldNotBeNull();
			actionResult.ShouldBeOfType<NotFoundObjectResult>();
		}

		[Fact]
		public async Task Delete_should_return_badrequest_when_friendlyid_is_empty()
		{
			// Arrange + Act
			IActionResult actionResult = await _controller.Delete("");

			// Assert
			actionResult.ShouldNotBeNull();
			actionResult.ShouldBeOfType<BadRequestObjectResult>();
		}
	}
}
