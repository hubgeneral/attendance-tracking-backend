using attendance_tracking_backend.Data;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using System;
using HotChocolate.Authorization;


namespace attendance_tracking_backend.GraphQL
{
    [Authorize]
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ManualLogsQuery
    {
        [UseProjection,UseFiltering,UseSorting]
       public IQueryable<RequestLog> GetManualLogs([Service] DatabaseContext dbcontext) {

           return  dbcontext.RequestLogs;
        }
    }
}

