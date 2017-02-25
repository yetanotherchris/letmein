﻿using System;
using System.Collections.Generic;
using Letmein.Core;
using Letmein.Core.Services;
using Letmein.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IConfiguration = Letmein.Core.Configuration.IConfiguration;
using StructureMap.TypeRules;

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

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			base.OnActionExecuted(context);
			ViewData["Version"] = typeof(HomeController).GetAssembly().GetName().Version.ToString(3);
		}

		public IActionResult Index()
		{
			// Model the times as nicely formatted minutes/hours
			IEnumerable<int> expiryItems = _configuration.ExpiryTimes;
			var formattedItems = new Dictionary<int, string>();

			foreach (int expiry in expiryItems)
			{
				string displayText = $"{expiry} minutes";
				if (expiry > 59)
				{
					if (expiry % 60 == 0)
					{
						int hours = expiry / 60;
						displayText = $"{hours} hour";
						displayText += (hours > 1) ? "s" : "";
					}
					else
					{
						displayText = TimeSpan.FromMinutes(expiry).ToString("%h' hour(s) '%m' minute(s)'");
					}
				}

				formattedItems.Add(expiry, displayText);
			}

			return View(formattedItems);
		}

		[HttpPost]
		public IActionResult Store(string cipherJson)
		{
			if (string.IsNullOrEmpty(cipherJson))
			{
				ModelState.AddModelError("model", "The cipherJson is empty. Is Javascript enabled, or is the script loaded?");
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

		[HttpPost]
		public IActionResult Delete(string friendlyid)
		{
			if (string.IsNullOrEmpty(friendlyid))
				return RedirectToAction(nameof(Index));

			bool result = _service.Delete(friendlyid);
			if (!result)
			{
				return RedirectToAction(nameof(Load), new { friendlyid = friendlyid });
			}

			return View("Deleted");
		}

		public IActionResult Error()
		{
			return View();
		}
	}
}
