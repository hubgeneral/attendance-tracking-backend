using FluentScheduler;
using attendance_tracking_backend.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace attendance_tracking_backend.Services
{
    public class TotalDailyHoursWorkedJob : IJob
    {
            public async void Execute()
            {

            using var scope = Program.ServiceProvider!.CreateScope();
            var dbcontext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

                // Step 1: Fetch all EntryExitLogs for today with valid ExitTime
                var groupedLogs = await dbcontext.EntryExitLogs.Where(e => e.CurrentDate == today && e.ExitTime != null)
                    .GroupBy(e => e.AttendanceId)
                    .Select(g => new
                    {
                        AttendanceId = g.Key,
                        TotalExitTime = g.Sum(x => (x.ExitTime!.Value.Ticks - x.EntryTime!.Value.Ticks) / (decimal)TimeSpan.TicksPerSecond)
                    })
                    .ToListAsync();

                // Step 2: Update related Attendance.TotalHoursWorked
                foreach (var logGroup in groupedLogs)
                {

                    var attendance = await dbcontext.Attendances.FirstOrDefaultAsync(a => a.Id == logGroup.AttendanceId);
                    if (attendance != null)
                    {
                        // convert seconds → hours (2 decimal places)
                        attendance.TotalHoursWorked = Math.Round((decimal)logGroup.TotalExitTime /3600m, 2);
                    }
                }
                await dbcontext.SaveChangesAsync();

            }
    }
}