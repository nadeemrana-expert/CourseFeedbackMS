# Course Feedback Management System
## Complete Step-by-Step Build Guide
### Stack: ASP.NET Core (ABP) + Angular + SQL Server + Hangfire

---

> **Author Note:** This guide is written from the perspective of a senior .NET + Angular developer walking you through every layer — from project scaffolding to deployment-ready configuration. Follow each phase sequentially. Do not skip steps.

---

## Table of Contents

1. [Prerequisites & Environment Setup](#phase-0-prerequisites--environment-setup)
2. [Project Scaffolding (ABP Template)](#phase-1-project-scaffolding)
3. [Database & Connection Configuration](#phase-2-database--connection-configuration)
4. [Backend: Domain Entities](#phase-3-backend-domain-entities)
5. [Backend: DTOs](#phase-4-backend-dtos)
6. [Backend: Permissions](#phase-5-backend-permissions)
7. [Backend: Application Services](#phase-6-backend-application-services)
8. [Backend: Controllers](#phase-7-backend-controllers)
9. [Backend: Tenant Settings](#phase-8-backend-tenant-settings)
10. [Backend: Hangfire Background Job](#phase-9-backend-hangfire-background-job)
11. [Backend: File Upload Service](#phase-10-backend-file-upload-service)
12. [Database Migration](#phase-11-database-migration)
13. [Frontend: Angular Module Setup](#phase-12-frontend-angular-module-setup)
14. [Frontend: Services (API Proxies)](#phase-13-frontend-services-api-proxies)
15. [Frontend: Course Module (List + CRUD)](#phase-14-frontend-course-module)
16. [Frontend: Feedback Module (List + CRUD + File Upload)](#phase-15-frontend-feedback-module)
17. [Frontend: Dashboard Card](#phase-16-frontend-dashboard-card)
18. [Frontend: Routing & Navigation](#phase-17-frontend-routing--navigation)
19. [Testing & Validation Checklist](#phase-18-testing--validation-checklist)
20. [README Template](#phase-19-readme-template)

---

## PHASE 0: Prerequisites & Environment Setup

### Required Tools
Install the following before starting:

```
- .NET 6 SDK           → https://dotnet.microsoft.com/download/dotnet/6.0
- Node.js v16+         → https://nodejs.org
- Angular CLI v15+     → npm install -g @angular/cli@15
- SQL Server 2019+     → LocalDB is fine for dev
- Visual Studio 2022   → Community Edition works
- VS Code              → For Angular frontend
- Git                  → For version control
```

### Verify Installations
```bash
dotnet --version          # Should show 6.x.x
node --version            # Should show 16.x or 18.x
ng version                # Should show Angular CLI 15+
sqlcmd -?                 # SQL Server CLI available
```

---

## PHASE 1: Project Scaffolding

### Step 1.1 — Download ABP Template

Go to: https://aspnetboilerplate.com/Templates

Select the following options:
- **Framework:** ASP.NET Core 6.0
- **UI:** Angular
- **Multi-tenancy:** Enabled
- **Module Zero:** Included
- Click **"Create my project"**

Download the ZIP, extract to: `C:\Projects\CourseFeedback\`

### Step 1.2 — Understand the Solution Structure

After extraction you will see:
```
CourseFeedback/
├── aspnet-core/
│   ├── src/
│   │   ├── CourseFeedback.Core/           ← Domain entities, permissions, settings
│   │   ├── CourseFeedback.Application/    ← App services, DTOs
│   │   ├── CourseFeedback.EntityFrameworkCore/  ← DbContext, migrations
│   │   ├── CourseFeedback.Web.Host/       ← ASP.NET Core startup, controllers
│   └── test/
├── angular/
│   ├── src/
│   │   ├── app/
│   │   │   ├── shared/
│   │   │   ├── admin/
│   │   │   └── app.module.ts
│   └── package.json
```

### Step 1.3 — Install NuGet & NPM Packages

**Backend:**
```bash
cd aspnet-core
dotnet restore
```

Install Hangfire in `CourseFeedback.Web.Host`:
```bash
cd src/CourseFeedback.Web.Host
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.SqlServer
```

**Frontend:**
```bash
cd angular
npm install
npm install primeng primeicons           # PrimeNG UI components
npm install primeflex                    # PrimeNG layout utilities
```

---

## PHASE 2: Database & Connection Configuration

### Step 2.1 — Update appsettings.json

Open: `aspnet-core/src/CourseFeedback.Web.Host/appsettings.json`

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=CourseFeedbackDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "App": {
    "ServerRootAddress": "https://localhost:44301/",
    "ClientRootAddress": "http://localhost:4200/",
    "CorsOrigins": "http://localhost:4200"
  }
}
```

> **Note:** Adjust the SQL Server instance name if you are using a named instance or LocalDB. For LocalDB use: `Server=(localdb)\\mssqllocaldb;Database=CourseFeedbackDb;Trusted_Connection=True`

---

## PHASE 3: Backend — Domain Entities

All entity files go in: `CourseFeedback.Core/CourseFeedback/`

### Step 3.1 — Create the Course Entity

Create file: `CourseFeedback.Core/CourseFeedback/Course.cs`

```csharp
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseFeedback.CourseFeedback
{
    public class Course : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstructorName { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
```

### Step 3.2 — Create the Feedback Entity

Create file: `CourseFeedback.Core/CourseFeedback/Feedback.cs`

```csharp
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseFeedback.CourseFeedback
{
    public class Feedback : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string StudentName { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [MaxLength(2000)]
        public string Comment { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // File upload
        [MaxLength(500)]
        public string AttachmentPath { get; set; }

        [MaxLength(200)]
        public string AttachmentFileName { get; set; }
    }
}
```

### Step 3.3 — Register Entities in DbContext

Open: `CourseFeedback.EntityFrameworkCore/EntityFrameworkCore/CourseFeedbackDbContext.cs`

Add the following DbSet properties:

```csharp
// Add inside the class body, after existing DbSet declarations:
public DbSet<CourseFeedback.Course> Courses { get; set; }
public DbSet<CourseFeedback.Feedback> Feedbacks { get; set; }
```

Also add the `using` at the top:
```csharp
using CourseFeedback.CourseFeedback;
```

### Step 3.4 — Configure Entity Relationships in OnModelCreating

Still in `CourseFeedbackDbContext.cs`, inside `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Course>(b =>
    {
        b.ToTable("Courses");
        b.HasIndex(e => e.TenantId);
        b.Property(e => e.CourseName).IsRequired().HasMaxLength(200);
        b.Property(e => e.InstructorName).IsRequired().HasMaxLength(200);
    });

    modelBuilder.Entity<Feedback>(b =>
    {
        b.ToTable("Feedbacks");
        b.HasIndex(e => e.TenantId);
        b.HasIndex(e => e.CourseId);
        b.HasOne(e => e.Course)
         .WithMany(c => c.Feedbacks)
         .HasForeignKey(e => e.CourseId)
         .OnDelete(DeleteBehavior.Restrict);
        b.Property(e => e.StudentName).IsRequired().HasMaxLength(200);
        b.Property(e => e.Comment).HasMaxLength(2000);
    });
}
```

---

## PHASE 4: Backend — DTOs

Create folder: `CourseFeedback.Application/CourseFeedback/Dto/`

### Step 4.1 — Course DTOs

**`CourseDto.cs`**
```csharp
using Abp.Application.Services.Dto;
using System;

namespace CourseFeedback.CourseFeedback.Dto
{
    public class CourseDto : EntityDto
    {
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationTime { get; set; }
        public int FeedbackCount { get; set; }
        public double? AverageRating { get; set; }
    }
}
```

**`CreateCourseDto.cs`**
```csharp
using System.ComponentModel.DataAnnotations;

namespace CourseFeedback.CourseFeedback.Dto
{
    public class CreateCourseDto
    {
        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstructorName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
```

**`UpdateCourseDto.cs`**
```csharp
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace CourseFeedback.CourseFeedback.Dto
{
    public class UpdateCourseDto : EntityDto
    {
        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstructorName { get; set; }

        public bool IsActive { get; set; }
    }
}
```

**`GetCoursesInput.cs`**
```csharp
using Abp.Application.Services.Dto;

namespace CourseFeedback.CourseFeedback.Dto
{
    public class GetCoursesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public bool? IsActive { get; set; }
    }
}
```

### Step 4.2 — Feedback DTOs

**`FeedbackDto.cs`**
```csharp
using Abp.Application.Services.Dto;
using System;

namespace CourseFeedback.CourseFeedback.Dto
{
    public class FeedbackDto : EntityDto
    {
        public string StudentName { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AttachmentPath { get; set; }
        public string AttachmentFileName { get; set; }
    }
}
```

**`CreateFeedbackDto.cs`**
```csharp
using System.ComponentModel.DataAnnotations;

namespace CourseFeedback.CourseFeedback.Dto
{
    public class CreateFeedbackDto
    {
        [Required]
        [MaxLength(200)]
        public string StudentName { get; set; }

        [Required]
        public int CourseId { get; set; }

        [MaxLength(2000)]
        public string Comment { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string AttachmentPath { get; set; }
        public string AttachmentFileName { get; set; }
    }
}
```

**`UpdateFeedbackDto.cs`**
```csharp
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace CourseFeedback.CourseFeedback.Dto
{
    public class UpdateFeedbackDto : EntityDto
    {
        [Required]
        [MaxLength(200)]
        public string StudentName { get; set; }

        [Required]
        public int CourseId { get; set; }

        [MaxLength(2000)]
        public string Comment { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string AttachmentPath { get; set; }
        public string AttachmentFileName { get; set; }
    }
}
```

**`GetFeedbacksInput.cs`**
```csharp
using Abp.Application.Services.Dto;

namespace CourseFeedback.CourseFeedback.Dto
{
    public class GetFeedbacksInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public int? CourseId { get; set; }
        public int? Rating { get; set; }
    }
}
```

**`DashboardDto.cs`**
```csharp
using System.Collections.Generic;

namespace CourseFeedback.CourseFeedback.Dto
{
    public class DashboardDto
    {
        public int TotalFeedbackCount { get; set; }
        public List<TopCourseDto> TopCoursesByRating { get; set; }
    }

    public class TopCourseDto
    {
        public string CourseName { get; set; }
        public double AverageRating { get; set; }
        public int FeedbackCount { get; set; }
    }
}
```

### Step 4.3 — Configure AutoMapper

Open: `CourseFeedback.Application/CourseFeedbackApplicationModule.cs`

Add to `PreInitialize()`:
```csharp
Configuration.Modules.AbpAutoMapper().Configurators.Add(config =>
{
    config.CreateMap<Course, CourseDto>()
          .ForMember(dest => dest.FeedbackCount,
                     opt => opt.MapFrom(src => src.Feedbacks != null ? src.Feedbacks.Count : 0))
          .ForMember(dest => dest.AverageRating,
                     opt => opt.MapFrom(src => src.Feedbacks != null && src.Feedbacks.Any()
                                ? src.Feedbacks.Average(f => f.Rating)
                                : (double?)null));

    config.CreateMap<CreateCourseDto, Course>();
    config.CreateMap<UpdateCourseDto, Course>();

    config.CreateMap<Feedback, FeedbackDto>()
          .ForMember(dest => dest.CourseName,
                     opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseName : ""));

    config.CreateMap<CreateFeedbackDto, Feedback>();
    config.CreateMap<UpdateFeedbackDto, Feedback>();
});
```

---

## PHASE 5: Backend — Permissions

### Step 5.1 — Define Permission Names

Open or create: `CourseFeedback.Core/Authorization/PermissionNames.cs`

Add the new constants:
```csharp
public static class PermissionNames
{
    // Existing permissions...
    public const string Pages_Tenants = "Pages.Tenants";
    public const string Pages_Users = "Pages.Users";
    public const string Pages_Roles = "Pages.Roles";

    // NEW PERMISSIONS:
    public const string Pages_Courses = "Pages.Courses";
    public const string Pages_Feedbacks = "Pages.Feedbacks";
}
```

### Step 5.2 — Register Permissions

Open: `CourseFeedback.Core/Authorization/CourseFeedbackAuthorizationProvider.cs`

Inside `SetPermissions()`:
```csharp
public override void SetPermissions(IPermissionDefinitionContext context)
{
    // Existing permissions...
    var pages = context.GetPermissionOrNull(PermissionNames.Pages_Users) != null
        ? context.GetPermissionOrNull("Pages")
        : context.CreatePermission("Pages", L("Pages"));

    // Add new permissions under the "Pages" group:
    pages.CreateChildPermission(
        PermissionNames.Pages_Courses,
        L("Courses"),
        multiTenancySides: MultiTenancySides.Tenant
    );

    pages.CreateChildPermission(
        PermissionNames.Pages_Feedbacks,
        L("Feedbacks"),
        multiTenancySides: MultiTenancySides.Tenant
    );
}
```

### Step 5.3 — Add Localization Strings

Open: `CourseFeedback.Core/Localization/CourseFeedback/CourseFeedback-en.xml`

Add inside `<texts>`:
```xml
<text name="Courses" value="Courses" />
<text name="Feedbacks" value="Feedbacks" />
<text name="CourseName" value="Course Name" />
<text name="InstructorName" value="Instructor Name" />
<text name="IsActive" value="Is Active" />
<text name="StudentName" value="Student Name" />
<text name="Rating" value="Rating" />
<text name="Comment" value="Comment" />
<text name="CreatedDate" value="Created Date" />
<text name="Attachment" value="Attachment" />
<text name="MaxFeedbackPerCourse" value="Max Feedback Per Course" />
```

---

## PHASE 6: Backend — Application Services

Create folder: `CourseFeedback.Application/CourseFeedback/`

### Step 6.1 — Course Application Service Interface

**`ICourseAppService.cs`**
```csharp
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CourseFeedback.CourseFeedback.Dto;
using System.Threading.Tasks;

namespace CourseFeedback.CourseFeedback
{
    public interface ICourseAppService : IApplicationService
    {
        Task<PagedResultDto<CourseDto>> GetAllAsync(GetCoursesInput input);
        Task<CourseDto> GetAsync(EntityDto input);
        Task<CourseDto> CreateAsync(CreateCourseDto input);
        Task<CourseDto> UpdateAsync(UpdateCourseDto input);
        Task DeleteAsync(EntityDto input);
        Task<ListResultDto<CourseDto>> GetActiveCoursesAsync();
    }
}
```

### Step 6.2 — Course Application Service Implementation

**`CourseAppService.cs`**
```csharp
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using AutoMapper;
using CourseFeedback.Authorization;
using CourseFeedback.CourseFeedback.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CourseFeedback.CourseFeedback
{
    [AbpAuthorize(PermissionNames.Pages_Courses)]
    public class CourseAppService : ApplicationService, ICourseAppService
    {
        private readonly IRepository<Course> _courseRepository;

        public CourseAppService(IRepository<Course> courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<PagedResultDto<CourseDto>> GetAllAsync(GetCoursesInput input)
        {
            var query = _courseRepository
                .GetAllIncluding(c => c.Feedbacks)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    c => c.CourseName.Contains(input.Filter) ||
                         c.InstructorName.Contains(input.Filter))
                .WhereIf(input.IsActive.HasValue,
                    c => c.IsActive == input.IsActive.Value);

            var totalCount = await query.CountAsync();

            var courses = await query
                .OrderBy(string.IsNullOrEmpty(input.Sorting) ? "CourseName" : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
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

        public async Task<CourseDto> CreateAsync(CreateCourseDto input)
        {
            var course = ObjectMapper.Map<Course>(input);
            await _courseRepository.InsertAsync(course);
            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<CourseDto>(course);
        }

        public async Task<CourseDto> UpdateAsync(UpdateCourseDto input)
        {
            var course = await _courseRepository.GetAsync(input.Id);
            ObjectMapper.Map(input, course);
            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<CourseDto>(course);
        }

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
```

### Step 6.3 — Feedback Application Service Interface

**`IFeedbackAppService.cs`**
```csharp
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CourseFeedback.CourseFeedback.Dto;
using System.Threading.Tasks;

namespace CourseFeedback.CourseFeedback
{
    public interface IFeedbackAppService : IApplicationService
    {
        Task<PagedResultDto<FeedbackDto>> GetAllAsync(GetFeedbacksInput input);
        Task<FeedbackDto> GetAsync(EntityDto input);
        Task<FeedbackDto> CreateAsync(CreateFeedbackDto input);
        Task<FeedbackDto> UpdateAsync(UpdateFeedbackDto input);
        Task DeleteAsync(EntityDto input);
        Task<DashboardDto> GetDashboardDataAsync();
    }
}
```

### Step 6.4 — Feedback Application Service Implementation

**`FeedbackAppService.cs`**
```csharp
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using CourseFeedback.Authorization;
using CourseFeedback.CourseFeedback.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CourseFeedback.CourseFeedback
{
    [AbpAuthorize(PermissionNames.Pages_Feedbacks)]
    public class FeedbackAppService : ApplicationService, IFeedbackAppService
    {
        private readonly IRepository<Feedback> _feedbackRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly ISettingManager _settingManager;

        public FeedbackAppService(
            IRepository<Feedback> feedbackRepository,
            IRepository<Course> courseRepository,
            ISettingManager settingManager)
        {
            _feedbackRepository = feedbackRepository;
            _courseRepository = courseRepository;
            _settingManager = settingManager;
        }

        public async Task<PagedResultDto<FeedbackDto>> GetAllAsync(GetFeedbacksInput input)
        {
            var query = _feedbackRepository
                .GetAllIncluding(f => f.Course)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    f => f.StudentName.Contains(input.Filter) ||
                         f.Comment.Contains(input.Filter))
                .WhereIf(input.CourseId.HasValue, f => f.CourseId == input.CourseId.Value)
                .WhereIf(input.Rating.HasValue, f => f.Rating == input.Rating.Value);

            var totalCount = await query.CountAsync();

            var feedbacks = await query
                .OrderBy(string.IsNullOrEmpty(input.Sorting) ? "CreatedDate desc" : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
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

        public async Task<FeedbackDto> CreateAsync(CreateFeedbackDto input)
        {
            // Tenant setting check: max feedbacks per course
            var maxFeedbackStr = await _settingManager.GetSettingValueAsync(
                AppSettingNames.MaxFeedbackPerCourse);

            if (int.TryParse(maxFeedbackStr, out int maxFeedback) && maxFeedback > 0)
            {
                var existingCount = await _feedbackRepository
                    .GetAll()
                    .CountAsync(f => f.CourseId == input.CourseId);

                if (existingCount >= maxFeedback)
                {
                    throw new UserFriendlyException(
                        $"This course has reached the maximum feedback limit of {maxFeedback}.");
                }
            }

            var feedback = ObjectMapper.Map<Feedback>(input);
            feedback.CreatedDate = DateTime.UtcNow;
            await _feedbackRepository.InsertAsync(feedback);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(new EntityDto(feedback.Id));
        }

        public async Task<FeedbackDto> UpdateAsync(UpdateFeedbackDto input)
        {
            var feedback = await _feedbackRepository.GetAsync(input.Id);
            ObjectMapper.Map(input, feedback);
            await CurrentUnitOfWork.SaveChangesAsync();
            return await GetAsync(new EntityDto(feedback.Id));
        }

        public async Task DeleteAsync(EntityDto input)
        {
            await _feedbackRepository.DeleteAsync(input.Id);
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var totalCount = await _feedbackRepository.CountAsync();

            var topCourses = await _feedbackRepository
                .GetAllIncluding(f => f.Course)
                .GroupBy(f => new { f.CourseId, f.Course.CourseName })
                .Select(g => new TopCourseDto
                {
                    CourseName = g.Key.CourseName,
                    AverageRating = g.Average(f => f.Rating),
                    FeedbackCount = g.Count()
                })
                .OrderByDescending(c => c.AverageRating)
                .Take(5)
                .ToListAsync();

            return new DashboardDto
            {
                TotalFeedbackCount = totalCount,
                TopCoursesByRating = topCourses
            };
        }
    }
}
```

---

## PHASE 7: Backend — Controllers

Create folder: `CourseFeedback.Web.Host/Controllers/`

### Step 7.1 — File Upload Controller

**`FileUploadController.cs`**
```csharp
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CourseFeedback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : AbpController
    {
        private readonly IWebHostEnvironment _environment;

        public FileUploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("upload-feedback-attachment")]
        public async Task<IActionResult> UploadFeedbackAttachment(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Array.Exists(allowedExtensions, e => e == extension))
                return BadRequest("Invalid file type. Only .pdf, .jpg, and .png are allowed.");

            // Max 10MB
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("File size cannot exceed 10MB.");

            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "feedbacks");
            Directory.CreateDirectory(uploadFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new
            {
                filePath = $"/uploads/feedbacks/{uniqueFileName}",
                fileName = file.FileName
            });
        }
    }
}
```

### Step 7.2 — Register Static File Serving

In `Startup.cs` or `Program.cs`, ensure static files are served:
```csharp
app.UseStaticFiles(); // This must be present
```

---

## PHASE 8: Backend — Tenant Settings

### Step 8.1 — Define Setting Names

Create: `CourseFeedback.Core/Configuration/AppSettingNames.cs`

```csharp
namespace CourseFeedback.Configuration
{
    public class AppSettingNames
    {
        public const string MaxFeedbackPerCourse = "App.Feedback.MaxFeedbackPerCourse";
    }
}
```

### Step 8.2 — Register the Setting in the Setting Provider

Open: `CourseFeedback.Core/Configuration/AppSettingProvider.cs`

```csharp
using Abp.Configuration;
using System.Collections.Generic;

namespace CourseFeedback.Configuration
{
    public class AppSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(
            SettingDefinitionProviderContext context)
        {
            return new[]
            {
                new SettingDefinition(
                    AppSettingNames.MaxFeedbackPerCourse,
                    "10",                                   // default value
                    scopes: SettingScopes.Application | SettingScopes.Tenant,
                    isVisibleToClients: true
                )
            };
        }
    }
}
```

### Step 8.3 — Register Provider in Core Module

Open: `CourseFeedback.Core/CourseFeedbackCoreModule.cs`

```csharp
Configuration.Settings.Providers.Add<AppSettingProvider>();
```

---

## PHASE 9: Backend — Hangfire Background Job

### Step 9.1 — Configure Hangfire in Startup

Open `Startup.cs`:

```csharp
// In ConfigureServices:
services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(Configuration.GetConnectionString("Default"),
              new SqlServerStorageOptions
              {
                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                  QueuePollInterval = TimeSpan.Zero,
                  UseRecommendedIsolationLevel = true,
                  DisableGlobalLocks = true
              }));

services.AddHangfireServer();

// In Configure:
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Restrict to admin only in production
});
```

### Step 9.2 — Create the Background Job

Create: `CourseFeedback.Application/CourseFeedback/Jobs/FeedbackCheckJob.cs`

```csharp
using Abp.Domain.Repositories;
using Abp.Logging;
using CourseFeedback.CourseFeedback;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CourseFeedback.CourseFeedback.Jobs
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
                    .AnyAsync(f => f.CourseId == course.Id &&
                                   f.CreatedDate >= tenDaysAgo);

                if (!hasRecentFeedback)
                {
                    LogHelper.Logger.Warn(
                        $"No feedback received for Course [{course.CourseName}] in last 10 days.");

                    // Optional: send email to admin
                    // await _emailSender.SendAsync(adminEmail, subject, body);
                }
            }
        }
    }
}
```

### Step 9.3 — Schedule the Job on Startup

In `Startup.cs` Configure method (after Hangfire middleware):

```csharp
// Schedule the daily check — runs every day at 8 AM
RecurringJob.AddOrUpdate<FeedbackCheckJob>(
    "feedback-daily-check",
    job => job.CheckCoursesForInactiveFeedback(),
    Cron.Daily(8, 0)
);
```

---

## PHASE 10: Backend — File Upload Service (Serving Files)

Add to `Startup.cs` Configure method to serve uploaded files:
```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});
```

Also create the folder structure manually or add a startup check:
```csharp
// In Program.cs or startup:
var uploadPath = Path.Combine(builder.Environment.WebRootPath, "uploads", "feedbacks");
Directory.CreateDirectory(uploadPath);
```

---

## PHASE 11: Database Migration

### Step 11.1 — Add Migration

Open Package Manager Console in Visual Studio, set default project to `CourseFeedback.EntityFrameworkCore`:

```bash
Add-Migration "AddCourseFeedbackEntities"
Update-Database
```

OR via CLI:
```bash
cd aspnet-core/src/CourseFeedback.EntityFrameworkCore
dotnet ef migrations add "AddCourseFeedbackEntities" --startup-project ../CourseFeedback.Web.Host
dotnet ef database update --startup-project ../CourseFeedback.Web.Host
```

### Step 11.2 — Seed Initial Data (Optional)

Create: `CourseFeedback.EntityFrameworkCore/EntityFrameworkCore/Seed/CourseFeedbackDataSeeder.cs`

```csharp
// Seed 2-3 sample courses for testing
if (!context.Courses.Any())
{
    context.Courses.AddRange(
        new Course { TenantId = 1, CourseName = "Angular Fundamentals", InstructorName = "John Doe", IsActive = true },
        new Course { TenantId = 1, CourseName = "ASP.NET Core Deep Dive", InstructorName = "Jane Smith", IsActive = true },
        new Course { TenantId = 1, CourseName = "SQL Server Essentials", InstructorName = "Bob Lee", IsActive = true }
    );
    context.SaveChanges();
}
```

### Step 11.3 — Verify Backend Runs

```bash
cd aspnet-core/src/CourseFeedback.Web.Host
dotnet run
```

Visit: `https://localhost:44301/swagger` to confirm all APIs are available.

---

## PHASE 12: Frontend — Angular Module Setup

### Step 12.1 — Install PrimeNG

```bash
cd angular
npm install primeng primeicons primeflex
```

### Step 12.2 — Register PrimeNG in app.module.ts

Open `angular/src/app/app.module.ts` and add:

```typescript
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { RatingModule } from 'primeng/rating';
import { FileUploadModule } from 'primeng/fileupload';
import { CardModule } from 'primeng/card';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Add all above to imports array and ConfirmationService, MessageService to providers
```

### Step 12.3 — Add PrimeNG styles to angular.json

In `angular.json` under `styles`:
```json
"styles": [
  "node_modules/primeng/resources/themes/lara-light-blue/theme.css",
  "node_modules/primeng/resources/primeng.min.css",
  "node_modules/primeicons/primeicons.css",
  "node_modules/primeflex/primeflex.css",
  "src/styles.css"
]
```

### Step 12.4 — Create the CourseFeedback Feature Module

```bash
cd angular/src/app
ng generate module course-feedback --routing
ng generate component course-feedback/courses
ng generate component course-feedback/feedbacks
ng generate component course-feedback/dashboard
```

---

## PHASE 13: Frontend — Services (API Proxies)

Create: `angular/src/app/course-feedback/`

### Step 13.1 — Course Service

**`course.service.ts`**
```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';

export interface CourseDto {
  id: number;
  courseName: string;
  instructorName: string;
  isActive: boolean;
  creationTime: Date;
  feedbackCount: number;
  averageRating?: number;
}

export interface PagedResult<T> {
  totalCount: number;
  items: T[];
}

@Injectable({ providedIn: 'root' })
export class CourseService {
  private readonly baseUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app/Course`;

  constructor(private http: HttpClient) {}

  getAll(filter?: string, isActive?: boolean, skipCount = 0, maxResultCount = 10, sorting = 'CourseName'): Observable<{ result: PagedResult<CourseDto> }> {
    let params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString())
      .set('Sorting', sorting);

    if (filter) params = params.set('Filter', filter);
    if (isActive !== undefined) params = params.set('IsActive', isActive.toString());

    return this.http.get<any>(`${this.baseUrl}/GetAll`, { params });
  }

  get(id: number): Observable<{ result: CourseDto }> {
    return this.http.get<any>(`${this.baseUrl}/Get`, { params: { Id: id.toString() } });
  }

  create(input: Partial<CourseDto>): Observable<{ result: CourseDto }> {
    return this.http.post<any>(`${this.baseUrl}/Create`, input);
  }

  update(input: CourseDto): Observable<{ result: CourseDto }> {
    return this.http.put<any>(`${this.baseUrl}/Update`, input);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Delete`, { params: { Id: id.toString() } });
  }

  getActiveCourses(): Observable<{ result: { items: CourseDto[] } }> {
    return this.http.get<any>(`${this.baseUrl}/GetActiveCourses`);
  }
}
```

### Step 13.2 — Feedback Service

**`feedback.service.ts`**
```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';

export interface FeedbackDto {
  id: number;
  studentName: string;
  courseId: number;
  courseName: string;
  comment: string;
  rating: number;
  createdDate: Date;
  attachmentPath?: string;
  attachmentFileName?: string;
}

export interface DashboardDto {
  totalFeedbackCount: number;
  topCoursesByRating: { courseName: string; averageRating: number; feedbackCount: number }[];
}

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private readonly baseUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app/Feedback`;

  constructor(private http: HttpClient) {}

  getAll(filter?: string, courseId?: number, rating?: number, skipCount = 0, maxResultCount = 10): Observable<any> {
    let params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString());

    if (filter) params = params.set('Filter', filter);
    if (courseId) params = params.set('CourseId', courseId.toString());
    if (rating) params = params.set('Rating', rating.toString());

    return this.http.get<any>(`${this.baseUrl}/GetAll`, { params });
  }

  create(input: Partial<FeedbackDto>): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Create`, input);
  }

  update(input: FeedbackDto): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/Update`, input);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Delete`, { params: { Id: id.toString() } });
  }

  getDashboardData(): Observable<{ result: DashboardDto }> {
    return this.http.get<any>(`${this.baseUrl}/GetDashboardData`);
  }

  uploadAttachment(file: File): Observable<{ filePath: string; fileName: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<any>(
      `${AppConsts.remoteServiceBaseUrl}/api/FileUpload/upload-feedback-attachment`,
      formData
    );
  }
}
```

---

## PHASE 14: Frontend — Course Module

### Step 14.1 — Courses Component Template

**`courses.component.html`**
```html
<div class="card">
  <div class="flex justify-content-between align-items-center mb-4">
    <h2>Courses</h2>
    <button pButton label="Add Course" icon="pi pi-plus"
            (click)="openCreateModal()" class="p-button-success"></button>
  </div>

  <!-- Search & Filter Bar -->
  <div class="flex gap-2 mb-4">
    <span class="p-input-icon-left">
      <i class="pi pi-search"></i>
      <input pInputText type="text" [(ngModel)]="filterText"
             placeholder="Search courses..." (input)="onSearch()"/>
    </span>
    <p-dropdown [options]="activeFilterOptions" [(ngModel)]="isActiveFilter"
                placeholder="All" (onChange)="onSearch()" [showClear]="true"></p-dropdown>
  </div>

  <!-- Data Table -->
  <p-table [value]="courses" [lazy]="true" (onLazyLoad)="loadCourses($event)"
           [totalRecords]="totalCount" [rows]="pageSize" [paginator]="true"
           [sortField]="sortField" [sortOrder]="sortOrder"
           [loading]="loading" dataKey="id"
           styleClass="p-datatable-gridlines p-datatable-striped">
    <ng-template pTemplate="header">
      <tr>
        <th pSortableColumn="CourseName">Course Name <p-sortIcon field="CourseName"></p-sortIcon></th>
        <th pSortableColumn="InstructorName">Instructor <p-sortIcon field="InstructorName"></p-sortIcon></th>
        <th>Status</th>
        <th>Feedbacks</th>
        <th>Avg Rating</th>
        <th>Actions</th>
      </tr>
    </ng-template>
    <ng-template pTemplate="body" let-course>
      <tr>
        <td>{{ course.courseName }}</td>
        <td>{{ course.instructorName }}</td>
        <td>
          <span [class]="course.isActive ? 'badge badge-success' : 'badge badge-danger'">
            {{ course.isActive ? 'Active' : 'Inactive' }}
          </span>
        </td>
        <td>{{ course.feedbackCount }}</td>
        <td>{{ course.averageRating ? (course.averageRating | number:'1.1-1') : 'N/A' }}</td>
        <td>
          <button pButton icon="pi pi-pencil" class="p-button-rounded p-button-info mr-2"
                  (click)="openEditModal(course)"></button>
          <button pButton icon="pi pi-trash" class="p-button-rounded p-button-danger"
                  (click)="deleteCourse(course.id)"></button>
        </td>
      </tr>
    </ng-template>
    <ng-template pTemplate="emptymessage">
      <tr><td colspan="6" class="text-center">No courses found.</td></tr>
    </ng-template>
  </p-table>
</div>

<!-- Create/Edit Modal -->
<p-dialog [(visible)]="showModal" [header]="isEdit ? 'Edit Course' : 'Create Course'"
          [modal]="true" [style]="{width: '500px'}" [draggable]="false">
  <form [formGroup]="courseForm">
    <div class="field mt-3">
      <label for="courseName">Course Name *</label>
      <input id="courseName" pInputText formControlName="courseName"
             class="w-full" placeholder="Enter course name"/>
      <small class="p-error" *ngIf="courseForm.get('courseName')?.invalid && courseForm.get('courseName')?.touched">
        Course name is required.
      </small>
    </div>
    <div class="field mt-3">
      <label for="instructorName">Instructor Name *</label>
      <input id="instructorName" pInputText formControlName="instructorName"
             class="w-full" placeholder="Enter instructor name"/>
      <small class="p-error" *ngIf="courseForm.get('instructorName')?.invalid && courseForm.get('instructorName')?.touched">
        Instructor name is required.
      </small>
    </div>
    <div class="field mt-3 flex align-items-center gap-2">
      <p-checkbox formControlName="isActive" [binary]="true" inputId="isActive"></p-checkbox>
      <label for="isActive">Active</label>
    </div>
  </form>
  <ng-template pTemplate="footer">
    <button pButton label="Cancel" icon="pi pi-times"
            (click)="showModal = false" class="p-button-text"></button>
    <button pButton label="Save" icon="pi pi-check"
            (click)="saveCourse()" [disabled]="courseForm.invalid"></button>
  </ng-template>
</p-dialog>

<p-confirmDialog></p-confirmDialog>
<p-toast></p-toast>
```

### Step 14.2 — Courses Component TypeScript

**`courses.component.ts`**
```typescript
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CourseService, CourseDto } from '../services/course.service';

@Component({
  selector: 'app-courses',
  templateUrl: './courses.component.html'
})
export class CoursesComponent implements OnInit {
  courses: CourseDto[] = [];
  totalCount = 0;
  pageSize = 10;
  loading = false;
  showModal = false;
  isEdit = false;
  selectedId: number;
  filterText = '';
  isActiveFilter: boolean;
  sortField = 'CourseName';
  sortOrder = 1;
  courseForm: FormGroup;

  activeFilterOptions = [
    { label: 'Active', value: true },
    { label: 'Inactive', value: false }
  ];

  constructor(
    private courseService: CourseService,
    private fb: FormBuilder,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadCourses({ first: 0, rows: this.pageSize, sortField: 'CourseName', sortOrder: 1 });
  }

  buildForm(): void {
    this.courseForm = this.fb.group({
      courseName: ['', [Validators.required, Validators.maxLength(200)]],
      instructorName: ['', [Validators.required, Validators.maxLength(200)]],
      isActive: [true]
    });
  }

  loadCourses(event: any): void {
    this.loading = true;
    const sorting = event.sortField
      ? `${event.sortField} ${event.sortOrder === 1 ? 'asc' : 'desc'}`
      : 'CourseName';

    this.courseService.getAll(
      this.filterText, this.isActiveFilter, event.first, event.rows, sorting
    ).subscribe({
      next: (res) => {
        this.courses = res.result.items;
        this.totalCount = res.result.totalCount;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  onSearch(): void {
    this.loadCourses({ first: 0, rows: this.pageSize, sortField: this.sortField, sortOrder: this.sortOrder });
  }

  openCreateModal(): void {
    this.isEdit = false;
    this.courseForm.reset({ isActive: true });
    this.showModal = true;
  }

  openEditModal(course: CourseDto): void {
    this.isEdit = true;
    this.selectedId = course.id;
    this.courseForm.patchValue({
      courseName: course.courseName,
      instructorName: course.instructorName,
      isActive: course.isActive
    });
    this.showModal = true;
  }

  saveCourse(): void {
    if (this.courseForm.invalid) return;

    const formValue = this.courseForm.value;

    const operation = this.isEdit
      ? this.courseService.update({ id: this.selectedId, ...formValue })
      : this.courseService.create(formValue);

    operation.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Course ${this.isEdit ? 'updated' : 'created'} successfully.`
        });
        this.showModal = false;
        this.onSearch();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err?.error?.error?.message || 'An error occurred.'
        });
      }
    });
  }

  deleteCourse(id: number): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this course?',
      accept: () => {
        this.courseService.delete(id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Course deleted.' });
            this.onSearch();
          }
        });
      }
    });
  }
}
```

---

## PHASE 15: Frontend — Feedback Module

### Step 15.1 — Feedbacks Component Template

**`feedbacks.component.html`**
```html
<div class="card">
  <div class="flex justify-content-between align-items-center mb-4">
    <h2>Feedbacks</h2>
    <button pButton label="Add Feedback" icon="pi pi-plus"
            (click)="openCreateModal()" class="p-button-success"></button>
  </div>

  <!-- Filters -->
  <div class="flex gap-2 mb-4 flex-wrap">
    <span class="p-input-icon-left">
      <i class="pi pi-search"></i>
      <input pInputText type="text" [(ngModel)]="filterText"
             placeholder="Search..." (input)="onSearch()"/>
    </span>
    <p-dropdown [options]="activeCourses" [(ngModel)]="courseFilter"
                optionLabel="courseName" optionValue="id"
                placeholder="Filter by course" [showClear]="true"
                (onChange)="onSearch()"></p-dropdown>
    <p-dropdown [options]="ratingOptions" [(ngModel)]="ratingFilter"
                placeholder="Filter by rating" [showClear]="true"
                (onChange)="onSearch()"></p-dropdown>
  </div>

  <!-- Table -->
  <p-table [value]="feedbacks" [lazy]="true" (onLazyLoad)="loadFeedbacks($event)"
           [totalRecords]="totalCount" [rows]="pageSize" [paginator]="true"
           [loading]="loading" dataKey="id" styleClass="p-datatable-gridlines p-datatable-striped">
    <ng-template pTemplate="header">
      <tr>
        <th>Student</th>
        <th>Course</th>
        <th>Rating</th>
        <th>Comment</th>
        <th>Date</th>
        <th>Attachment</th>
        <th>Actions</th>
      </tr>
    </ng-template>
    <ng-template pTemplate="body" let-fb>
      <tr>
        <td>{{ fb.studentName }}</td>
        <td>{{ fb.courseName }}</td>
        <td>
          <p-rating [ngModel]="fb.rating" [readonly]="true" [cancel]="false" [stars]="5"></p-rating>
        </td>
        <td>{{ fb.comment | slice:0:80 }}{{ fb.comment?.length > 80 ? '...' : '' }}</td>
        <td>{{ fb.createdDate | date:'mediumDate' }}</td>
        <td>
          <a *ngIf="fb.attachmentPath" [href]="getFileUrl(fb.attachmentPath)"
             target="_blank" class="text-primary">
            <i class="pi pi-paperclip mr-1"></i>{{ fb.attachmentFileName }}
          </a>
          <span *ngIf="!fb.attachmentPath" class="text-secondary">None</span>
        </td>
        <td>
          <button pButton icon="pi pi-pencil" class="p-button-rounded p-button-info mr-2"
                  (click)="openEditModal(fb)"></button>
          <button pButton icon="pi pi-trash" class="p-button-rounded p-button-danger"
                  (click)="deleteFeedback(fb.id)"></button>
        </td>
      </tr>
    </ng-template>
  </p-table>
</div>

<!-- Create/Edit Modal -->
<p-dialog [(visible)]="showModal" [header]="isEdit ? 'Edit Feedback' : 'Add Feedback'"
          [modal]="true" [style]="{width: '560px'}" [draggable]="false">
  <form [formGroup]="feedbackForm" class="mt-2">
    <div class="field">
      <label>Student Name *</label>
      <input pInputText formControlName="studentName" class="w-full"/>
    </div>
    <div class="field mt-3">
      <label>Course *</label>
      <p-dropdown formControlName="courseId" [options]="activeCourses"
                  optionLabel="courseName" optionValue="id"
                  placeholder="Select course" class="w-full"></p-dropdown>
    </div>
    <div class="field mt-3">
      <label>Rating *</label>
      <p-rating formControlName="rating" [cancel]="false" [stars]="5"></p-rating>
    </div>
    <div class="field mt-3">
      <label>Comment</label>
      <textarea pInputTextarea formControlName="comment" rows="4" class="w-full"
                placeholder="Enter feedback comment..."></textarea>
    </div>
    <div class="field mt-3">
      <label>Attachment (PDF, JPG, PNG — max 10MB)</label>
      <p-fileUpload mode="basic" [auto]="true" [customUpload]="true"
                    (uploadHandler)="onFileUpload($event)"
                    accept=".pdf,.jpg,.jpeg,.png"
                    chooseLabel="Choose File" styleClass="w-full"></p-fileUpload>
      <small *ngIf="uploadedFileName" class="text-success mt-1 block">
        <i class="pi pi-check mr-1"></i>{{ uploadedFileName }} uploaded
      </small>
    </div>
  </form>
  <ng-template pTemplate="footer">
    <button pButton label="Cancel" icon="pi pi-times"
            (click)="showModal = false" class="p-button-text"></button>
    <button pButton label="Save" icon="pi pi-check"
            (click)="saveFeedback()" [disabled]="feedbackForm.invalid || uploading"></button>
  </ng-template>
</p-dialog>

<p-confirmDialog></p-confirmDialog>
<p-toast></p-toast>
```

### Step 15.2 — Feedbacks Component TypeScript

**`feedbacks.component.ts`**
```typescript
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FeedbackService, FeedbackDto } from '../services/feedback.service';
import { CourseService, CourseDto } from '../services/course.service';
import { AppConsts } from '@shared/AppConsts';

@Component({
  selector: 'app-feedbacks',
  templateUrl: './feedbacks.component.html'
})
export class FeedbacksComponent implements OnInit {
  feedbacks: FeedbackDto[] = [];
  activeCourses: CourseDto[] = [];
  totalCount = 0;
  pageSize = 10;
  loading = false;
  showModal = false;
  isEdit = false;
  selectedId: number;
  filterText = '';
  courseFilter: number;
  ratingFilter: number;
  feedbackForm: FormGroup;
  uploadedFilePath = '';
  uploadedFileName = '';
  uploading = false;

  ratingOptions = [1, 2, 3, 4, 5].map(r => ({ label: '★'.repeat(r), value: r }));

  constructor(
    private feedbackService: FeedbackService,
    private courseService: CourseService,
    private fb: FormBuilder,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadActiveCourses();
    this.loadFeedbacks({ first: 0, rows: this.pageSize });
  }

  buildForm(): void {
    this.feedbackForm = this.fb.group({
      studentName: ['', [Validators.required, Validators.maxLength(200)]],
      courseId: [null, Validators.required],
      rating: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: ['', Validators.maxLength(2000)]
    });
  }

  loadActiveCourses(): void {
    this.courseService.getActiveCourses().subscribe(res => {
      this.activeCourses = res.result.items;
    });
  }

  loadFeedbacks(event: any): void {
    this.loading = true;
    this.feedbackService.getAll(
      this.filterText, this.courseFilter, this.ratingFilter, event.first, event.rows
    ).subscribe({
      next: (res) => {
        this.feedbacks = res.result.items;
        this.totalCount = res.result.totalCount;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  onSearch(): void {
    this.loadFeedbacks({ first: 0, rows: this.pageSize });
  }

  openCreateModal(): void {
    this.isEdit = false;
    this.feedbackForm.reset();
    this.uploadedFilePath = '';
    this.uploadedFileName = '';
    this.showModal = true;
  }

  openEditModal(fb: FeedbackDto): void {
    this.isEdit = true;
    this.selectedId = fb.id;
    this.uploadedFilePath = fb.attachmentPath || '';
    this.uploadedFileName = fb.attachmentFileName || '';
    this.feedbackForm.patchValue({
      studentName: fb.studentName,
      courseId: fb.courseId,
      rating: fb.rating,
      comment: fb.comment
    });
    this.showModal = true;
  }

  onFileUpload(event: any): void {
    const file: File = event.files[0];
    if (!file) return;
    this.uploading = true;
    this.feedbackService.uploadAttachment(file).subscribe({
      next: (res) => {
        this.uploadedFilePath = res.filePath;
        this.uploadedFileName = file.name;
        this.uploading = false;
        this.messageService.add({ severity: 'info', summary: 'Uploaded', detail: 'File uploaded successfully.' });
      },
      error: (err) => {
        this.uploading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error || 'Upload failed.' });
      }
    });
  }

  saveFeedback(): void {
    if (this.feedbackForm.invalid) return;
    const input = {
      ...this.feedbackForm.value,
      attachmentPath: this.uploadedFilePath,
      attachmentFileName: this.uploadedFileName,
      ...(this.isEdit ? { id: this.selectedId } : {})
    };

    const op = this.isEdit
      ? this.feedbackService.update(input)
      : this.feedbackService.create(input);

    op.subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Saved', detail: 'Feedback saved.' });
        this.showModal = false;
        this.onSearch();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err?.error?.error?.message || 'An error occurred.'
        });
      }
    });
  }

  deleteFeedback(id: number): void {
    this.confirmationService.confirm({
      message: 'Delete this feedback?',
      accept: () => {
        this.feedbackService.delete(id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Feedback deleted.' });
            this.onSearch();
          }
        });
      }
    });
  }

  getFileUrl(path: string): string {
    return `${AppConsts.remoteServiceBaseUrl}${path}`;
  }
}
```

---

## PHASE 16: Frontend — Dashboard Card

**`dashboard.component.html`**
```html
<div class="grid mt-4" *ngIf="dashboardData">
  <!-- Total Feedback Count Card -->
  <div class="col-12 md:col-4">
    <div class="card text-center shadow-2">
      <div class="text-4xl font-bold text-primary">{{ dashboardData.totalFeedbackCount }}</div>
      <div class="text-xl mt-2 text-secondary">Total Feedbacks</div>
    </div>
  </div>

  <!-- Top 5 Courses by Rating -->
  <div class="col-12 md:col-8">
    <div class="card shadow-2">
      <h3 class="mb-3">Top 5 Courses by Average Rating</h3>
      <p-table [value]="dashboardData.topCoursesByRating" styleClass="p-datatable-sm">
        <ng-template pTemplate="header">
          <tr>
            <th>#</th>
            <th>Course</th>
            <th>Avg Rating</th>
            <th>Total Feedbacks</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-c let-rowIndex="rowIndex">
          <tr>
            <td>{{ rowIndex + 1 }}</td>
            <td>{{ c.courseName }}</td>
            <td>
              <p-rating [ngModel]="c.averageRating" [readonly]="true" [cancel]="false"></p-rating>
              <span class="ml-2 text-sm">({{ c.averageRating | number:'1.1-1' }})</span>
            </td>
            <td>{{ c.feedbackCount }}</td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  </div>
</div>
```

**`dashboard.component.ts`**
```typescript
import { Component, OnInit } from '@angular/core';
import { FeedbackService, DashboardDto } from '../services/feedback.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  dashboardData: DashboardDto;

  constructor(private feedbackService: FeedbackService) {}

  ngOnInit(): void {
    this.feedbackService.getDashboardData().subscribe(res => {
      this.dashboardData = res.result;
    });
  }
}
```

---

## PHASE 17: Frontend — Routing & Navigation

### Step 17.1 — Feature Module Routes

**`course-feedback-routing.module.ts`**
```typescript
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CoursesComponent } from './courses/courses.component';
import { FeedbacksComponent } from './feedbacks/feedbacks.component';
import { DashboardComponent } from './dashboard/dashboard.component';

const routes: Routes = [
  { path: 'courses', component: CoursesComponent },
  { path: 'feedbacks', component: FeedbacksComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CourseFeedbackRoutingModule {}
```

### Step 17.2 — Register in App Routing

In `angular/src/app/app-routing.module.ts`, add:
```typescript
{
  path: 'app/main/course-feedback',
  loadChildren: () =>
    import('./course-feedback/course-feedback.module').then(m => m.CourseFeedbackModule),
  canActivate: [AppRouteGuard]
}
```

### Step 17.3 — Add Navigation Menu Items

Open: `angular/src/app/shared/layout/nav/app-nav-item.component.html` or the sidebar config file.

Add:
```typescript
// In the menu items array (typically in app-navigation.service.ts or similar):
new AppMenuItem(
  'Course Feedback',
  'Pages.Courses',        // permission check
  'flaticon-home',
  '/app/main/course-feedback',
  [
    new AppMenuItem('Dashboard', '', 'flaticon-line-graph', '/app/main/course-feedback/dashboard'),
    new AppMenuItem('Courses', 'Pages.Courses', 'flaticon-book', '/app/main/course-feedback/courses'),
    new AppMenuItem('Feedbacks', 'Pages.Feedbacks', 'flaticon-chat', '/app/main/course-feedback/feedbacks'),
  ]
)
```

---

## PHASE 18: Testing & Validation Checklist

Work through each item below before submission:

### Backend Verification
```
[ ] API starts without errors: https://localhost:44301/swagger
[ ] /api/services/app/Course/GetAll returns paginated results
[ ] /api/services/app/Feedback/GetAll returns paginated results
[ ] Creating Feedback respects MaxFeedbackPerCourse setting
[ ] File upload endpoint accepts .pdf/.jpg/.png and rejects other types
[ ] Hangfire dashboard accessible at /hangfire
[ ] Background job "feedback-daily-check" appears in Hangfire recurring jobs
[ ] Permissions Pages.Courses and Pages.Feedbacks visible in Role Management
```

### Frontend Verification
```
[ ] Angular app loads at http://localhost:4200
[ ] Login with admin / 123qwe works
[ ] Navigation shows "Course Feedback" menu group
[ ] Courses list shows with pagination, sorting, filtering
[ ] Create/Edit course opens PrimeNG modal and saves correctly
[ ] Delete course with confirmation prompt works
[ ] Feedbacks list shows with course name, rating stars, file link
[ ] Create feedback validates rating 1-5, student name required
[ ] File upload in feedback form works, link shown in list
[ ] Rating dropdown filter works
[ ] Course filter dropdown shows only active courses
[ ] Dashboard shows total feedback count and top 5 courses
[ ] Role Management > Assign Pages.Courses permission → only accessible by that role
[ ] Tenant Settings page shows MaxFeedbackPerCourse, changing it restricts feedback creation
```

---

## PHASE 19: README Template

Create `README.md` at the project root:

```markdown
# Course Feedback Management System

## Tech Stack
- **Backend:** ASP.NET Core 6 / ABP Boilerplate
- **Frontend:** Angular 15 + PrimeNG
- **Database:** SQL Server / Entity Framework Core
- **Background Jobs:** Hangfire

## Setup Instructions

### Prerequisites
- .NET 6 SDK
- Node.js 16+
- SQL Server (LocalDB or Express)
- Angular CLI 15+

### Backend
1. Clone the repo
2. Update `aspnet-core/src/CourseFeedback.Web.Host/appsettings.json` connection string
3. Run migrations:
   ```
   dotnet ef database update --project ../CourseFeedback.EntityFrameworkCore
   ```
4. Start backend:
   ```
   cd aspnet-core/src/CourseFeedback.Web.Host
   dotnet run
   ```

### Frontend
1. Install dependencies:
   ```
   cd angular
   npm install
   ```
2. Start dev server:
   ```
   ng serve
   ```
3. Open http://localhost:4200

## Default Credentials
- **Tenant:** Default
- **Username:** admin
- **Password:** 123qwe

## Features Implemented
- Full CRUD for Courses and Feedbacks
- Role-based permissions (Pages.Courses, Pages.Feedbacks)
- Tenant-level setting: MaxFeedbackPerCourse
- File upload (PDF/JPG/PNG) on Feedback records
- Hangfire daily job checking inactive courses
- PrimeNG modals, paginated/sortable/filterable tables
- Dashboard with Top 5 Courses and Total Feedback count

## AI Tools Used
- **Claude / ChatGPT:** PRD generation, entity scaffolding, code review
- See `/docs/ai-chat-logs/` for screenshots of AI-assisted sessions

## Hangfire Dashboard
- URL: https://localhost:44301/hangfire
- Recurring Job: `feedback-daily-check` (runs daily at 8:00 AM UTC)
```

---

## Quick Reference: File Structure After Completion

```
aspnet-core/src/
├── CourseFeedback.Core/
│   ├── CourseFeedback/
│   │   ├── Course.cs
│   │   └── Feedback.cs
│   ├── Authorization/
│   │   └── PermissionNames.cs         ← added Pages.Courses, Pages.Feedbacks
│   └── Configuration/
│       └── AppSettingNames.cs         ← MaxFeedbackPerCourse
│       └── AppSettingProvider.cs
├── CourseFeedback.Application/
│   └── CourseFeedback/
│       ├── Dto/                       ← all DTOs
│       ├── ICourseAppService.cs
│       ├── CourseAppService.cs
│       ├── IFeedbackAppService.cs
│       ├── FeedbackAppService.cs
│       └── Jobs/
│           └── FeedbackCheckJob.cs
├── CourseFeedback.EntityFrameworkCore/
│   └── CourseFeedbackDbContext.cs     ← DbSets + Fluent config
└── CourseFeedback.Web.Host/
    ├── Controllers/
    │   └── FileUploadController.cs
    └── Startup.cs                     ← Hangfire config + static files

angular/src/app/
└── course-feedback/
    ├── courses/
    │   ├── courses.component.ts
    │   └── courses.component.html
    ├── feedbacks/
    │   ├── feedbacks.component.ts
    │   └── feedbacks.component.html
    ├── dashboard/
    │   ├── dashboard.component.ts
    │   └── dashboard.component.html
    ├── services/
    │   ├── course.service.ts
    │   └── feedback.service.ts
    ├── course-feedback.module.ts
    └── course-feedback-routing.module.ts
```

---

*End of Build Guide — Expected completion time: 2 days for a trainee developer*
