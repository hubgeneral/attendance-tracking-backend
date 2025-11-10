using attendance_tracking_backend.Data;

namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class AttendanceMutation
    {
        public async Task<Attendance> CreateAttendance(
                DateTime clockin,
                DateTime clockout,
                int totalhoursworked,
                string status,
                DateOnly currentdate,
                int appuserid,
                [Service] DatabaseContext dbcontext
            )
        {
            var attendance = new Attendance()
            {
                ClockIn = clockin,
                ClockOut = clockout,
                TotalHoursWorked = totalhoursworked,
                Status = status,
                CurrentDate = currentdate,
                AppUserId = appuserid
            };

            dbcontext.Attendances.Add(attendance);
            await dbcontext.SaveChangesAsync();
            return attendance;
        }

        public async Task<Attendance?> UpdateAttendance(
             DateTime clockin,
             DateTime clockout,
             int totalhoursworked,
             string status,
             DateOnly currentdate,
             int appuserid,
             [Service] DatabaseContext dbcontext
         )
        {
            var attendance = dbcontext.Attendances.FirstOrDefault(a => a.AppUserId == appuserid && a.CurrentDate == currentdate);

            if (attendance == null) return null;

            attendance.ClockIn = clockin;
            attendance.ClockOut = clockout;
            attendance.TotalHoursWorked = totalhoursworked;
            attendance.Status = status;
            attendance.CurrentDate = currentdate;
            attendance.AppUserId = appuserid;

            await dbcontext.SaveChangesAsync();
            return attendance;
        }

        public async Task<bool> DeleteAttendance(int id, [Service] DatabaseContext dbcontext)
        {
            var attendance = dbcontext.Attendances.Find(id);
            if (attendance == null) return false;

            dbcontext.Attendances.Remove(attendance);
            await dbcontext.SaveChangesAsync();
            return true;
        }
    }
}
