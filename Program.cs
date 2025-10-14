using attendance_tracking_backend.ClientHttp;
using attendance_tracking_backend.Data;
using attendance_tracking_backend.GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

namespace attendance_tracking_backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PgsqlConnection")));
            // Identity
            builder.Services.AddIdentity<AppUser,AppRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // for localhost
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.AddAuthorization();
            // Http Clients and Services
            builder.Services.AddHttpClient<UserApiService>();
            builder.Services.AddHttpClient<FetchSaveLeaveService>();
            builder.Services.AddScoped<UserApiService>();
           builder.Services.AddScoped<UserDataService>();

            // GraphQL
            builder.Services.AddGraphQLServer()
                .AddQueryType<UserQuery>()
                .AddType<AttendanceQuery>() //extended querytype
                .AddMutationType<UserMutation>()
                .AddType<AttendanceMutation>()    //extended mutationtype
                .AddProjections()
                .AddFiltering()
                .AddSorting();
               //.AddType<UserType>() // optional

            // CORS
            builder.Services.AddCors();

            var app = builder.Build();

           
            using (var scope = app.Services.CreateScope())
            {
 
                // fetch and store employee data
                var apiService = scope.ServiceProvider.GetRequiredService<UserApiService>();
                var dataService = scope.ServiceProvider.GetRequiredService<UserDataService>();
                var emp_data = await apiService.FetchEmployeeApiDataAsync();
                Console.WriteLine(JsonSerializer.Serialize(emp_data));
                await dataService.StoreDataAsync(emp_data);

                // fetch and store leave data
                var leaveService = scope.ServiceProvider.GetRequiredService<FetchSaveLeaveService>();
                var leave_data = await leaveService.FetchLeaveDataAsync();
                Console.WriteLine(JsonSerializer.Serialize(leave_data));
                await leaveService.StoreLeaveDataAsync(leave_data);
            }

            // Middlewares
            if (app.Environment.IsDevelopment())
            {
                app.UseCors(policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            }
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapGraphQL();
            app.Run();
        }
    }
}
