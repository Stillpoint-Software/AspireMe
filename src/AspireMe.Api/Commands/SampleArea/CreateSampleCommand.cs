
using Audit.Core;

using Hyperbee.Pipeline;
using Hyperbee.Pipeline.Commands;
using Hyperbee.Pipeline.Context;
using AspireMe.Core.Commands;
using AspireMe.Core.Commands.Middleware;
using AspireMe.Core.Identity;
using AspireMe.Data.Abstractions.Services;
using AspireMe.Data.Abstractions.Entity;
using AspireMe.Data.Abstractions.Services.Models;
using AspireMe.Core.Extensions;
using Microsoft.Extensions.Logging;


namespace AspireMe.Api.Commands.SampleArea;

public record CreateSample(string Name, string Description);

public interface ICreateSampleCommand : ICommandFunction<CreateSample, SampleDefinition>;

public class CreateSampleCommand : ServiceCommandFunction<CreateSample, SampleDefinition>, ICreateSampleCommand
{
    private readonly ISampleService _sampleService;
    private readonly string _user;

    public CreateSampleCommand(
        ISampleService sampleService,
        IPrincipalProvider principalProvider,
        IPipelineContextFactory pipelineContextFactory,
        ILogger<CreateSampleCommand> logger) :
        base(pipelineContextFactory, logger)
    {
        _sampleService = sampleService;
        _user = principalProvider.GetEmail();
    }

    protected override FunctionAsync<CreateSample, SampleDefinition> CreatePipeline()
    {
        return PipelineFactory
            .Start<CreateSample>()
            .WithLogging()
            .PipeAsync(CreateSampleAsync)
            .CancelOnFailure(Validate<Sample>)
            .PipeAsync(InsertSampleAsync)
            .Build();
    }

    private async Task<Sample> CreateSampleAsync(IPipelineContext context, CreateSample sample)
    {

        return await Task.FromResult(new Sample
        {
            Name = sample.Name,
            Description = sample.Description,
            CreatedBy = _user ?? string.Empty,
        });
    }

    
        private async Task<SampleDefinition> InsertSampleAsync(IPipelineContext context, Sample sample)
    {
        using (AuditScope.Create("Sample:Create", () => sample))
        {
            sample.Id = await _sampleService.CreateSampleAsync(sample);

            var sampleDefinition = new SampleDefinition
            (
                
                sample.Id,
                 
                sample.Name,
                sample.Description
            );
            return sampleDefinition;
        }
    } 
    
}
