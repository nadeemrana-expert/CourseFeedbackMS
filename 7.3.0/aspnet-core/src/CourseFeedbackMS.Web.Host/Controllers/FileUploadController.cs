using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CourseFeedbackMS.Controllers
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
