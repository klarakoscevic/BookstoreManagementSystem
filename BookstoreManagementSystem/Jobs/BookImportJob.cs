using BookstoreManagementSystem.Services;
using Quartz;

namespace BookstoreManagementSystem.Jobs;

public class BookImportJob : IJob
{
    private readonly IBookImportService _importService;
    private readonly ILogger<BookImportJob> _logger;

    public BookImportJob(IBookImportService importService, ILogger<BookImportJob> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("BookImportJob started at {Time}", DateTime.UtcNow);

        try
        {
            await _importService.ImportBooksAsync();
            _logger.LogInformation("BookImportJob completed successfully at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BookImportJob failed at {Time}", DateTime.UtcNow);
            throw;
        }
    }
}
