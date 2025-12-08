using HotChocolate.Data;
using attendance_tracking_backend.Data;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Authorization;

namespace attendance_tracking_backend.GraphQL
{
    [Authorize]
    [ExtendObjectType(OperationTypeNames.Query)]
    public class RequestLogsQuery
    {
        [UseProjection, UseFiltering, UseSorting]
        public IQueryable<RequestLog> GetRequestLogs([Service] DatabaseContext dbcontext) {

            return dbcontext.RequestLogs;
        }

        public IQueryable<RequestLog> GetRequestLogsByUserId(int id, [Service] DatabaseContext dbcontext)
        {
            var user = dbcontext.Users.Where(u=> u.Id == id).FirstOrDefault();
            return dbcontext.RequestLogs.Where(r => r.AppUserId == user!.Id);
        }

    }
}
