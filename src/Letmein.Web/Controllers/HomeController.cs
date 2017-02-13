﻿using System;
using Letmein.Core;
using Letmein.Core.Services;
using Letmein.Web.Models;
using Microsoft.AspNetCore.Mvc;
using IConfiguration = Letmein.Core.Configuration.IConfiguration;

namespace Letmein.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly ITextEncryptionService _service;
		private readonly IConfiguration _configuration;

		public HomeController(ITextEncryptionService service, IConfiguration configuration)
		{
			_service = service;
			_configuration = configuration;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Store(string cipherJson)
		{
			if (string.IsNullOrEmpty(cipherJson))
			{
				ModelState.AddModelError("model", "The cipherJson is empty.");
				return View(nameof(Index));
			}

			string friendlyId = _service.StoredEncryptedJson(cipherJson, "");
			var model = new EncryptedItemViewModel() { FriendlyId = friendlyId };

			ViewData["BaseUrl"] = this.Request.Host;

			TimeSpan expireTimeSpan = TimeSpan.FromMinutes(_configuration.ExpirePastesAfter);

			if (expireTimeSpan.TotalHours < 1)
			{
				ViewData["ExpiresIn"] = expireTimeSpan.TotalMinutes + " minutes";
			}
			else
			{
				ViewData["ExpiresIn"] = expireTimeSpan.ToString("%h' hour(s) '%m' minute(s)'");
			}

			return View(model);
		}

		public IActionResult Load(string friendlyId)
		{
			if (string.IsNullOrEmpty(friendlyId))
			{
				return RedirectToAction(nameof(Index));
			}

			EncryptedItem encryptedItem = _service.LoadEncryptedJson(friendlyId);
			if (encryptedItem == null)
			{
				ModelState.AddModelError("Failure", "The url is invalid or the paste has expired.");
				return View();
			}

			if (encryptedItem.ExpiresOn <= DateTime.UtcNow)
			{
				ModelState.AddModelError("Failure", "The paste has expired.");
				return View();
			}

			var model = new EncryptedItemViewModel()
			{
				FriendlyId = encryptedItem.FriendlyId,
				CipherJson = encryptedItem.CipherJson,
				ExpiryDate = encryptedItem.ExpiresOn
			};

			return View(model);
		}

		public IActionResult Error()
		{
			return View();
		}
	}
}
