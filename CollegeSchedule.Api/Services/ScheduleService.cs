using CollegeSchedule.Api.Models;
using CollegeSchedule.Data;      
using CollegeSchedule.DTO;       
using CollegeSchedule.Models;    
using Microsoft.EntityFrameworkCore;

namespace CollegeSchedule.Api.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly AppDbContext _db;

        public ScheduleService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ScheduleByDateDto>> GetScheduleForGroup(string groupName, DateTime startDate, DateTime endDate)
        {
            var group = await _db.StudentGroups
                .FirstOrDefaultAsync(g => g.GroupName == groupName);

            if (group == null)
                throw new KeyNotFoundException($"Группа {groupName} не найдена.");

            var schedules = await _db.Schedules
                .Where(s => s.GroupId == group.GroupId &&
                           s.LessonDate >= startDate &&
                           s.LessonDate <= endDate)
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Classroom)
                    .ThenInclude(c => c.Building)
                .OrderBy(s => s.LessonDate)
                .ThenBy(s => s.LessonTime.LessonNumber)
                .ThenBy(s => s.GroupPart)
                .ToListAsync();

            return BuildScheduleDto(startDate, endDate, schedules);
        }

        public async Task<List<string>> GetAllGroups()
        {
            return await _db.StudentGroups
                .OrderBy(g => g.GroupName)
                .Select(g => g.GroupName)
                .ToListAsync();
        }

        private List<ScheduleByDateDto> BuildScheduleDto(DateTime startDate, DateTime endDate, List<Schedule> schedules)
        {
            var scheduleByDate = schedules
                .GroupBy(s => s.LessonDate)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<ScheduleByDateDto>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                if (!scheduleByDate.TryGetValue(date, out var daySchedules))
                {
                    result.Add(new ScheduleByDateDto
                    {
                        LessonDate = date,
                        Weekday = GetRussianWeekday(date.DayOfWeek),
                        Lessons = new List<LessonDto>()
                    });
                }
                else
                {
                    result.Add(BuildDayDto(daySchedules));
                }
            }

            return result;
        }

        private ScheduleByDateDto BuildDayDto(List<Schedule> daySchedules)
        {
            var lessons = daySchedules
                .GroupBy(s => new { s.LessonTime.LessonNumber, s.LessonTime.TimeStart, s.LessonTime.TimeEnd })
                .Select(BuildLessonDto)
                .ToList();

            return new ScheduleByDateDto
            {
                LessonDate = daySchedules.First().LessonDate,
                Weekday = daySchedules.First().Weekday.Name,
                Lessons = lessons
            };
        }

        private LessonDto BuildLessonDto(IGrouping<dynamic, Schedule> lessonGroup)
        {
            var lessonDto = new LessonDto
            {
                LessonNumber = lessonGroup.Key.LessonNumber,
                Time = $"{lessonGroup.Key.TimeStart:hh\\:mm}-{lessonGroup.Key.TimeEnd:hh\\:mm}",
                GroupParts = new Dictionary<LessonGroupPart, LessonPartDto?>()
            };

            foreach (var part in lessonGroup)
            {
                lessonDto.GroupParts[part.GroupPart] = new LessonPartDto
                {
                    Subject = part.Subject.Name,
                    Teacher = $"{part.Teacher.LastName} {part.Teacher.FirstName} {part.Teacher.MiddleName}".Trim(),
                    TeacherPosition = part.Teacher.Position,
                    Classroom = part.Classroom.RoomNumber,
                    Building = part.Classroom.Building.Name,
                    Address = part.Classroom.Building.Address
                };
            }

            return lessonDto;
        }

        private string GetRussianWeekday(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                _ => day.ToString()
            };
        }
    }
}