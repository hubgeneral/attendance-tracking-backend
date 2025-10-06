using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace attendance_tracking_backend.DTO
{
    public class EmployeeData
    {
        [Key]
        public int Id { set; get; }
        public string? Email { set; get; }
        public string? EmployeeName { set; get; }
        public string? StaffId { set; get; }  //Username
    }



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
}
