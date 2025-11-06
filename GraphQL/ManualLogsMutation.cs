using attendance_tracking_backend.Data;
using attendance_tracking_backend.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class ManualLogsMutation
    {
        public  async Task<RequestLog> CreateManualLog(int userid, string employeeName, string reason,DateTime clockIn,DateTime clockOut,string approvalStatus,int adminId, string adminName, [Service] DatabaseContext dbcontext)
        {
            DateTime timeOfDay = DateTime.UtcNow;
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            var adminUser = await dbcontext.Users.FindAsync(adminId);
            if (adminUser == null) throw new GraphQLException("Admin does not exist");

            var employee = await dbcontext.Users.FindAsync(userid);
            if (employee == null) throw new GraphQLException("User does not exist");

            var newRequestLog = new RequestLog
            {
               EmployeeName = employeeName, 
               Reason = reason,
               ClockIn = clockIn,
               ClockOut = clockOut,
               ActionBy = adminUser.EmployeeName,
               TimeOfDay = timeOfDay,
               ApprovalStatus= approvalStatus,
               AppUserId = employee.Id         
            };


            var attendance = await dbcontext.Attendances.FirstOrDefaultAsync(a => a.CurrentDate == today && a.AppUserId == employee.Id);

            if(attendance == null)
            {
                var newAttendance = new Attendance
                {
                    AppUserId = employee.Id,
                    CurrentDate = today,
                    Status = AttendanceStatus.Present.ToString(),
                    ClockIn = clockIn,
                    ClockOut = clockOut,
                    ClockingType = true,
                    TotalHoursWorked = null
                };

                dbcontext.Attendances.Add(newAttendance);
                await dbcontext.SaveChangesAsync();
            }
            else
            {
                attendance.ClockIn = clockIn;
                attendance.ClockOut = clockOut;
                await dbcontext.SaveChangesAsync();
            }
                return newRequestLog;
        }
    }
}
