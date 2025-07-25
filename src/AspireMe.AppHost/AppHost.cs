using AspireMe.AppHost;



var builder = DistributedApplication.CreateBuilder( args );

var dbPassword = builder.AddParameter( "DbPassword", "postgres", true );
var dbUser = builder.AddParameter( "DbUser", "postgres", true );

var dbServer = builder.AddPostgres( "postgres", userName: dbUser, password: dbPassword )
    .PublishAsConnectionString()
    .WithDataVolume()
    .WithPgAdmin( x => x.WithImageTag( "9.5" ) );



var projectdb = dbServer.AddDatabase( "medical" );

var apiService = builder.AddProject<Projects.AspireMe_Api>( "aspireme-api" )
    .WithReference( projectdb )
    .WithExternalHttpEndpoints()



    .WithSwaggerUI()
    .WithHttpHealthCheck( "/health" );

builder.AddProject<Projects.AspireMe_Migrations>( "aspireme-migrations" )
    .WaitFor( projectdb )

    .WithReference( projectdb );

builder.Build().Run();
