using System;
using HotChocolate.Data;
using attendance_tracking_backend.Data;
using Microsoft.EntityFrameworkCore;
using attendance_tracking_backend.Helpers;


namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class AttendanceQuery
    {
        [UseProjection, UseFiltering,UseSorting]
        public IQueryable<Attendance> GetAttendances([Service] DatabaseContext dbcontext)
        {
            return dbcontext.Attendances;
        }

        public IQueryable<Attendance> GetAttendanceByUserId(string username, [Service] DatabaseContext dbcontext)
        {
            var user = dbcontext.Users.Where(u=> u.UserName == username).FirstOrDefault();
      
            return dbcontext.Attendances.Where(a => a.AppUserId == user!.Id);
        }


        public IQueryable<Attendance> GetAttendanceByDate(DateTime startDate, DateTime stopDate, [Service] DatabaseContext dbcontext) {

            // Normalize dates: ensure we compare only dates, not time
            var start = startDate.Date;
            var end = stopDate.Date;

            // Query Attendances within the specified date range
            var results = dbcontext.Attendances
                .Where(a => a.CurrentDate >= DateOnly.FromDateTime(start)
                         && a.CurrentDate <= DateOnly.FromDateTime(end))
                .Select(a => new Attendance
                {
                    Id = a.Id,
                    ClockIn = a.ClockIn,
                    ClockOut = a.ClockOut,
                    ClockingType = a.ClockingType,
                    Status = a.Status,
                    CurrentDate = a.CurrentDate,
                    AppUserId = a.AppUserId,
                    User = a.User,
                    EntryExitLogs = a.EntryExitLogs,
                    TotalHoursWorked = CalculateWorkHours.CalculateWorkingHours(a.ClockIn, a.ClockOut)
                });

            return results;

        }
        
    }
}
