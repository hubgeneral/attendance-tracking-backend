using attendance_tracking_backend.DTO;
using attendance_tracking_backend.Data;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace attendance_tracking_backend.ClientHttp
{
    public class UserApiService 
    {
        private HttpClient? httpClient;
        private string employee_api_data;
       // private string leave_data_api;

        public UserApiService(HttpClient _httpClient)
        {
            httpClient = _httpClient;
            employee_api_data = "https://default57952406af2843c8b4dea4e06f5747.6d.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/dff87e0d5d4f462fb547c5c5a0abbae0/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=xlTKwja_ueoWWFXSJJ3u5e_4uuUM622MUHsJk3Y_S_Y";
        }

        public async Task<List<EmployeeData>> FetchEmployeeApiDataAsync()     //Task<List<EmployeeData>>
        {
            var response = await httpClient!.GetAsync(employee_api_data);
             response.EnsureSuccessStatusCode();
             var raw = await response.Content.ReadAsStringAsync();
             Console.WriteLine("API Response:");
             Console.WriteLine(raw);   
             var data = JsonSerializer.Deserialize<List<EmployeeData>>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
             
            return data ?? new List<EmployeeData>();

        }

       
    }
}
