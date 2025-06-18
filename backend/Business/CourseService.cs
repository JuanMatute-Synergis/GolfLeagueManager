using System.Linq;

namespace GolfLeagueManager
{
    public class CourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _courseRepository.GetAllAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(Guid id)
        {
            return await _courseRepository.GetByIdWithHolesAsync(id);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            return await _courseRepository.CreateAsync(course);
        }

        public async Task<Course> UpdateCourseAsync(Course course)
        {
            return await _courseRepository.UpdateAsync(course);
        }

        public async Task<bool> DeleteCourseAsync(Guid id)
        {
            return await _courseRepository.DeleteAsync(id);
        }

        public async Task<Course> UpdateCourseFromDataAsync(string courseName, CourseUpdateData courseData)
        {
            // Find the existing course by name
            var courses = await _courseRepository.GetAllAsync();
            var existingCourse = courses.FirstOrDefault(c => c.Name.Contains(courseName, StringComparison.OrdinalIgnoreCase));
            
            Course courseWithHoles;

            if (existingCourse == null)
            {
                // Course doesn't exist, create it
                courseWithHoles = new Course
                {
                    Name = courseName,
                    Location = string.Empty, // Will be updated if provided in courseData
                    SlopeRating = 113, // Default values
                    CourseRating = 72,
                    TotalPar = 72,
                    TotalYardage = 6400,
                    CourseHoles = new List<CourseHole>()
                };

                // Create basic 18-hole structure with default values
                for (int i = 1; i <= 18; i++)
                {
                    courseWithHoles.CourseHoles.Add(new CourseHole
                    {
                        HoleNumber = i,
                        Par = 4, // Default par, will be updated below if provided
                        Yardage = 350, // Default yardage, will be updated below if provided
                        HandicapIndex = i // Default handicap index, will be updated below if provided
                    });
                }

                // Create the course first to get the ID
                courseWithHoles = await _courseRepository.CreateAsync(courseWithHoles);
            }
            else
            {
                // Get the existing course with holes for update
                var retrievedCourse = await _courseRepository.GetByIdWithHolesAsync(existingCourse.Id);
                if (retrievedCourse == null)
                {
                    throw new InvalidOperationException("Course with holes not found");
                }
                courseWithHoles = retrievedCourse;
            }

            // Update course-level information
            if (!string.IsNullOrEmpty(courseData.Location))
                courseWithHoles.Location = courseData.Location;
            
            if (courseData.CourseRating.HasValue)
                courseWithHoles.CourseRating = courseData.CourseRating.Value;
            
            if (courseData.SlopeRating.HasValue)
                courseWithHoles.SlopeRating = courseData.SlopeRating.Value;
            
            if (courseData.TotalYardage.HasValue)
                courseWithHoles.TotalYardage = courseData.TotalYardage.Value;

            // Update hole data
            if (courseData.Holes != null && courseData.Holes.Any())
            {
                foreach (var holeData in courseData.Holes)
                {
                    var existingHole = courseWithHoles.CourseHoles.FirstOrDefault(h => h.HoleNumber == holeData.HoleNumber);
                    if (existingHole != null)
                    {
                        if (holeData.Par.HasValue)
                            existingHole.Par = holeData.Par.Value;
                        
                        if (holeData.HandicapIndex.HasValue)
                            existingHole.HandicapIndex = holeData.HandicapIndex.Value;
                        
                        if (holeData.Yardage.HasValue)
                            existingHole.Yardage = holeData.Yardage.Value;
                    }
                }

                // Recalculate total par
                courseWithHoles.TotalPar = courseWithHoles.CourseHoles.Sum(h => h.Par);
            }

            return await _courseRepository.UpdateAsync(courseWithHoles);
        }
    }
}
