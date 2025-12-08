using attendance_tracking_backend.Data;
using attendance_tracking_backend.DTO;
using attendance_tracking_backend.Helpers;
using HotChocolate.Authorization;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using System;


namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class AttendanceQuery
    {
        [Authorize]
        [UseProjection, UseFiltering,UseSorting]
        public IQueryable<Attendance> GetAttendances([Service] DatabaseContext dbcontext)  //get all attendances
        {
            return dbcontext.Attendances;
        }

        public IQueryable<Attendance> GetAttendanceByUserName(string username, DateOnly day, [Service] DatabaseContext dbcontext)
        {
            var user = dbcontext.Users.Where(u=> u.UserName == username).FirstOrDefault();
      
            return dbcontext.Attendances.Where(a => a.AppUserId == user!.Id && a.CurrentDate == day);
        }

        //get attendance by date range
        [UseProjection, UseFiltering, UseSorting]
        public IQueryable<Attendance> GetAttendanceByDate(DateOnly startDate, DateOnly stopDate, [Service] DatabaseContext dbcontext) 
        {

            // Normalize dates: ensure we compare only dates, not time
            var start = startDate;
            var end = stopDate;

            // Query Attendances within the specified date range
            var results = dbcontext.Attendances
                .Where(a => a.CurrentDate >= start
                         && a.CurrentDate <= end
                      ).Select(a => new Attendance
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
                                }
                             );

            return results;

        }

        //get attendance by user id and date range
        [UseProjection, UseFiltering, UseSorting]
        public IQueryable<Attendance> GetAttendanceByUserId(int userId,DateOnly startDate, DateOnly stopDate ,[Service] DatabaseContext dbcontext)
        {
            return dbcontext.Attendances.Where(a => a.AppUserId == userId && a.CurrentDate >= startDate && a.CurrentDate <=  stopDate).Select(
                a => new Attendance
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
          
        }


        [UseProjection, UseFiltering, UseSorting]    
        public IEnumerable<AverageAttendanceResult> GetAverageAttendanceByDate(DateOnly? startDate,DateOnly? stopDate,[Service] DatabaseContext dbcontext)  //get average attendance of all employees by date
        {
            // Default to all records if date range not supplied
            var query = dbcontext.Attendances.Include(a => a.User).AsQueryable();

            if (startDate.HasValue)
            { query = query.Where(a => a.CurrentDate >= startDate); }

            if (stopDate.HasValue)
            {query = query.Where(a => a.CurrentDate <= stopDate);}

            // Group by user
            var result = query.GroupBy(a => new { a.AppUserId, a.User!.EmployeeName }) // Assuming AppUser has FullName
                .Select(g => new AverageAttendanceResult
                        {
                            UserId = g.Key.AppUserId,
                            EmployeeeName = g.Key.EmployeeName,
                            startDate = startDate ?? g.Min(x => x.CurrentDate) ?? DateOnly.FromDateTime(DateTime.MinValue),
                            stopDate = stopDate ?? g.Max(x => x.CurrentDate) ?? DateOnly.FromDateTime(DateTime.MinValue),
                            // Average ClockIn
                            AverageClockIn = g.Where(x => x.ClockIn.HasValue).Select(x => x.ClockIn!.Value.TimeOfDay.TotalSeconds).DefaultIfEmpty().Average() == 0 ? null: 
                            DateTime.Today.AddSeconds(g.Where(x => x.ClockIn.HasValue).Select(x => x.ClockIn!.Value.TimeOfDay.TotalSeconds).Average()),
                            // Average ClockOut
                            AverageClockOut = g.Where(x => x.ClockOut.HasValue).Select(x => x.ClockOut!.Value.TimeOfDay.TotalSeconds).DefaultIfEmpty().Average() == 0 ? null:
                            DateTime.Today.AddSeconds( g.Where(x => x.ClockOut.HasValue).Select(x => x.ClockOut!.Value.TimeOfDay.TotalSeconds).Average()),
                            // Average total hours
                            AverageTotalHoursWorked = g.Where(x => x.TotalHoursWorked.HasValue).Average(x => x.TotalHoursWorked)
                        }).ToList();

            return result;
        }

        [UseProjection, UseFiltering, UseSorting]
        public AverageAttendanceResult? GetAverageAttendanceByUserId(int userId,DateOnly? startDate, DateOnly? endDate,[Service] DatabaseContext dbcontext)  //get average attendance of an employee
        {
            var query = dbcontext.Attendances.Include(a => a.User).Where(a => a.AppUserId == userId).AsQueryable();

            if (startDate.HasValue)
            { query = query.Where(a => a.CurrentDate >= startDate);}

            if (endDate.HasValue)
            { query = query.Where(a => a.CurrentDate <= endDate);}

            // Fetch user info
            var user = dbcontext.Users.FirstOrDefault(u => u.Id == userId);
            if (!query.Any())
                return null;
            // Average calculations
            var avgClockInSeconds = query.Where(x => x.ClockIn.HasValue).Select(x => x.ClockIn!.Value.TimeOfDay.TotalSeconds).DefaultIfEmpty().Average();

            var avgClockOutSeconds = query.Where(x => x.ClockOut.HasValue).Select(x => x.ClockOut!.Value.TimeOfDay.TotalSeconds).DefaultIfEmpty().Average();

            var avgTotalHours = query.Where(x => x.TotalHoursWorked.HasValue).Average(x => x.TotalHoursWorked);

            // Build result
            var result = new AverageAttendanceResult
                    {
                        UserId = userId,
                        EmployeeeName = user != null ? $"{user.EmployeeName}" : "Unknown",
                        startDate = startDate ?? query.Min(x => x.CurrentDate) ?? DateOnly.FromDateTime(DateTime.MinValue),
                        stopDate = endDate ?? query.Max(x => x.CurrentDate) ?? DateOnly.FromDateTime(DateTime.MinValue),
                        AverageClockIn = avgClockInSeconds == 0 ? null : DateTime.Today.AddSeconds(avgClockInSeconds),
                        AverageClockOut = avgClockOutSeconds == 0 ? null : DateTime.Today.AddSeconds(avgClockOutSeconds),
                        AverageTotalHoursWorked = avgTotalHours
                    };

            return result;
        }

    }
}
