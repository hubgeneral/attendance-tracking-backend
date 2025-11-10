using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace attendance_tracking_backend.DTO
{
    //Employee Data
    public class EmployeeData
    {
        [Key]
        public int Id { set; get; }
        public string? Email { set; get; }
        public string? EmployeeName { set; get; }
        public string? StaffId { set; get; }  //Username
        public string? Role { set; get; }
        public string? EmploymentType { set; get; }
    }

    //Leave Data
    public class LeaveData
    {
        public int Id { set; get; }
        public string? EmployeeName { set; get; }

        public string? Email { set; get; }

       [JsonConverter(typeof(NullableIntConverter))]
        public int? DaysRequested { set; get; }
        public DateTime? StartDate { set; get; }
        public DateTime? EndDate { set; get; }
        public string? ApprovalStatus { set; get; }
    }

    public class UserLoginResponse
    {
       
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Role {  get; set; }
        public string? AccessToken { get; set; }   // add token here
        public string? RefreshToken { get; set; }   // add token here
        public bool IsPasswordReset { get; set; }
        public string? ResetToken { get; set; }
    }

    public class UserForgotPasswordResponse
    {
        public string? Id { get; set; }
        public string? Token { get; set; }   // add token here
        public string? Username { get; set; }
    }

    public class UserResetPasswordResponse
    {
        public string? message { get; set; }
        public bool? IsPasswordReset { get; set; }
    }

    public class UserChangePassword
    {
        public string? Id { get; set; }
        public string? Token { get; set; }   // add token here
        public string? Username { get; set; }
    }

    public class UserWithRoleResponse
    {
        public int UserId { get; set; }
        public string? StaffId { get; set; } 
        public string? UserName { get; set; } 
        public string? EmployeeName { get; set; } 
        public string? EmployeeType { get; set; } 
        public string? Email { get; set; } 
        public int RoleId { get; set; } 
        public string? RoleName { get; set; } 
        public string? Status { get; set; } 


    }



    //Dashboard DTOs
    public class DashboardRequests
    {
        public int RequestId { set; get; }
        public string? Description { set; get; }
        public DateTime TimeOfDay { set; get; }
        public string? EmployeeName { set; get; }
    }

    public class LateEmployees
    {
        public string? EmployeeName { set; get; }
        public DateTime?  TimeOfDay { set; get; }       
    }

    public class PunctualEmployees
    {
        public string? EmployeeName { set; get; }
        public DateTime? TimeOfDay { set; get; }
    }

    public class WorkingHours
    {
        public decimal TotalWorkingHours {  get; set; }
        public decimal TotalOffHours {  get; set; } 

    }

    public class MostHoursWorkedByEmployee
    {
        public string? EmployeeName { set; get; }
        public decimal? TotalWorkingHours { get; set; }

    }

    public class MostHoursWastedByEmployee
    {
        public string? EmployeeName { set; get; }
        public decimal? TotalWastedHours { get; set; }
    }
}
