using attendance_tracking_backend.Data;
using HotChocolate.Authorization;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using attendance_tracking_backend.DTO;
using System;
using HotChocolate.Authorization;

namespace attendance_tracking_backend.GraphQL
{
    [Authorize]
    [ExtendObjectType(OperationTypeNames.Query)]
 
    public class DashboardQuery
    {
        public DashboardTotalSummary GetDashboardTotalStats( DateOnly? startDate, DateOnly? endDate, [Service] DatabaseContext dbcontext)
        {
            var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);

            var start = startDate ?? utcToday;

            var end = endDate ?? utcToday;
            var startDateTime = DateTime.SpecifyKind(start.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var endDateTime = DateTime.SpecifyKind(end.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

            var totalEmployees = dbcontext.Users.Count();

            var employeesClockedIn = dbcontext.Attendances
                .Where(a => a.ClockIn != null && a.CurrentDate >= start && a.CurrentDate <= end)
                .Select(a => a.AppUserId)
                .Distinct()
                .Count();

            var employeesClockedOut = dbcontext.Attendances
                .Where(a => a.ClockOut != null && a.CurrentDate >= start && a.CurrentDate <= end)
                .Select(a => a.AppUserId)
                .Distinct()
                .Count();

            var totalLeaves = dbcontext.Leaves.
                Where(l => l.StartDate <= startDateTime && l.EndDate >= endDateTime && l.ApprovalStatus == "Approved")
                .Select(l=>l.AppUserId)
                .Distinct()
                .Count();

            var totalAbsent = totalEmployees - employeesClockedIn-totalLeaves;

            return new DashboardTotalSummary
            {
                TotalEmployees = totalEmployees,
                EmployeesClockedIn = employeesClockedIn,
                EmployeesClockedOut = employeesClockedOut,
                TotalLeaves = totalLeaves,
                TotalAbsent = totalAbsent
            };
        }

      

        public AverageClockTimeResult AverageClockTime(  DateOnly? startDate, DateOnly? endDate,[Service] DatabaseContext dbcontext)
        {
            var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
            var start = startDate ?? utcToday;
            var end = endDate ?? utcToday;

            var clockInTimes = dbcontext.Attendances
                .Where(a => a.ClockIn != null && a.CurrentDate >= start && a.CurrentDate <= end)
                .Select(a => a.ClockIn!.Value.TimeOfDay.TotalMilliseconds)
                .ToList();

            var clockOutTimes = dbcontext.Attendances
                .Where(a => a.ClockOut != null && a.CurrentDate >= start && a.CurrentDate <= end)
                .Select(a => a.ClockOut!.Value.TimeOfDay.TotalMilliseconds)
                .ToList();

            DateTime? averageClockIn = null;
            DateTime? averageClockOut = null;

            if (clockInTimes.Any())
                averageClockIn = DateTime.Today.Add(TimeSpan.FromMilliseconds(clockInTimes.Average()));

            if (clockOutTimes.Any())
                averageClockOut = DateTime.Today.Add(TimeSpan.FromMilliseconds(clockOutTimes.Average()));
                
            return new AverageClockTimeResult
            {
                AverageClockIn = averageClockIn,
                AverageClockOut = averageClockOut
            };
        }

     
        public IEnumerable<GraphDataResults> GraphData( DateOnly? startDate, DateOnly? endDate,[Service] DatabaseContext dbcontext)
        {
            var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);

            // Default to current work week (Monday–Friday)
            var startOfWeek = utcToday.AddDays(-(int)utcToday.DayOfWeek + (utcToday.DayOfWeek == DayOfWeek.Sunday ? 1 : 0));
            if (startOfWeek.DayOfWeek != DayOfWeek.Monday)
                startOfWeek = utcToday.AddDays(-(int)utcToday.DayOfWeek + 1);

            var start = startDate ?? startOfWeek;
            var end = endDate ?? start.AddDays(4); // Friday

            // Convert DateOnly to UTC DateTime
            static DateTime AsUtcDate(DateOnly d, TimeOnly t) =>
                DateTime.SpecifyKind(d.ToDateTime(t), DateTimeKind.Utc);

            var results = Enumerable.Range(1, 5)
                .Select(i => new GraphDataResults
                {
                    Day = (DayOfWeek)i,
                    ClockedInCount = 0,
                    Absent = 0,
                    OnLeave = 0
                })
                .ToArray();

            var totalEmployees = dbcontext.Users.Count();

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                var dayOfWeek = date.DayOfWeek;
                var result = results.First(r => r.Day == dayOfWeek);

                // Convert current date boundaries to UTC
                var dayStartUtc = AsUtcDate(date, TimeOnly.MinValue);
                var dayEndUtc = AsUtcDate(date, TimeOnly.MaxValue);

                // Count employees who clocked in (assuming CurrentDate is DateOnly)
                var clockedIn = dbcontext.Attendances
                    .Where(a => a.CurrentDate == date && a.ClockIn != null)
                    .Select(a => a.AppUserId)
                    .Distinct()
                    .Count();

                // dayStartUtc and dayEndUtc are DateTime with Kind = Utc
                var onLeave = dbcontext.Leaves
                    .Where(l =>
                        l.ApprovalStatus == "Approved" &&
                        l.StartDate <= dayEndUtc &&
                        (l.EndDate ?? DateTime.MaxValue) >= dayStartUtc)   // treat null EndDate as open-ended
                    .Select(l => l.AppUserId)
                    .Distinct()
                    .Count();


                var absent = totalEmployees - (clockedIn + onLeave);

                result.ClockedInCount = clockedIn;
                result.OnLeave = onLeave;
                result.Absent = absent;
            }

            return results.ToList();
        }

        public IQueryable<RequestLog> GetRequestLogs(DateOnly startday,DateOnly stopdate , DatabaseContext dbcontext)
        {
           // today = DateOnly.FromDateTime(DateTime.UtcNow);
           var results = dbcontext.RequestLogs.Where(r => r.CurrentDate >= startday && r.CurrentDate <= stopdate);

            return results;
        }

        public WorkingHours WorkHoursSummary(DateOnly startday,DateOnly stopdate, [Service] DatabaseContext dbcontext)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            var TotalNumberOfAttendances = dbcontext.Attendances.Where(r => r.CurrentDate >= startday && r.CurrentDate <= stopdate).Count();
            var TotalWorkingHours = dbcontext.Attendances.Where(r => r.CurrentDate == today).Sum(a => a.TotalHoursWorked);
            decimal totalWorkingHours = 8;
            decimal expectTotalWorkingHours = (decimal)totalWorkingHours * TotalNumberOfAttendances;

            var newWorkingHours = new WorkingHours
            {
                TotalWorkingHours = totalWorkingHours,
                TotalOffHours = expectTotalWorkingHours - totalWorkingHours
            };

            return newWorkingHours;
        }


        //most hours worked
        public IQueryable<MostHoursWorkedByEmployee> MostHoursWorked(DateOnly startday,DateOnly stopdate, [Service] DatabaseContext dbcontext)
        {
            

            var top5Employees = dbcontext.Attendances
           .Include(a => a.User) // Include related AppUser for EmployeeName
           .Where(a => a.CurrentDate >= startday && a.CurrentDate <= stopdate && a.TotalHoursWorked != null).GroupBy(a => new { a.AppUserId, a.User!.EmployeeName })
           .Select(g => new MostHoursWorkedByEmployee
           {
               EmployeeName = g.Key.EmployeeName,
               TotalWorkingHours = g.Sum(x => x.TotalHoursWorked ?? 0)
           })
           .OrderByDescending(x => x.TotalWorkingHours).Take(5);

            return top5Employees;

        }

        //most off hours
        public IQueryable<MostHoursWastedByEmployee>  MostWastedHours(DateOnly startday, DateOnly stopdate, [Service] DatabaseContext dbcontext)
        {
            const decimal standardWorkingHours = 8m; // full workday

            var top5Employees = dbcontext.Attendances
                .Include(a => a.User) // Load employee info
                .Where(a => a.CurrentDate >= startday && a.CurrentDate <= stopdate && a.TotalHoursWorked != null)
                .GroupBy(a => new { a.AppUserId, a.User!.EmployeeName })
                .Select(g => new MostHoursWastedByEmployee
                {
                    EmployeeName = g.Key.EmployeeName,
                    TotalWastedHours = standardWorkingHours - g.Sum(x => x.TotalHoursWorked ?? 0)
                })
                .Where(x => x.TotalWastedHours > 0) // only employees who worked less than 8 hours
                .OrderByDescending(x => x.TotalWastedHours)
                .Take(5);

            return top5Employees;
        }


        // most late employees
        public IEnumerable<LateEmployees> GetLateEmployees(DateOnly startday ,DateOnly stopdate, [Service] DatabaseContext dbcontext)
        {
            // Define the start of work (8:00 AM)
            var workStartTime = new TimeSpan(8, 0, 0);

            // Query to find the top 5 latest clock-ins for the given day
            var lateEmployees = dbcontext.Attendances
                .Include(a => a.User)
                .Where(a => a.CurrentDate >= startday && a.CurrentDate <= stopdate && a.ClockIn != null)
                .Select(a => new LateEmployees
                {
                    EmployeeName = a.User!.EmployeeName,
                    TimeOfDay = a.ClockIn
                })
                // Only consider employees who clocked in after 8:00 AM
                .Where(a => a.TimeOfDay!.Value.TimeOfDay > workStartTime)
                // Sort by lateness (latest arrivals first)
                .OrderByDescending(a => a.TimeOfDay)
                // Get top 5
                .Take(5)
                .ToList();

            return lateEmployees;
        }

        //most punctual employees
        public IEnumerable<PunctualEmployees> GetPunctualEmployees(DateOnly startday, DateOnly stopdate ,[Service] DatabaseContext dbcontext)
        {
            // Define the early arrival range
            var earliestTime = new TimeSpan(0, 0, 0);   // 12:00 AM
            var workStartTime = new TimeSpan(8, 0, 0);  // 8:00 AM

            // Query: Top 5 earliest arrivals between 12 AM and 8 AM
            var punctualEmployees = dbcontext.Attendances
                .Include(a => a.User)
                .Where(a => a.CurrentDate == startday && a.CurrentDate <= stopdate && a.ClockIn != null)
                .Select(a => new PunctualEmployees
                {
                    EmployeeName = a.User!.EmployeeName,
                    TimeOfDay = a.ClockIn
                })
                // Filter only employees who clocked in between 12 AM and 8 AM
                .Where(a => a.TimeOfDay!.Value.TimeOfDay >= earliestTime &&
                            a.TimeOfDay!.Value.TimeOfDay <= workStartTime)
                // Order by earliest arrivals
                .OrderBy(a => a.TimeOfDay)
                // Take top 5 earliest
                .Take(5)
                .ToList();

            return punctualEmployees;
        }

    }
}
