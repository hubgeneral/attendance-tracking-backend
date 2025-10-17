using attendance_tracking_backend.Data;
using attendance_tracking_backend.Helpers;
using FluentScheduler;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace attendance_tracking_backend.Services
{
    public class LeaveJob : IJob
    {
        public void Execute()
        {

            var today = DateTime.UtcNow.DayOfWeek;

            // Skip weekends
            if (today == DayOfWeek.Saturday || today == DayOfWeek.Sunday)
            {
                Console.WriteLine($"[{DateTime.UtcNow}] Skipping job - weekend");
                return;
            }

            using var scope = Program.ServiceProvider!.CreateScope();
            var dbcontext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var onLeaveUsers = dbcontext.Leaves
                .Where(l => l.StartDate.HasValue && l.EndDate.HasValue && DateOnly.FromDateTime(l.StartDate.Value.ToUniversalTime()) <= currentDate && DateOnly.FromDateTime(l.EndDate.Value.ToUniversalTime()) >= currentDate && l.ApprovalStatus == "Approved")
                .Select(l => l.AppUserId).Distinct() .ToList();

            foreach (var userId in onLeaveUsers)
            {
                var attendance = dbcontext.Attendances
                    .FirstOrDefault(a => a.AppUserId == userId && a.CurrentDate == currentDate);

                if (attendance == null)
                {
                    dbcontext.Attendances.Add(new Attendance
                    {
                        AppUserId = userId,
                        CurrentDate = currentDate,
                        Status = AttendanceStatus.OnLeave.ToString(),
                        ClockIn = DateTimeHandler.GetStartDateTimeUtc(),
                        ClockOut = DateTimeHandler.GetClosingDateTimeUtc(),
                        ClockingType = false,
                        TotalHoursWorked = (decimal)(DateTimeHandler.GetClosingDateTimeUtc() - DateTimeHandler.GetStartDateTimeUtc()).TotalHours,
                        
                    });
                }
                else
                {
                    attendance.Status = AttendanceStatus.OnLeave.ToString();
                    dbcontext.Attendances.Update(attendance);
                }
            }

            dbcontext.SaveChanges();
            Console.WriteLine($"[{DateTime.UtcNow}] Updated leave statuses for {onLeaveUsers.Count} users.");


        }
    }
}
