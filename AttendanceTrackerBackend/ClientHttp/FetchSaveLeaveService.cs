using attendance_tracking_backend.Data;
using attendance_tracking_backend.DTO;
using Microsoft.EntityFrameworkCore;

using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace attendance_tracking_backend.ClientHttp
{
    public class FetchSaveLeaveService
    {
        private readonly HttpClient? httpClient;
        private readonly DatabaseContext? dbcontext;
        private readonly string? leave_api_data;

        public FetchSaveLeaveService() { }

        public FetchSaveLeaveService(HttpClient _httpClient, DatabaseContext _dbcontext)
        {
            httpClient = _httpClient;
            dbcontext = _dbcontext;
            leave_api_data = "https://default57952406af2843c8b4dea4e06f5747.6d.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/2ac2b25429ac4af1ac15fd90569928f5/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=6sRh4jrg082VTv2aGdM4QIdBBHHRPRw0fMWJMUKySwA";
        }



        public async Task<List<LeaveData>> FetchLeaveDataAsync()
        {
            var response = await httpClient!.GetAsync(leave_api_data);
            response.EnsureSuccessStatusCode();
            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API Response:");
            Console.WriteLine(raw);
            var data = JsonSerializer.Deserialize<List<LeaveData>>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return data ?? new List<LeaveData>();
        }

        public async Task StoreLeaveDataAsync(List<LeaveData> data)
        {
            if (data == null || data.Count == 0) return;

            
                foreach (var dto in data)
                {
                        // Check if email already exists to avoid duplicates
                    var existingLeave = await dbcontext!.Leaves
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && DateTime.SpecifyKind((DateTime)u.StartDate!.Value, DateTimeKind.Utc) == DateTime.SpecifyKind((DateTime)dto.StartDate!.Value, DateTimeKind.Utc));

                    if (existingLeave != null) continue;

                    var appUserId = await dbcontext.AppUsers.Where(u => u.Email == dto.Email).Select(u => u.Id).FirstOrDefaultAsync();

                    var leave = new Leave
                    {
                        Email = dto.Email,
                        EmployeeName = dto.EmployeeName,
                        DaysRequested = dto.DaysRequested,
                        StartDate = DateTime.SpecifyKind((DateTime)dto.StartDate!.Value, DateTimeKind.Utc),
                        EndDate = DateTime.SpecifyKind((DateTime)dto.EndDate!.Value, DateTimeKind.Utc),
                        ApprovalStatus = dto.ApprovalStatus,
                        AppUserId = appUserId
                    };

                        dbcontext.Leaves.Add(leave);
                    
                }
                await dbcontext!.SaveChangesAsync();

            
        }
    }
}
