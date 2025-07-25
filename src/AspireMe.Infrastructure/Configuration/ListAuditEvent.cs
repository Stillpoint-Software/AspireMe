using Audit.Core;
using System.Collections;

namespace AspireMe.Infrastructure;
public class ListAuditEvent : AuditEvent
{
    public ListAuditEvent( IList list ) => List = list;

    public IList List { get; set; }

}