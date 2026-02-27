using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using CourseFeedbackMS.Authorization;
using CourseFeedbackMS.Authorization.Roles;
using CourseFeedbackMS.Authorization.Users;
using CourseFeedbackMS.CourseFeedback.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CourseFeedbackMS.CourseFeedback
{
    [AbpAuthorize(PermissionNames.Pages_Courses)]
    public class CourseAppService : ApplicationService, ICourseAppService
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly UserManager _userManager;

        public CourseAppService(
            IRepository<Course> courseRepository,
            UserManager userManager)
        {
            _courseRepository = courseRepository;
            _userManager = userManager;
        }

        public async Task<PagedResultDto<CourseDto>> GetAllAsync(GetCoursesInput input)
        {
            var currentUser = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());
            var roles = await _userManager.GetRolesAsync(currentUser);
            bool isTeacher = roles.Contains(StaticRoleNames.Tenants.Teacher);

            var query = _courseRepository
                .GetAllIncluding(c => c.Feedbacks)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    c => c.CourseName.Contains(input.Filter) || c.InstructorName.Contains(input.Filter))
                .WhereIf(input.IsActive.HasValue, c => c.IsActive == input.IsActive.Value);

            // Teachers see only courses they instruct
            if (isTeacher)
            {
                query = query.Where(c => c.InstructorName == currentUser.FullName);
            }
            // Admin and Student see all courses

            var totalCount = await query.CountAsync();

            var courses = await query
                .OrderBy(string.IsNullOrEmpty(input.Sorting) ? "CourseName" : input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<CourseDto>(totalCount, ObjectMapper.Map<List<CourseDto>>(courses));
        }

        public async Task<CourseDto> GetAsync(EntityDto input)
        {
            var course = await _courseRepository
                .GetAllIncluding(c => c.Feedbacks)
                .FirstOrDefaultAsync(c => c.Id == input.Id);

            return ObjectMapper.Map<CourseDto>(course);
        }

        [AbpAuthorize(PermissionNames.Pages_Courses_Create)]
        public async Task<CourseDto> CreateAsync(CreateCourseDto input)
        {
            var course = ObjectMapper.Map<Course>(input);
            await _courseRepository.InsertAsync(course);
            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<CourseDto>(course);
        }

        [AbpAuthorize(PermissionNames.Pages_Courses_Edit)]
        public async Task<CourseDto> UpdateAsync(UpdateCourseDto input)
        {
            var currentUser = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());
            var roles = await _userManager.GetRolesAsync(currentUser);
            bool isTeacher = roles.Contains(StaticRoleNames.Tenants.Teacher);

            var course = await _courseRepository.GetAsync(input.Id);

            // Teachers can only edit their own courses
            if (isTeacher && course.InstructorName != currentUser.FullName)
            {
                throw new UserFriendlyException("You can only edit your own courses.");
            }

            ObjectMapper.Map(input, course);
            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<CourseDto>(course);
        }

        [AbpAuthorize(PermissionNames.Pages_Courses_Delete)]
        public async Task DeleteAsync(EntityDto input)
        {
            await _courseRepository.DeleteAsync(input.Id);
        }

        public async Task<ListResultDto<CourseDto>> GetActiveCoursesAsync()
        {
            var courses = await _courseRepository
                .GetAll()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            return new ListResultDto<CourseDto>(ObjectMapper.Map<List<CourseDto>>(courses));
        }
    }
}
