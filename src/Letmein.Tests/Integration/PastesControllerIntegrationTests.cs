using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Letmein.Tests.Integration
{
    public class PastesControllerIntegrationTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly ApiWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public PastesControllerIntegrationTests(ApiWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetExpiryTimes_ReturnsOkWithFormattedExpiryTimes()
        {
            // Act
            var response = await _client.GetAsync("/api/pastes/expiry-times");
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

            var expiryTimes = JsonSerializer.Deserialize<Dictionary<string, string>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            expiryTimes.ShouldNotBeNull();
            expiryTimes.ShouldContainKey("60");
            expiryTimes.ShouldContainKey("360");
            expiryTimes.ShouldContainKey("1440");

            expiryTimes["60"].ShouldBe("1 hour");
            expiryTimes["360"].ShouldBe("6 hours");
            expiryTimes["1440"].ShouldBe("1 day");
        }

        [Fact]
        public async Task PostStore_WithValidData_ReturnsOkWithFriendlyId()
        {
            // Arrange
            var storeRequest = new
            {
                cipherJson = "{\"iv\":\"test\",\"ct\":\"encrypted-data\"}",
                expiryTime = 60
            };

            var json = JsonSerializer.Serialize(storeRequest);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/pastes", httpContent);
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.GetProperty("friendlyId").GetString().ShouldNotBeNullOrEmpty();
            result.GetProperty("expiresIn").GetString().ShouldBe("1 hour");
            result.GetProperty("expiryMinutes").GetInt32().ShouldBe(60);
        }

        [Fact]
        public async Task PostStore_WithEmptyCipherJson_ReturnsBadRequest()
        {
            // Arrange
            var storeRequest = new
            {
                cipherJson = "",
                expiryTime = 60
            };

            var json = JsonSerializer.Serialize(storeRequest);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/pastes", httpContent);
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.GetProperty("error").GetString().ShouldBe("The cipherJson is empty.");
        }

        [Fact]
        public async Task PostStore_WithUnsupportedExpiryTime_ReturnsBadRequest()
        {
            // Arrange
            var storeRequest = new
            {
                cipherJson = "{\"iv\":\"test\",\"ct\":\"encrypted-data\"}",
                expiryTime = 999 // Unsupported expiry time
            };

            var json = JsonSerializer.Serialize(storeRequest);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/pastes", httpContent);
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.GetProperty("error").GetString().ShouldBe("That expiry time isn't supported.");
        }

        [Fact]
        public async Task GetLoad_WithValidId_ReturnsOkWithEncryptedData()
        {
            // Arrange - First create a paste
            var storeRequest = new
            {
                cipherJson = "{\"iv\":\"test-iv\",\"ct\":\"encrypted-content\"}",
                expiryTime = 60
            };

            var json = JsonSerializer.Serialize(storeRequest);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var storeResponse = await _client.PostAsync("/api/pastes", httpContent);
            var storeContent = await storeResponse.Content.ReadAsStringAsync();
            var storeResult = JsonSerializer.Deserialize<JsonElement>(storeContent);
            var friendlyId = storeResult.GetProperty("friendlyId").GetString();

            _output.WriteLine($"Created paste with ID: {friendlyId}");

            // Act - Load the paste
            var loadResponse = await _client.GetAsync($"/api/pastes/{friendlyId}");
            var loadContent = await loadResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"Load response: {loadContent}");

            // Assert
            loadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var loadResult = JsonSerializer.Deserialize<JsonElement>(loadContent);
            loadResult.GetProperty("friendlyId").GetString().ShouldBe(friendlyId);
            loadResult.GetProperty("cipherJson").GetString().ShouldBe("{\"iv\":\"test-iv\",\"ct\":\"encrypted-content\"}");
            loadResult.GetProperty("expiryDate").GetString().ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetLoad_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/pastes/nonexistent-id");
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.GetProperty("error").GetString().ShouldBe("The url is invalid or the paste has expired.");
        }

        [Fact]
        public async Task GetLoad_WithEmptyId_ReturnsMethodNotAllowed()
        {
            // Act
            var response = await _client.GetAsync("/api/pastes/");

            // Assert
            // This returns MethodNotAllowed because the route matches /api/pastes but with wrong HTTP method
            // GET /api/pastes/ would need to match a POST endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        }

        [Fact]
        public async Task DeletePaste_WithValidId_ReturnsOkWithSuccessMessage()
        {
            // Arrange - First create a paste
            var storeRequest = new
            {
                cipherJson = "{\"iv\":\"test\",\"ct\":\"encrypted-data\"}",
                expiryTime = 60
            };

            var json = JsonSerializer.Serialize(storeRequest);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var storeResponse = await _client.PostAsync("/api/pastes", httpContent);
            var storeContent = await storeResponse.Content.ReadAsStringAsync();
            var storeResult = JsonSerializer.Deserialize<JsonElement>(storeContent);
            var friendlyId = storeResult.GetProperty("friendlyId").GetString();

            _output.WriteLine($"Created paste with ID: {friendlyId}");

            // Act - Delete the paste
            var deleteResponse = await _client.DeleteAsync($"/api/pastes/{friendlyId}");
            var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"Delete response: {deleteContent}");

            // Assert
            deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var deleteResult = JsonSerializer.Deserialize<JsonElement>(deleteContent);
            deleteResult.GetProperty("message").GetString().ShouldBe("Item deleted successfully");

            // Verify the paste is actually deleted
            var loadResponse = await _client.GetAsync($"/api/pastes/{friendlyId}");
            loadResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeletePaste_WithInvalidId_ReturnsOkIdempotently()
        {
            // Act
            var response = await _client.DeleteAsync("/api/pastes/nonexistent-id-12345");
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");

            // Assert
            // Note: The filesystem repository treats DELETE as idempotent -
            // deleting a non-existent item returns success rather than an error
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.GetProperty("message").GetString().ShouldBe("Item deleted successfully");
        }

        [Fact]
        public async Task FullWorkflow_StoreLoadAndDelete_WorksCorrectly()
        {
            // Arrange
            var cipherData = "{\"iv\":\"test-workflow-iv\",\"ct\":\"workflow-encrypted-content\"}";
            var storeRequest = new
            {
                cipherJson = cipherData,
                expiryTime = 360
            };

            // Act & Assert - Store
            var json = JsonSerializer.Serialize(storeRequest);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var storeResponse = await _client.PostAsync("/api/pastes", httpContent);
            storeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var storeContent = await storeResponse.Content.ReadAsStringAsync();
            var storeResult = JsonSerializer.Deserialize<JsonElement>(storeContent);
            var friendlyId = storeResult.GetProperty("friendlyId").GetString();

            _output.WriteLine($"Stored paste with ID: {friendlyId}");

            // Act & Assert - Load
            var loadResponse = await _client.GetAsync($"/api/pastes/{friendlyId}");
            loadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var loadContent = await loadResponse.Content.ReadAsStringAsync();
            var loadResult = JsonSerializer.Deserialize<JsonElement>(loadContent);
            loadResult.GetProperty("cipherJson").GetString().ShouldBe(cipherData);

            _output.WriteLine("Successfully loaded paste");

            // Act & Assert - Delete
            var deleteResponse = await _client.DeleteAsync($"/api/pastes/{friendlyId}");
            deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            _output.WriteLine("Successfully deleted paste");

            // Verify deletion
            var verifyResponse = await _client.GetAsync($"/api/pastes/{friendlyId}");
            verifyResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);

            _output.WriteLine("Verified paste was deleted");
        }
    }
}
