using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using CourseFeedbackMS.Authorization;
using CourseFeedbackMS.Authorization.Roles;
using CourseFeedbackMS.Authorization.Users;
using CourseFeedbackMS.Configuration;
using CourseFeedbackMS.CourseFeedback.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CourseFeedbackMS.CourseFeedback
{
    [AbpAuthorize(PermissionNames.Pages_Feedbacks)]
    public class FeedbackAppService : ApplicationService, IFeedbackAppService
    {
        private readonly IRepository<Feedback> _feedbackRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly UserManager _userManager;

        public FeedbackAppService(
            IRepository<Feedback> feedbackRepository,
            IRepository<Course> courseRepository,
            UserManager userManager)
        {
            _feedbackRepository = feedbackRepository;
            _userManager = userManager;
            _courseRepository = courseRepository;
        }

        private async Task<(bool IsAdmin, bool IsTeacher, bool IsStudent, User CurrentUser)> GetCurrentUserWithRolesAsync()
        {
            var user = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());
            var roles = await _userManager.GetRolesAsync(user);
            return (
                roles.Contains(StaticRoleNames.Tenants.Admin),
                roles.Contains(StaticRoleNames.Tenants.Teacher),
                roles.Contains(StaticRoleNames.Tenants.Student),
                user
            );
        }

        public async Task<PagedResultDto<FeedbackDto>> GetAllAsync(GetFeedbacksInput input)
        {
            var query = _feedbackRepository
                .GetAllIncluding(f => f.Course)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    f => f.StudentName.Contains(input.Filter) || f.Comment.Contains(input.Filter))
                .WhereIf(input.CourseId.HasValue, f => f.CourseId == input.CourseId.Value)
                .WhereIf(input.Rating.HasValue, f => f.Rating == input.Rating.Value);

            // ── Role-based data scoping ──
            var (isAdmin, isTeacher, isStudent, currentUser) = await GetCurrentUserWithRolesAsync();

            if (isStudent)
            {
                query = query.Where(f => f.StudentName == currentUser.FullName);
            }
            else if (isTeacher)
            {
                query = query.Where(f => f.Course.InstructorName == currentUser.FullName);
            }
            // Admin sees all feedbacks

            var totalCount = await query.CountAsync();

            var feedbacks = await query
                .OrderBy(string.IsNullOrEmpty(input.Sorting) ? "CreatedDate desc" : input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<FeedbackDto>(totalCount, ObjectMapper.Map<List<FeedbackDto>>(feedbacks));
        }

        public async Task<FeedbackDto> GetAsync(EntityDto input)
        {
            var feedback = await _feedbackRepository
                .GetAllIncluding(f => f.Course)
                .FirstOrDefaultAsync(f => f.Id == input.Id);

            return ObjectMapper.Map<FeedbackDto>(feedback);
        }

        [AbpAuthorize(PermissionNames.Pages_Feedbacks_Create)]
        public async Task<FeedbackDto> CreateAsync(CreateFeedbackDto input)
        {
            // Business rule: only students can submit feedback
            var (_, _, isStudent, currentUser) = await GetCurrentUserWithRolesAsync();
            if (!isStudent)
            {
                throw new UserFriendlyException("Only students can submit feedback.");
            }

            // Tenant setting check: max feedbacks per course
            var maxFeedbackStr = await SettingManager.GetSettingValueAsync(
                AppSettingNames.MaxFeedbackPerCourse);

            if (int.TryParse(maxFeedbackStr, out int maxFeedback) && maxFeedback > 0)
            {
                var existingCount = await _feedbackRepository
                    .CountAsync(f => f.CourseId == input.CourseId);

                if (existingCount >= maxFeedback)
                {
                    throw new UserFriendlyException(
                        $"Maximum feedback limit ({maxFeedback}) reached for this course.");
                }
            }

            var feedback = ObjectMapper.Map<Feedback>(input);
            feedback.CreatedDate = DateTime.UtcNow;

            // Auto-set student name from logged-in user
            feedback.StudentName = currentUser.FullName;

            await _feedbackRepository.InsertAsync(feedback);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(new EntityDto(feedback.Id));
        }

        [AbpAuthorize(PermissionNames.Pages_Feedbacks_Edit)]
        public async Task<FeedbackDto> UpdateAsync(UpdateFeedbackDto input)
        {
            var currentUser = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());
            var feedback = await _feedbackRepository.GetAsync(input.Id);

            // Ownership check: only the submitting student can edit
            if (feedback.StudentName != currentUser.FullName)
            {
                throw new UserFriendlyException("You can only edit your own feedback.");
            }

            ObjectMapper.Map(input, feedback);
            feedback.StudentName = currentUser.FullName; // prevent name tampering
            await CurrentUnitOfWork.SaveChangesAsync();
            return await GetAsync(new EntityDto(feedback.Id));
        }

        [AbpAuthorize(PermissionNames.Pages_Feedbacks_Delete)]
        public async Task DeleteAsync(EntityDto input)
        {
            var (isAdmin, _, _, currentUser) = await GetCurrentUserWithRolesAsync();

            if (!isAdmin)
            {
                // Non-admin users can only delete their own feedback
                var feedback = await _feedbackRepository.GetAsync(input.Id);
                if (feedback.StudentName != currentUser.FullName)
                {
                    throw new UserFriendlyException("You can only delete your own feedback.");
                }
            }

            await _feedbackRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(PermissionNames.Pages_Dashboard)]
        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var (isAdmin, isTeacher, isStudent, currentUser) = await GetCurrentUserWithRolesAsync();

            // Build role-scoped queries
            var feedbackQuery = _feedbackRepository.GetAllIncluding(f => f.Course);
            var courseQuery = _courseRepository.GetAll();

            if (isStudent)
            {
                feedbackQuery = feedbackQuery.Where(f => f.StudentName == currentUser.FullName);
                courseQuery = courseQuery.Where(c => c.Feedbacks.Any(f => f.StudentName == currentUser.FullName));
            }
            else if (isTeacher)
            {
                feedbackQuery = feedbackQuery.Where(f => f.Course.InstructorName == currentUser.FullName);
                courseQuery = courseQuery.Where(c => c.InstructorName == currentUser.FullName);
            }
            // Admin sees everything

            var totalFeedbackCount = await feedbackQuery.CountAsync();
            var totalCourseCount = await courseQuery.CountAsync();
            var averageRating = totalFeedbackCount > 0
                ? await feedbackQuery.AverageAsync(f => (double)f.Rating)
                : (double?)null;

            var topCourses = await feedbackQuery
                .GroupBy(f => new { f.CourseId, f.Course.CourseName })
                .Select(g => new TopCourseDto
                {
                    CourseName = g.Key.CourseName,
                    AverageRating = g.Average(f => f.Rating),
                    FeedbackCount = g.Count()
                })
                .OrderByDescending(x => x.AverageRating)
                .Take(5)
                .ToListAsync();

            var recentFeedbackEntities = await feedbackQuery
                .OrderByDescending(f => f.CreatedDate)
                .Take(5)
                .ToListAsync();

            var recentFeedbacks = recentFeedbackEntities.Select(f => new RecentFeedbackDto
            {
                StudentName = f.StudentName,
                CourseName = f.Course?.CourseName,
                Rating = f.Rating,
                CreatedDate = f.CreatedDate
            }).ToList();

            string userRole = isAdmin ? "Admin" : isTeacher ? "Teacher" : "Student";

            return new DashboardDto
            {
                TotalFeedbackCount = totalFeedbackCount,
                TotalCourseCount = totalCourseCount,
                AverageRating = averageRating,
                UserRole = userRole,
                TopCoursesByRating = topCourses,
                RecentFeedbacks = recentFeedbacks
            };
        }
    }
}
