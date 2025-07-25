using Microsoft.Extensions.Logging;
using AspireMe.Core.Services;
using AspireMe.Data.Abstractions.Entity;
using AspireMe.Data.Abstractions.Services;
using AspireMe.Data.Abstractions.Services.Models;
using Microsoft.Extensions.Logging;

using Microsoft.EntityFrameworkCore;


namespace AspireMe.Data.PostgreSql.Services;



public class SampleService : ISampleService
{
    private readonly DatabaseContext _databaseContext;
    private readonly ILogger _logger;

    public SampleService(DatabaseContext databaseContext, ILogger<Sample> logger)
    {
        _databaseContext = databaseContext;
        _logger = logger;
    }

    public async Task<SampleDefinition> GetSampleAsync(int sampleId)
    {
        try
        {
            return await _databaseContext.Samples
                  .Where(x => x.Id == sampleId)
                  .Select(x => new SampleDefinition(
                      x.Id,
                      x.Name ?? string.Empty,
                      x.Description ?? string.Empty
                  ))
                  .FirstOrDefaultAsync() ?? throw new ServiceException(nameof(GetSampleAsync), "Sample not found.");
        }
        catch (Exception ex)
        {
            throw new ServiceException(nameof(GetSampleAsync), "Error getting sample.", ex);
        }
    }

    

    public async Task<int> CreateSampleAsync( Sample sample )
    {
        try
        {
            _databaseContext.Samples.Add( sample );
            await _databaseContext.SaveChangesAsync();
            return sample.Id;
        }
        catch (Exception ex)
        {
            throw new ServiceException( nameof( CreateSampleAsync ), "Error saving sample.", ex );
        }
    }

   public async Task<SampleDefinition> UpdateSampleAsync( Sample existing, int sampleId, string name, string description )
    {
        try
        {
            if (existing == null)
                throw new ServiceException( nameof( UpdateSampleAsync ), "Sample cannot be null." );

            _databaseContext.Entry( existing ).CurrentValues.SetValues( new
            {
                Name = name,
                Description = description
            } );

            await _databaseContext.SaveChangesAsync();

            return new SampleDefinition(
                existing.Id,
                existing.Name ?? string.Empty,
                existing.Description ?? string.Empty
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException( nameof( UpdateSampleAsync ), "Error updating Sample.", ex );
        }
    }

}


