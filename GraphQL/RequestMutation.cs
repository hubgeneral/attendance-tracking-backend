using HotChocolate.Data;
using attendance_tracking_backend.Data;
using Microsoft.AspNetCore.Identity;

namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class RequestMutation
    {
        public async Task<string> CreateRequest(string username, string Description ,[Service] UserManager<AppUser> userManager,[Service]DatabaseContext dbcontext)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null) throw new GraphQLException("User does not exits");

            DateTime currentTime = DateTime.UtcNow;

            var newRequest = new Request
            {
                Description = Description,
                TimeOfDay = currentTime,
                AppUserId = user!.Id
            };
            dbcontext.Requests.Add(newRequest);
            await dbcontext.SaveChangesAsync();

            return "New request created";
        }

        public async Task<string> UpdateRequest(int requestId,string Description, [Service] DatabaseContext dbcontext)
        {         
            var request = await dbcontext.Requests.FindAsync(requestId);
            if (request == null) throw new GraphQLException("Request record does not exist");

            request.Description = Description;
            await dbcontext.SaveChangesAsync();  
            
            return "Request record updated";
        }
        
        public async Task<string> DeleteRequest(int requestId, [Service] DatabaseContext dbcontext)
        {
            var request = await dbcontext.Requests.FindAsync(requestId);
            if (request == null) throw new GraphQLException("Request record does not exist");
            dbcontext.Remove(request);
            await dbcontext.SaveChangesAsync();

            return "Request record deleted successfully";
        }
    }
}
