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

        public async Task<Course> CreateAllentownMunicipalCourseAsync()
        {
            var course = new Course
            {
                Name = "Allentown Municipal Golf Course",
                Location = "Allentown, PA",
                SlopeRating = 113,
                CourseRating = 72,
                CourseHoles = new List<CourseHole>
                {
                    new CourseHole { HoleNumber = 1, Par = 4, Yardage = 380, HandicapIndex = 3 },
                    new CourseHole { HoleNumber = 2, Par = 3, Yardage = 165, HandicapIndex = 11 },
                    new CourseHole { HoleNumber = 3, Par = 5, Yardage = 520, HandicapIndex = 1 },
                    new CourseHole { HoleNumber = 4, Par = 4, Yardage = 420, HandicapIndex = 5 },
                    new CourseHole { HoleNumber = 5, Par = 3, Yardage = 180, HandicapIndex = 17 },
                    new CourseHole { HoleNumber = 6, Par = 4, Yardage = 400, HandicapIndex = 7 },
                    new CourseHole { HoleNumber = 7, Par = 5, Yardage = 540, HandicapIndex = 13 },
                    new CourseHole { HoleNumber = 8, Par = 4, Yardage = 360, HandicapIndex = 9 },
                    new CourseHole { HoleNumber = 9, Par = 4, Yardage = 390, HandicapIndex = 15 },
                    new CourseHole { HoleNumber = 10, Par = 4, Yardage = 410, HandicapIndex = 12 },
                    new CourseHole { HoleNumber = 11, Par = 3, Yardage = 170, HandicapIndex = 4 },
                    new CourseHole { HoleNumber = 12, Par = 5, Yardage = 560, HandicapIndex = 2 },
                    new CourseHole { HoleNumber = 13, Par = 4, Yardage = 440, HandicapIndex = 6 },
                    new CourseHole { HoleNumber = 14, Par = 3, Yardage = 190, HandicapIndex = 14 },
                    new CourseHole { HoleNumber = 15, Par = 4, Yardage = 380, HandicapIndex = 10 },
                    new CourseHole { HoleNumber = 16, Par = 5, Yardage = 530, HandicapIndex = 8 },
                    new CourseHole { HoleNumber = 17, Par = 4, Yardage = 370, HandicapIndex = 18 },
                    new CourseHole { HoleNumber = 18, Par = 4, Yardage = 415, HandicapIndex = 16 }
                }
            };

            // Calculate total par and yardage
            course.TotalPar = course.CourseHoles.Sum(h => h.Par);
            course.TotalYardage = course.CourseHoles.Sum(h => h.Yardage);

            return await _courseRepository.CreateAsync(course);
        }
    }
}
