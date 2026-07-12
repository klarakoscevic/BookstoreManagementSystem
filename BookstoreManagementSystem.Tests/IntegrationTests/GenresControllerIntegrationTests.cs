using BookstoreManagementSystem.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace BookstoreManagementSystem.Tests.IntegrationTests;

public class GenresControllerIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAllGenres_ShouldReturnAllGenres()
    {
        // Act
        var response = await Client.GetAsync("/api/genres");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var genres = await response.Content.ReadFromJsonAsync<List<GenreDto>>();
        genres.Should().NotBeNull();
        genres.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetGenre_WithValidId_ShouldReturnGenre()
    {
        // Act
        var response = await Client.GetAsync("/api/genres/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var genre = await response.Content.ReadFromJsonAsync<GenreDto>();
        genre.Should().NotBeNull();
        genre!.Id.Should().Be(1);
        genre.Name.Should().Be("Fiction");
    }

    [Fact]
    public async Task GetGenre_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/genres/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateGenre_WithValidData_AsAdmin_ShouldCreateGenre()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var createGenreDto = new CreateGenreDto
        {
            Name = "Mystery"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/genres", createGenreDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdGenre = await response.Content.ReadFromJsonAsync<GenreDto>();
        createdGenre.Should().NotBeNull();
        createdGenre!.Name.Should().Be("Mystery");

        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateGenre_AsReadUser_ShouldReturnForbidden()
    {
        // Arrange
        var createGenreDto = new CreateGenreDto
        {
            Name = "Mystery"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/genres", createGenreDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateGenre_WithValidData_AsAdmin_ShouldUpdateGenre()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var updateGenreDto = new UpdateGenreDto
        {
            Name = "Updated Fiction"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/genres/1", updateGenreDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedGenre = await response.Content.ReadFromJsonAsync<GenreDto>();
        updatedGenre.Should().NotBeNull();
        updatedGenre!.Id.Should().Be(1);
        updatedGenre.Name.Should().Be("Updated Fiction");
    }

    [Fact]
    public async Task UpdateGenre_WithInvalidId_AsAdmin_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var updateGenreDto = new UpdateGenreDto
        {
            Name = "Updated Genre"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/genres/999", updateGenreDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateGenre_AsReadUser_ShouldReturnForbidden()
    {
        // Arrange
        var updateGenreDto = new UpdateGenreDto
        {
            Name = "Updated Fiction"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/genres/1", updateGenreDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteGenre_WithValidId_AsAdmin_ShouldSoftDeleteGenre()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        // Act
        var response = await Client.DeleteAsync("/api/genres/3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify genre is soft deleted
        var getResponse = await Client.GetAsync("/api/genres");
        var genres = await getResponse.Content.ReadFromJsonAsync<List<GenreDto>>();
        genres.Should().NotContain(g => g.Id == 3);
    }    
}
