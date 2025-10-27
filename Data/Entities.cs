using Microsoft.AspNetCore.Identity;  //identity namespace
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace attendance_tracking_backend.Data
{
    public class AppUser :  IdentityUser<int>
    {
        override
        public int Id { set; get; }
        public string? EmployeeName { set; get; }
        override
        public string? Email { set; get; }
        public string? StaffId { set; get; }
        override
        public string? UserName { set; get; }
        override
        public string? NormalizedUserName { set; get; }
        public string? Password { set; get; }
        public bool IsPasswordReset {  set; get; }
        public string? Status { set; get; }
        public string? EmployeeType { set; get; }
        public ICollection<Leave>? Leaves { set; get; }
        public ICollection<Attendance>? Attendances { set; get; }
        public ICollection<RequestLog>? RequestLogs { set; get; }
        public ICollection<EntryExitLog>? EntryExitLogs { set; get; } 
        public ICollection<AppUserRole> UserRoles { get; set; } = [];
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }

    public class AppRole : IdentityRole<int> {

        public AppRole() { }
        public AppRole(string roleName) : base(roleName){}
        public ICollection<AppUserRole> UserRoles { get; set; } = [];
    }

    public class AppUserRole : IdentityUserRole<int>
    {
        public required AppUser User { get; set; }
        public required AppRole Role { get; set; }
    }

    public class Attendance
    {
        [Key]
        public int Id { get; set; }
        public DateTime? ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public bool ClockingType { get; set; } = false;
        public Decimal? TotalHoursWorked { set; get; }
        public string? Status { get; set; }
        public DateOnly? CurrentDate { set; get; }
        public int AppUserId {  get; set; }
        public AppUser? User { get; set; }
        public ICollection<EntryExitLog> EntryExitLogs { get; set; } = [];
    }

    public class EntryExitLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime? ExitTime { get; set; }
        public DateTime? EntryTime { get; set; }
        public int? TotalExitTime { get; set; }
        public DateOnly? CurrentDate { get; set; }
        public int? AppUserId { get; set; }
        public AppUser? User { set; get; }
        public int AttendanceId { get; set; }
        public Attendance? Attendance { get; set; }
    }

  
    public class RequestLog
    {
        public int Id { set; get; }
        public string? EmployeeName { set; get; }
        public string? Reason { set; get; }
        public DateTime? TimeOfDay { set; get; }
        public DateTime? ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public string? ApprovalStatus { set; get; }
         public string? ActionBy { get; set; }

        public int AppUserId { get; set; }
        public AppUser? User { get; set; }


    }
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; }
        public int AppUserId { get; set; }
        public AppUser? User { get; set; }
    }

   


    public class Leave
    {
        [Key]
        public int Id { set; get; }
        public string? Email { set; get; }
        public string? EmployeeName { set; get; }
        public int? DaysRequested { set; get; }
        public DateTime? StartDate { set; get; }
        public DateTime? EndDate { set; get; }
        public string? ApprovalStatus { set; get; }
        public int AppUserId { get; set; }
        public AppUser? User { set; get; }
    }
}
