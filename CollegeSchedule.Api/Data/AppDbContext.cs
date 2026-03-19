using CollegeSchedule.Api.Models;
using CollegeSchedule.Models;
using Microsoft.EntityFrameworkCore;

namespace CollegeSchedule.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet для каждой модели (таблицы)
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<StudentGroup> StudentGroups { get; set; }
        public DbSet<Weekday> Weekdays { get; set; }
        public DbSet<LessonTime> LessonTimes { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Индексы для Schedule
            modelBuilder.Entity<Schedule>()
                .HasIndex(s => new { s.LessonDate, s.LessonTimeId, s.GroupId, s.GroupPart })
                .IsUnique();

            modelBuilder.Entity<Schedule>()
                .HasIndex(s => new { s.LessonDate, s.LessonTimeId, s.ClassroomId })
                .IsUnique();

            // Конвертация enum в string
            modelBuilder.Entity<Schedule>()
                .Property(s => s.GroupPart)
                .HasConversion<string>();
        }
    }
}