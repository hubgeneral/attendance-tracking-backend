using attendance_tracking_backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HotChocolate; 
using HotChocolate.Types;
using attendance_tracking_backend.Helpers;


namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class GeoFenceMutation
    {
        // Clock In
        public async Task<string> GeofenceClockIn(int id, DateTime clockinUtc, [Service] DatabaseContext dbcontext)
        {
            var clockInTime = clockinUtc.ToUniversalTime();

            var user = await dbcontext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new GraphQLException("User does not exist");

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Check if user is on leave today
            var onLeave = await dbcontext.Leaves.AnyAsync(l =>
                l.AppUserId == user.Id &&
                l.StartDate.HasValue && l.EndDate.HasValue &&
                DateOnly.FromDateTime(l.StartDate.Value.ToUniversalTime()) <= today &&
                DateOnly.FromDateTime(l.EndDate.Value.ToUniversalTime()) >= today
            );

            if (onLeave)
            {
                // Optionally create an attendance record with OnLeave status
                var leaveAttendance = await dbcontext.Attendances.FirstOrDefaultAsync(a => a.AppUserId == user.Id && a.CurrentDate == today);
                if (leaveAttendance == null)
                {
                    leaveAttendance = new Attendance
                    {
                        AppUserId = user.Id,
                        CurrentDate = today,
                        Status = AttendanceStatus.OnLeave.ToString(),
                        ClockIn = DateTimeHandler.GetStartDateTimeUtc(),
                        ClockOut = DateTimeHandler.GetClosingDateTimeUtc(),
                    };
                    await dbcontext.Attendances.AddAsync(leaveAttendance);
                    await dbcontext.SaveChangesAsync();
                }

                return "User is on leave today";
            }
            else { 

                    // Ensure attendance exists for today
                    var attendance = await dbcontext.Attendances.Include(a => a.EntryExitLogs).FirstOrDefaultAsync(a => a.AppUserId == user.Id && a.CurrentDate == today);

                        if (attendance == null)
                        {
                            attendance = new Attendance
                            {
                                AppUserId = user.Id,
                                ClockIn = clockInTime,
                                ClockingType = true,
                                CurrentDate = today,
                                Status = AttendanceStatus.Present.ToString()
                            };
                            await dbcontext.Attendances.AddAsync(attendance);
                            await dbcontext.SaveChangesAsync();
                        }
                        else
                        {
                            // If there is already an open session (EntryTime set and ExitTime null) -> ignore duplicate clock-ins
                            var openLog = await dbcontext.EntryExitLogs.FirstOrDefaultAsync(e => e.AppUserId == user.Id && e.CurrentDate == today && e.EntryTime ==clockInTime  && e.ExitTime == null);
                            if (openLog != null)
                            {
                                return $"Already clocked in at {openLog.EntryTime:HH:mm:ss} (UTC)";
                            }

                            attendance.ClockingType = true;
                            attendance.ClockIn ??= clockInTime;
                            attendance.Status = AttendanceStatus.Present.ToString();
                            dbcontext.Attendances.Update(attendance);
                            await dbcontext.SaveChangesAsync();
                        }

                        // Start a new EntryExitLog
                        var entryLog = new EntryExitLog
                        {
                            AppUserId = user.Id,
                            EntryTime = clockInTime,
                            CurrentDate = today,
                            AttendanceId = attendance.Id
                        };
                        await dbcontext.EntryExitLogs.AddAsync(entryLog);
                        await dbcontext.SaveChangesAsync();
            }
            return $"{user.EmployeeName} Clocked in at {clockInTime:HH:mm:ss} (UTC)";
        }

        // Clock Out
        public async Task<string> GeofenceClockOut(int id, DateTime clockoutUtc, [Service] DatabaseContext dbcontext)
        {
            var clockOutTime = clockoutUtc.ToUniversalTime();

            var user = await dbcontext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new GraphQLException("User does not exist");

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Ensure attendance exists
            var attendance = await dbcontext.Attendances.Include(a => a.EntryExitLogs)
                .FirstOrDefaultAsync(a => a.AppUserId == user.Id && a.CurrentDate == today);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    AppUserId = user.Id,
                    ClockIn = null,
                    ClockOut = clockOutTime,
                    ClockingType = false,
                    CurrentDate = today,
                    Status = AttendanceStatus.Present.ToString()
                };
                await dbcontext.Attendances.AddAsync(attendance);
                await dbcontext.SaveChangesAsync();
            }

            // Try to find an open entry-exit log
            var openLog = await dbcontext.EntryExitLogs.FirstOrDefaultAsync(e => e.AppUserId == user.Id && e.CurrentDate == today && e.EntryTime != null && e.ExitTime == null);

            if (openLog == null)
            {
                // No open session: create a synthetic session with entry = clockOutTime (zero-length)
                openLog = new EntryExitLog
                {
                    AppUserId = user.Id,
                    EntryTime = clockOutTime,
                    ExitTime = clockOutTime,
                    CurrentDate = today,
                    AttendanceId = attendance.Id
                };
                await dbcontext.EntryExitLogs.AddAsync(openLog);
            }
            else
            {
                openLog.ExitTime = clockOutTime;
                dbcontext.EntryExitLogs.Update(openLog);
            }

            // Update attendance's clock out and clocking type
            attendance.ClockOut = clockOutTime;
            attendance.ClockingType = false;
            dbcontext.Attendances.Update(attendance);
            await dbcontext.SaveChangesAsync();

            // Now compute hours strictly inside the official window: 08:00 - 17:00 UTC
            var workStart = DateTimeHandler.GetStartDateTimeUtc(DateTime.UtcNow);
            var workEnd = DateTimeHandler.GetClosingDateTimeUtc(DateTime.UtcNow);
            var totalWindowSeconds = (workEnd - workStart).TotalSeconds; // should be 9 * 3600 = 32400

            // Get all today's logs that have both entry and exit
            var logs = await dbcontext.EntryExitLogs
                .Where(e => e.AppUserId == user.Id && e.CurrentDate == today && e.EntryTime != null && e.ExitTime != null)
                .OrderBy(e => e.EntryTime)
                .ToListAsync();

            double totalSecondsInsideWindow = 0.0;
            foreach (var log in logs)
            {
                var entry = log.EntryTime!.Value.ToUniversalTime();
                var exit = log.ExitTime!.Value.ToUniversalTime();

                // defensive: ensure entry <= exit
                if (exit < entry)
                {
                    var t = entry;
                    entry = exit;
                    exit = t;
                }

                totalSecondsInsideWindow += TimeRangeHelper.OverlapSeconds(entry, exit, workStart, workEnd);
            }

            // Convert to hours with 2 decimals using decimal math
            decimal hoursWorked = 0m;
            if (totalSecondsInsideWindow > 0)
            {
                hoursWorked = Math.Round((decimal)totalSecondsInsideWindow / 3600m, 2);
            }

            attendance.TotalHoursWorked = hoursWorked;

            // Determine absence: if there were no logs or worked hours == 0 and not on leave -> Absent
            var onLeave = await dbcontext.Leaves.AnyAsync(l =>
                l.AppUserId == user.Id &&
                l.StartDate.HasValue && l.EndDate.HasValue &&
                DateOnly.FromDateTime(l.StartDate.Value.ToUniversalTime()) <= today &&
                DateOnly.FromDateTime(l.EndDate.Value.ToUniversalTime()) >= today
            );

            if ((!logs.Any() || hoursWorked == 0m) && !onLeave)
            {
                attendance.Status = AttendanceStatus.Absent.ToString();
            }
            else if (onLeave)
            {
                attendance.Status = AttendanceStatus.OnLeave.ToString();
            }
            else
            {
                attendance.Status = AttendanceStatus.Present.ToString();
            }

            dbcontext.Attendances.Update(attendance);
            await dbcontext.SaveChangesAsync();

            return $"{user.EmployeeName} clocked out at {clockOutTime:HH:mm:ss} (UTC). Hours counted (08:00-17:00): {attendance.TotalHoursWorked}h. Status: {attendance.Status}";
        }

    }
}
