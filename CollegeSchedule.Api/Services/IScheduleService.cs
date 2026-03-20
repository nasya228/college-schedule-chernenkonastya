using CollegeSchedule.DTO;
namespace CollegeSchedule.Api.Services
{
    public interface IScheduleService
    {
        Task<List<ScheduleByDateDto>> GetScheduleForGroup(string groupName, DateTime
       startDate, DateTime endDate);
        Task<List<string>> GetAllGroups();
    }
}