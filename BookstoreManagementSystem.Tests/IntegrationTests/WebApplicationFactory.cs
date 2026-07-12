using BookstoreManagementSystem.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BookstoreManagementSystem.Tests.IntegrationTests;

public class BookstoreWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"BookstoreTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing - this must be done early
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific overrides as the last configuration source
            // This ensures they have the highest priority
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSecretKeyForIntegrationTests12345678901234567890",
                ["JwtSettings:Issuer"] = "BookstoreTestIssuer",
                ["JwtSettings:Audience"] = "BookstoreTestAudience",
                ["JwtSettings:ExpirationHours"] = "1"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove all DbContext registrations to avoid conflicts
            var descriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<BookstoreDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(BookstoreDbContext))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database for testing with unique name per instance
            services.AddDbContext<BookstoreDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Reconfigure JWT Bearer authentication with test settings
            // PostConfigure runs after all Configure calls to ensure test settings override everything
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var secretKey = "TestSecretKeyForIntegrationTests12345678901234567890";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "BookstoreTestIssuer",
                    ValidAudience = "BookstoreTestAudience",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        });
    }
}
