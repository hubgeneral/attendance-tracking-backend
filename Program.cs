using attendance_tracking_backend.ClientHttp;
using attendance_tracking_backend.Data;
using attendance_tracking_backend.GraphQL;
using attendance_tracking_backend.Services;
using FluentScheduler;
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
        public static IServiceProvider? ServiceProvider { get; private set; } // variable for accessing services.

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
                options.Password.RequiredUniqueChars = 0;            
            })
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();

            //JWT Authentication *****************************************************************
            var secretKeyString = builder.Configuration["TokenSettings:Key"]!;
            if (string.IsNullOrEmpty(secretKeyString))
            {
                throw new InvalidOperationException("JWT key (TokenSettings:Key) is missing or empty.");
            }
            var secretKeyBytes = Encoding.ASCII.GetBytes(secretKeyString);
            var signingKey = new SymmetricSecurityKey(secretKeyBytes);

           // builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            builder.Services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; 
            })
               .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    //options.RequireHttpsMetadata = false; //for localhost
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer =  builder.Configuration["TokenSettings:Issuer"],
                        ValidAudience = builder.Configuration["TokenSettings:Audience"],
                        IssuerSigningKey = signingKey,
                        //IssuerSigningKey = new SymmetricSecurityKey(Base64UrlEncoder.DecodeBytes(builder.Configuration["TokenSettings:Key"]!)),
                        //IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["TokenSettings:Key"]!)),
                        ValidateLifetime = true,
                        //ClockSkew = TimeSpan.Zero,
                    };

                });

           // builder.Services.Configure<Jwt>(builder.Configuration.GetSection("Jwt")); ***********************************************************
            //End of JWT Authentication ***************************************************************


            //Http Clients and Services
            builder.Services.AddHttpClient<UserApiService>();
            builder.Services.AddHttpClient<FetchSaveLeaveService>();
            builder.Services.AddScoped<UserApiService>();
            builder.Services.AddScoped<UserDataService>();

            builder.Services.AddAuthorization();

            // GraphQL
            builder.Services.AddGraphQLServer()
                .AddQueryType<Query>() //root querytype
                    .AddTypeExtension<UserQuery>()
                    .AddTypeExtension<AttendanceQuery>()
                    .AddTypeExtension<DashboardQuery>()
                    .AddTypeExtension<ManualLogsQuery>()
                    .AddTypeExtension<RequestLogsQuery>()
                    .AddTypeExtension<LeaveQuery>()
                .AddMutationType<Mutation>()   //root mutationtype
                    .AddTypeExtension<UserMutation>()
                    .AddTypeExtension<AttendanceMutation>()
                    .AddTypeExtension<GeoFenceMutation>()
                    .AddTypeExtension<ManualLogsMutation>()
                    .AddTypeExtension<RequestLogsMutation>()
                .AddProjections()
                .AddFiltering()
                .AddSorting()
                .AddAuthorization();  //graphql server authorization;


            // CORS : old way of implementing cors
            /*  builder.Services.AddCors(options =>
              {
                 options.AddPolicy("AllowAll",p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
              });*/

            builder.Services.AddCors();
            var app = builder.Build();
            app.UseCors("AllowAll"); //Use Cors
            // After var app = builder.Build();
            Program.ServiceProvider = app.Services;

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

            //Initialize FluentScheduler AFTER ServiceProvider is available
            JobManager.Initialize(new LeaveJobRegistry());
            JobManager.Initialize(new TotalDailyHoursWorkedRegistry());
            JobManager.JobException += info =>
            {
               Console.WriteLine($"[Scheduler Error] {info.Exception.Message}");
            };

            // Middlewares
            if(app.Environment.IsDevelopment())
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
