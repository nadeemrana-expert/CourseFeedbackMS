using Abp.Domain.Repositories;
using CourseFeedbackMS.CourseFeedback;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CourseFeedbackMS.CourseFeedback.Jobs
{
    public class FeedbackCheckJob
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Feedback> _feedbackRepository;

        public FeedbackCheckJob(
            IRepository<Course> courseRepository,
            IRepository<Feedback> feedbackRepository)
        {
            _courseRepository = courseRepository;
            _feedbackRepository = feedbackRepository;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task CheckCoursesForInactiveFeedback()
        {
            var tenDaysAgo = DateTime.UtcNow.AddDays(-10);

            var activeCourses = await _courseRepository
                .GetAll()
                .Where(c => c.IsActive)
                .ToListAsync();

            foreach (var course in activeCourses)
            {
                var hasRecentFeedback = await _feedbackRepository
                    .GetAll()
                    .AnyAsync(f => f.CourseId == course.Id && f.CreatedDate >= tenDaysAgo);

                if (!hasRecentFeedback)
                {
                    // Log or flag courses with no recent feedback
                    Console.WriteLine($"[FeedbackCheckJob] Course '{course.CourseName}' (ID: {course.Id}) has no feedback in the last 10 days.");
                }
            }
        }
    }
}
