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
        public string? Token { get; set; }   // add token here
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Role {  get; set; }
        public bool IsPasswordReset { get; set; }
    }




    public class UserForgotPasswordResponse
    {
        public string? Token { get; set; }   // add token here
        public string? Id { get; set; }
        public string? Username { get; set; }
    }

    public class UserResetPasswordResponse
    {
        public string? Token { get; set; }   // add token here
        public string? Id { get; set; }
        public string? Username { get; set; }

    }

    public class UserChangePassword
    {
        public string? Token { get; set; }   // add token here
        public string? Id { get; set; }
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
}
