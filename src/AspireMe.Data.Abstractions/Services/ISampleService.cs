using AspireMe.Data.Abstractions.Entity;
using AspireMe.Data.Abstractions.Services.Models;


namespace AspireMe.Data.Abstractions.Services;
public interface ISampleService
{
   
   Task<int> CreateSampleAsync( Sample sample );
   Task<SampleDefinition> GetSampleAsync(int sampleId );
   
   Task <SampleDefinition> UpdateSampleAsync(  Sample existing, int sampleId, string name, string description );
   
   
}
