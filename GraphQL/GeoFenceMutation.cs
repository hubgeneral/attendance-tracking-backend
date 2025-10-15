using attendance_tracking_backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HotChocolate; 
using HotChocolate.Types; 

namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class GeoFenceMutation
    {
        public async Task<string> GeoFenceClockIn(string username, DateTime clockin, [Service] DatabaseContext dbcontext,[Service] UserManager<AppUser> userManager)
        { 
            var user = await userManager.FindByNameAsync(username);
            if (user == null) throw new GraphQLException("User does not exist");

            
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);  //current date(present day)
            var recordExists = await dbcontext.Attendances.FirstOrDefaultAsync(a => a.AppUserId == user.Id && a.CurrentDate == today);

            if (recordExists != null) return "";

            var newAttendance = new Attendance
            {
                AppUserId = user.Id,
                ClockIn = clockin,
                ClockingType = true,
                CurrentDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };
           
            await dbcontext.Attendances.AddAsync(newAttendance);
            await dbcontext.SaveChangesAsync();

            return   $" {username} clocked in at {newAttendance.ClockIn:HH:mm:ss} UTC.";
        }


        public async Task<string> GeofenceClockOut(string username, [Service] DatabaseContext dbcontext, [Service] UserManager<AppUser> userManager)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null) throw new GraphQLException("User does not exist");

            return $" {username} has clocked out ";
        }
    }
}
