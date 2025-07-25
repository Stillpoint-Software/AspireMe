using System.Reflection;
using Microsoft.Extensions.Hosting;

using Audit.Core;
 

using Audit.PostgreSql.Configuration;

using AspireMe.Data.PostgreSql;
using Microsoft.EntityFrameworkCore;
using AspireMe.Core.Security;
using AspireMe.Data.PostgreSql;

namespace AspireMe.Infrastructure.Configuration;

public static class AuditSetup
{
    private static DatabaseContext _dbContext;

    public static void ConfigureAudit(IHostApplicationBuilder builder)
    {

        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        
        var connectionString = builder.Configuration["ConnectionStrings:medical"];
          optionsBuilder.UseNpgsql( connectionString );
  _dbContext = new DatabaseContext( optionsBuilder.Options );

Audit.Core.Configuration
        .Setup()
        .UsePostgreSql( config => config
            .ConnectionString( connectionString )
            .Schema("medical")
            .TableName( "audit_event" )
            .IdColumnName( "event_id" )
            .LastUpdatedColumnName( "last_updated" )
            .DataColumn( "data", DataType.JSONB, ev => ev.ToJson() )
            .CustomColumn( "event_type", ev => ev.EventType ) );
        
        

        Audit.Core.Configuration.AddOnSavingAction(scope =>
        {
            if (scope.Event is ListAuditEvent auditEvent)
            {
                var auditList = auditEvent.List
                    .Cast<object>()
                    .Select(item => new ListAuditModel
                    {
            
            Id = (int)item.GetType().GetProperty("Id")!.GetValue(item)!
            
        })
        .ToList();

                auditEvent.List = auditList;
            }

            if (scope.Event.Target?.Type == null || scope.Event.Target?.New == null)
            {
                return;
            }

            SetSecuredProperties(scope.Event, _dbContext);
        } );
    }

    private static void SetSecuredProperties(AuditEvent auditEvent, DatabaseContext _dbContext)
    {
        var secureProperties = auditEvent.Target.New?.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<SecureAttribute>() != null);
        if (secureProperties != null)
        {
            foreach (var property in secureProperties)
            {
                
                if (auditEvent.Target.Old != null)
    {
        var propValue = property.GetValue( auditEvent.Target.Old );
        if (propValue != null)
        {
            var oldData = Convert.ToBase64String( _dbContext.EncryptData( propValue.ToString() ?? string.Empty ) );
            property.SetValue( auditEvent.Target.Old, oldData );
        }
    }
    var newValue = property.GetValue( auditEvent.Target.New );
    if (newValue != null)
    {
        var newData = Convert.ToBase64String( _dbContext.EncryptData( newValue.ToString() ?? string.Empty ) );
        property.SetValue( auditEvent.Target.New, newData );
    }
                
                
            }
        }
    }
}