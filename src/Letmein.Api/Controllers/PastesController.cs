using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Letmein.Core;
using Letmein.Core.Services;
using Microsoft.AspNetCore.Mvc;
using ILetmeinConfiguration = Letmein.Core.Configuration.ILetmeinConfiguration;

namespace Letmein.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PastesController : ControllerBase
    {
        private readonly ITextEncryptionService _service;
        private readonly ILetmeinConfiguration _configuration;

        public PastesController(ITextEncryptionService service, ILetmeinConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        /// <summary>
        /// Get available expiry times
        /// </summary>
        [HttpGet("expiry-times")]
        public IActionResult GetExpiryTimes()
        {
            IEnumerable<int> expiryItems = _configuration.ExpiryTimes;
            var formattedItems = new Dictionary<int, string>();

            foreach (int expiry in expiryItems)
            {
                TimeSpan expiryTimeSpan = TimeSpan.FromMinutes(expiry);
                string displayText = FormatTimeSpan(expiryTimeSpan);
                formattedItems.Add(expiry, displayText);
            }

            return Ok(formattedItems);
        }

        /// <summary>
        /// Store encrypted content
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Store([FromBody] StoreRequest request)
        {
            if (string.IsNullOrEmpty(request.CipherJson))
            {
                return BadRequest(new { error = "The cipherJson is empty." });
            }

            if (!_configuration.ExpiryTimes.Contains(request.ExpiryTime))
            {
                return BadRequest(new { error = "That expiry time isn't supported." });
            }

            string friendlyId = await _service.StoredEncryptedJson(request.CipherJson, "", request.ExpiryTime);

            TimeSpan expireTimeSpan = TimeSpan.FromMinutes(request.ExpiryTime);

            return Ok(new
            {
                friendlyId = friendlyId,
                expiresIn = FormatTimeSpan(expireTimeSpan),
                expiryMinutes = request.ExpiryTime
            });
        }

        /// <summary>
        /// Load encrypted content by ID
        /// </summary>
        [HttpGet("{friendlyId}")]
        public async Task<IActionResult> Load(string friendlyId)
        {
            if (string.IsNullOrEmpty(friendlyId))
            {
                return BadRequest(new { error = "FriendlyId is required" });
            }

            EncryptedItem encryptedItem = await _service.LoadEncryptedJson(friendlyId);
            if (encryptedItem == null)
            {
                return NotFound(new { error = "The url is invalid or the paste has expired." });
            }

            if (encryptedItem.ExpiresOn <= DateTime.UtcNow)
            {
                return Gone(new { error = "The paste has expired." });
            }

            return Ok(new
            {
                friendlyId = encryptedItem.FriendlyId,
                cipherJson = encryptedItem.CipherJson,
                expiryDate = encryptedItem.ExpiresOn
            });
        }

        /// <summary>
        /// Delete a paste by ID
        /// </summary>
        [HttpDelete("{friendlyId}")]
        public async Task<IActionResult> Delete(string friendlyId)
        {
            if (string.IsNullOrEmpty(friendlyId))
            {
                return BadRequest(new { error = "FriendlyId is required" });
            }

            bool result = await _service.Delete(friendlyId);
            if (!result)
            {
                return NotFound(new { error = "Item not found or already deleted" });
            }

            return Ok(new { message = "Item deleted successfully" });
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
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

        private ObjectResult Gone(object value)
        {
            return StatusCode(410, value);
        }
    }

    public class StoreRequest
    {
        public string CipherJson { get; set; }
        public int ExpiryTime { get; set; }
    }
}
