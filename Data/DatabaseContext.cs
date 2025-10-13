using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace attendance_tracking_backend.Data
{
    public class DatabaseContext : IdentityDbContext<AppUser,IdentityRole<int>, int> //DbContext
    {
         public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Leave> Leaves { get; set; }

        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole<int>>().HasData(
                 new IdentityRole<int>  {  Id = 1,  Name = "Admin", NormalizedName = "ADMIN"},
                 new IdentityRole<int>  { Id = 2, Name = "User",  NormalizedName = "USER" }
                );

            //Entity Relationships
            //************************
            modelBuilder.Entity<Leave>()
               .HasOne(l => l.User)
               .WithMany(u => u.Leaves)
               .HasForeignKey(l => l.AppUserId);

            modelBuilder.Entity<Attendance>()  
                .HasOne(a => a.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(a => a.AppUserId);

            //fetch  employee and leave data from api
            //****************************************
            //prevent duplicates 
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique(); 

        }
    }
}
