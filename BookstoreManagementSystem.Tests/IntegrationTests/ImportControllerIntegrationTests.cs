using BookstoreManagementSystem.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace BookstoreManagementSystem.Tests.IntegrationTests;

public class ImportControllerIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task TriggerImport_ShouldImportBooks()
    {
        // Note: This test may take longer as it imports data
        // In a real scenario, you might want to reduce the number of books imported for testing

        // Act
        var response = await Client.PostAsync("/api/import/trigger", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ImportResultDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Contain("completed");
    }   
}
