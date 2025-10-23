using HotChocolate.Data;
using attendance_tracking_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class RequestQuery
    {
        [UseProjection, UseFiltering, UseSorting]
        public IQueryable<RequestLog> GetRequests([Service] DatabaseContext dbcontext) {

            return dbcontext.RequestLogs;
        }

        public IQueryable<RequestLog> GetRequestByUserId(int id, [Service] DatabaseContext dbcontext)
        {
            var user = dbcontext.Users.Where(u=> u.Id == id).FirstOrDefault();
            return dbcontext.RequestLogs.Where(r => r.AppUserId == user!.Id);
        }

    }
}
