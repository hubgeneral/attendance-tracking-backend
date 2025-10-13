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

        public string? Role { get; set; }
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
        public int UserId { set; get; }
        public string? EmployeeName { set; get; }
        public string? Email { set; get; }
        public string? StaffId { set; get; }
        public string? UserName { set; get; }
        public string? Status { set; get; }
        public string? EmployeeType { set; get; }
        public int RoleId { set; get; }
        public string? RoleName { set; get; }
    }

}