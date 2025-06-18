using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly CourseService _courseService;

        public CoursesController(CourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetAllCourses()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(Guid id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            return Ok(course);
        }

        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse(Course course)
        {
            try
            {
                var createdCourse = await _courseService.CreateCourseAsync(course);
                return Created($"/api/courses/{createdCourse.Id}", createdCourse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Course>> UpdateCourse(Guid id, Course course)
        {
            if (id != course.Id)
                return BadRequest("Course ID mismatch");

            try
            {
                var updatedCourse = await _courseService.UpdateCourseAsync(course);
                return Ok(updatedCourse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var deleted = await _courseService.DeleteCourseAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPut("upsert-data/{courseName}")]
        public async Task<ActionResult<Course>> UpsertCourseData(string courseName, [FromBody] CourseUpdateData courseData)
        {
            try
            {
                var updatedCourse = await _courseService.UpdateCourseFromDataAsync(courseName, courseData);
                return Ok(updatedCourse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("seed-players")]
        public async Task<ActionResult> SeedPlayersWithHandicaps([FromServices] DataSeeder dataSeeder)
        {
            try
            {
                await dataSeeder.SeedPlayersWithHandicapsAsync();
                return Ok(new { message = "Players seeded successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-data/{courseName}")]
        public async Task<ActionResult<Course>> UpdateCourseData(string courseName, [FromBody] CourseUpdateData courseData)
        {
            try
            {
                var updatedCourse = await _courseService.UpdateCourseFromDataAsync(courseName, courseData);
                return Ok(updatedCourse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
