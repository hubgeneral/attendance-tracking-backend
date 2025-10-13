using attendance_tracking_backend.Data;

namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType("Query")]
    public class RequestQuery
    {
        [UseProjection, UseFiltering, UseSorting]

        public IQueryable<Request> GetRequests([Service] DatabaseContext dbcontext)
        {
            return dbcontext.Requests;
        }

        public Request? GetRequestsByUserId(int id, [Service] DatabaseContext dbcontext)
        {
           // var user = dbcontext.Users.Where(u=>u.UserName == username).FirstOrDefault();
           //return dbcontext.Requests.Where(r => r.AppUserId == id);

            return dbcontext.Requests.Find(id);
        }
    }
}
