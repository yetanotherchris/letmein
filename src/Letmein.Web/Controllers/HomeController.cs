using System;
using System.Collections.Generic;
using System.Linq;
using Letmein.Core;
using Letmein.Core.Services;
using Letmein.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IConfiguration = Letmein.Core.Configuration.IConfiguration;
using StructureMap.TypeRules;
using System.Threading.Tasks;

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

		public IActionResult FAQ()
		{
			return View();
		}

		public IActionResult Index()
		{
			// Model the times as nicely formatted minutes/hours
			IEnumerable<int> expiryItems = _configuration.ExpiryTimes;
			var formattedItems = new Dictionary<int, string>();

			foreach (int expiry in expiryItems)
			{
				TimeSpan expiryTimeSpan = TimeSpan.FromMinutes(expiry);
				string displayText = FormatTimeSpan(expiryTimeSpan);
				formattedItems.Add(expiry, displayText);
			}

			return View(formattedItems);
		}

		public string FormatTimeSpan(TimeSpan timeSpan)
		{
			timeSpan = timeSpan.Duration();

			string output = string.Format("{0} {1} {2}",
							GetDurationText(timeSpan.Days, "day"),
							GetDurationText(timeSpan.Hours, "hour"),
							GetDurationText(timeSpan.Minutes, "minute"));

			return output.Trim();
		}

		private string GetDurationText(int amount, string unit)
		{
			if (amount == 0)
				return "";

			return string.Format("{0} {1}{2}", amount, unit, (amount > 1) ? "s" : "");
		}

		[HttpPost]
		public async Task<IActionResult> Store(string cipherJson, int expiryTime)
		{
			if (string.IsNullOrEmpty(cipherJson))
			{
				ModelState.AddModelError("model", "The cipherJson is empty. Is Javascript enabled, or is the letmein.js script loaded?");
				return View("Error");
			}

			if (!_configuration.ExpiryTimes.Contains(expiryTime))
			{
				ModelState.AddModelError("model", "That expiry time isn't supported.");
				return View("Error");
			}

			string friendlyId = await _service.StoredEncryptedJson(cipherJson, "", expiryTime);
			var model = new EncryptedItemViewModel() { FriendlyId = friendlyId };

			TimeSpan expireTimeSpan = TimeSpan.FromMinutes(expiryTime);
			ViewData["ExpiresIn"] = FormatTimeSpan(expireTimeSpan);
			ViewData["BaseUrl"] = Request.Host;

			return View(model);
		}

		public async Task<IActionResult> Load(string friendlyId)
		{
			if (string.IsNullOrEmpty(friendlyId))
			{
				return RedirectToAction(nameof(HomeController.Index));
			}

			EncryptedItem encryptedItem = await _service.LoadEncryptedJson(friendlyId);
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
		public async Task<IActionResult> Delete(string friendlyId)
		{
			if (string.IsNullOrEmpty(friendlyId))
			{
				return RedirectToAction(nameof(Index));
			}

			bool result = await _service.Delete(friendlyId);
			if (!result)
			{
				return RedirectToAction(nameof(Load), new { friendlyid = friendlyId });
			}

			return View("Deleted");
		}

		public IActionResult Error()
		{
			return View();
		}
	}
}