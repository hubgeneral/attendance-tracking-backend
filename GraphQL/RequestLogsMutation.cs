using HotChocolate.Data;
using attendance_tracking_backend.Data;
using Microsoft.AspNetCore.Identity;
using HotChocolate.Authorization;

namespace attendance_tracking_backend.GraphQL
{
    [Authorize]
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class RequestLogsMutation
    {
        public async Task<string> CreateRequestLog(int userid, string reason ,[Service]DatabaseContext dbcontext)
        {
            var user = await dbcontext.Users.FindAsync(userid);
            if (user == null) throw new GraphQLException("User does not exits");

            DateTime currentTime = DateTime.UtcNow;

            var newRequest = new RequestLog
            {
                EmployeeName = user.EmployeeName,
                Reason = reason,
                TimeOfDay = currentTime,
                AppUserId = user.Id,              
            };
            dbcontext.RequestLogs.Add(newRequest);
            await dbcontext.SaveChangesAsync();

            return "New request created";
        }

        public async Task<string> UpdateRequestLog(int requestId,string reason, [Service] DatabaseContext dbcontext)
        {         
            var request = await dbcontext.RequestLogs.FindAsync(requestId);
            if (request == null) throw new GraphQLException("Request record does not exist");

            request.Reason = reason;
            await dbcontext.SaveChangesAsync();              
            return "Request record updated";
        }
        
        public async Task<string> DeleteRequest(int requestId, [Service] DatabaseContext dbcontext)
        {
            var request = await dbcontext.RequestLogs.FindAsync(requestId);
            if (request == null) throw new GraphQLException("Request record does not exist");
            dbcontext.Remove(request);
            await dbcontext.SaveChangesAsync();
            return "Request record deleted successfully";
        }
    }
}
