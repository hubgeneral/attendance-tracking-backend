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
        public string? Status { set; get; }
        public string? EmployeeType { set; get; }
        public ICollection<Leave>? Leaves { set; get; }
        public ICollection<Attendance>? Attendances { set; get; }
        public ICollection<Request>? Requests { set; get; }
        public ICollection<ActivityLogger>? ActivityLogger { set; get; }
    }

    public class Attendance
    {
        [Key]
        public int Id { get; set; }
        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public Decimal? TotalHoursWorked { set; get; }
        public string? Status { get; set; }
        public DateOnly CurrentDate { set; get; }
        public int AppUserId {  get; set; }
        public AppUser? User { get; set; }

    }

    public class Leave
    {
        [Key]
        public int Id { set; get; }
        public string? Email { set; get; }
        public string? EmployeeName {set; get; }
        public int? DaysRequested { set; get; }
        public DateTime? StartDate { set; get; }
        public DateTime? EndDate { set; get; }
        public string? ApprovalStatus { set; get; }

        public int AppUserId { get; set; }

        public AppUser? User { set; get; }
    }

    public class Request
    {
        public int Id { set; get; }
        public string? RequestDescription {  set; get; }
        public int AppUserId {  get; set; }
        public AppUser? User {  get; set; }
    }

    public class ActivityLogger
    {
        [Key]
        public int Id { set; get; }
        public string? ActivityLog { set; get; }
        public String? ActivityDescription { set; get; }
        
        public int AppUserId { get; set; }
        public AppUser? User { set; get; }
    }
  
    public class ExitLog
    {
        [Key]
        public int  Id {  get; set; }
        public DateTime ExitTime { get; set; }
        public int TotalExitTime { get; set; }
    }

}
