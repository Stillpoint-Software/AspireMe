using Microsoft.Extensions.Logging;
using NSubstitute;
using AspireMe.Api.Commands.SampleArea;
using AspireMe.Api.Validators;
using AspireMe.Core.Identity;
using AspireMe.Core.Validators;
using AspireMe.Data.Abstractions.Entity;
using AspireMe.Data.Abstractions.Services;
using AspireMe.Tests.TestSupport;


namespace AspireMe.Tests.Commands.SampleArea;

[TestClass]
public class CreateSampleCommandTests
{

   
    // test support
    private static ICreateSampleCommand CreateCommand()
    {
        var logger = Substitute.For<ILogger<CreateSampleCommand>>();

        var validatorProvider = Substitute.For<IValidatorProvider>();
        validatorProvider.For<Sample>().Returns( new SampleValidation() );

        var prinicipalProvider = Substitute.For<IPrincipalProvider>();

        var pipelineContextFactory = PipelineContextFactoryFixture.Next( validatorProvider: validatorProvider );
        var sampleService = Substitute.For<ISampleService>();

        return new CreateSampleCommand( sampleService, prinicipalProvider, pipelineContextFactory, logger );
    }

    [TestMethod]
    public async Task Should_execute_command()
    {
        // arrange
        var command = CreateCommand();

        // act
        var result = await command.ExecuteAsync( new CreateSample( "Test Sample", "This is a test") );

        // assert
        Assert.IsTrue( result.Context.Success, result.ContextMessage() );
    }

    [TestMethod]
    public async Task Should_Create_Sample()
    {
        // arrange
        var command = CreateCommand();

        // act
        var result = await command.ExecuteAsync( new CreateSample( "Test Sample", "This is a test") );

        // assert
        Assert.IsTrue( result.Context.Success, result.ContextMessage() );
    }

    [TestMethod]
    public async Task Should_Fail_Create_Sample()
    {
        // arrange
        var command = CreateCommand();

        // act
        var result = await command.ExecuteAsync( new CreateSample( null, "This is a test" ) );

        // assert
        Assert.IsFalse( result.Context.Success, result.ContextMessage() );
    }
}
