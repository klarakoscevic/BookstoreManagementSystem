using BookstoreManagementSystem.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace BookstoreManagementSystem.Tests.IntegrationTests;

public class AuthorsControllerIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAllAuthors_ShouldReturnAllAuthors()
    {
        // Act
        var response = await Client.GetAsync("/api/authors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authors = await response.Content.ReadFromJsonAsync<List<AuthorDto>>();
        authors.Should().NotBeNull();
        authors.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAuthor_WithValidId_ShouldReturnAuthor()
    {
        // Act
        var response = await Client.GetAsync("/api/authors/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var author = await response.Content.ReadFromJsonAsync<AuthorDto>();
        author.Should().NotBeNull();
        author!.Id.Should().Be(1);
        author.Name.Should().Be("J.K. Rowling");
    }

    [Fact]
    public async Task GetAuthor_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/authors/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAuthor_WithValidData_AsAdmin_ShouldCreateAuthor()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var createAuthorDto = new CreateAuthorDto
        {
            YearOfBirth = 1965,
            Name = "New Test Author"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/authors", createAuthorDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdAuthor = await response.Content.ReadFromJsonAsync<AuthorDto>();
        createdAuthor.Should().NotBeNull();
        createdAuthor!.Name.Should().Be("New Test Author");
        createdAuthor!.YearOfBirth.Should().Be(1965);

        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAuthor_AsReadUser_ShouldReturnForbidden()
    {
        // Arrange
        var createAuthorDto = new CreateAuthorDto
        {
            Name = "New Test Author"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/authors", createAuthorDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAuthor_WithValidData_AsAdmin_ShouldUpdateAuthor()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var updateAuthorDto = new UpdateAuthorDto
        {
            Name = "Updated Author Name",
            YearOfBirth = 1970
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/authors/1", updateAuthorDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedAuthor = await response.Content.ReadFromJsonAsync<AuthorDto>();
        updatedAuthor.Should().NotBeNull();
        updatedAuthor!.Id.Should().Be(1);
        updatedAuthor.Name.Should().Be("Updated Author Name");
        updatedAuthor.YearOfBirth.Should().Be(1970);
    }

    [Fact]
    public async Task UpdateAuthor_WithInvalidId_AsAdmin_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var updateAuthorDto = new UpdateAuthorDto
        {
            Name = "Updated Author",
            YearOfBirth = 1970
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/authors/999", updateAuthorDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAuthor_AsReadUser_ShouldReturnForbidden()
    {
        // Arrange
        var updateAuthorDto = new UpdateAuthorDto
        {
            Name = "Updated Author Name",
            YearOfBirth = 1970
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/authors/1", updateAuthorDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAuthor_WithValidId_AsAdmin_ShouldSoftDeleteAuthor()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        // Act
        var response = await Client.DeleteAsync("/api/authors/3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify author is soft deleted
        var getResponse = await Client.GetAsync("/api/authors");
        var authors = await getResponse.Content.ReadFromJsonAsync<List<AuthorDto>>();
        authors.Should().NotContain(a => a.Id == 3);
    }   
}
