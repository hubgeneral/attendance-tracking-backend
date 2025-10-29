using attendance_tracking_backend.Data;
using HotChocolate.Authorization;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using attendance_tracking_backend.DTO;
using System;

namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class LeaveQuery
    {
        public IQueryable<AppUser> UsersOnLeave(
    DateOnly? startDate,
    DateOnly? endDate,
    [Service] DatabaseContext dbcontext)
        {
            var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);

            // Default to current work week (Monday–Friday)
            var startOfWeek = utcToday.AddDays(-(int)utcToday.DayOfWeek + (utcToday.DayOfWeek == DayOfWeek.Sunday ? 1 : 0));
            if (startOfWeek.DayOfWeek != DayOfWeek.Monday)
                startOfWeek = utcToday.AddDays(-(int)utcToday.DayOfWeek + 1);

            var start = startDate ?? startOfWeek;
            var end = endDate ?? start.AddDays(4); // Friday

            static DateTime AsUtcDate(DateOnly d, TimeOnly t) =>
                DateTime.SpecifyKind(d.ToDateTime(t), DateTimeKind.Utc);

            // Convert range to UTC DateTime
            var rangeStartUtc = AsUtcDate(start, TimeOnly.MinValue);
            var rangeEndUtc = AsUtcDate(end, TimeOnly.MaxValue);

            // Get user IDs whose approved leaves overlap the range
            var onLeaveUserIds = dbcontext.Leaves
                .Where(l =>
                    l.ApprovalStatus == "Approved" &&
                    l.StartDate <= rangeEndUtc &&
                    (l.EndDate ?? DateTime.MaxValue) >= rangeStartUtc)
                .Select(l => l.AppUserId)
                .Distinct();

            // Return full user details
            var usersOnLeave = dbcontext.AppUsers
                .Where(u => onLeaveUserIds.Contains(u.Id));
                

            return usersOnLeave;
        }


        public IQueryable<AppUser> UsersOnLeaveToday([Service] DatabaseContext dbcontext)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            static DateTime AsUtcDate(DateOnly d, TimeOnly t) =>
                DateTime.SpecifyKind(d.ToDateTime(t), DateTimeKind.Utc);

            // Define today's range (00:00–23:59 UTC)
            var dayStartUtc = AsUtcDate(today, TimeOnly.MinValue);
            var dayEndUtc = AsUtcDate(today, TimeOnly.MaxValue);

            // Get user IDs whose approved leave overlaps today
            var onLeaveUserIds = dbcontext.Leaves
                .Where(l =>
                    l.ApprovalStatus == "Approved" &&
                    l.StartDate <= dayEndUtc &&
                    (l.EndDate ?? DateTime.MaxValue) >= dayStartUtc)
                .Select(l => l.AppUserId)
                .Distinct();

            // Return full user details
            var usersOnLeaveToday = dbcontext.AppUsers
                .Where(u => onLeaveUserIds.Contains(u.Id));

            return usersOnLeaveToday;
        }

    }

}
