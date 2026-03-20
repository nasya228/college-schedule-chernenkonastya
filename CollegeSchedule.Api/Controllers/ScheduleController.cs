using CollegeSchedule.Api.Services; 
using Microsoft.AspNetCore.Mvc;


namespace CollegeSchedule.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _service;

        public ScheduleController(IScheduleService service)
        {
            _service = service;
        }

        [HttpGet("group/{groupName}")]
        public async Task<IActionResult> GetSchedule(
            [FromRoute] string groupName,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            try
            {
                var result = await _service.GetScheduleForGroup(groupName, start, end);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}