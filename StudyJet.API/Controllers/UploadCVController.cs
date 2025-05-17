using Microsoft.AspNetCore.Mvc;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadCVController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public UploadCVController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }


        [HttpPost("upload-cv")]
        public async Task<IActionResult> UploadCV(IFormFile cvFile)
        {
            // Validate if file is provided
            if (cvFile == null || cvFile.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            // Allowed file extensions
            var allowedExtensions = new[] { ".pdf" };
            var extension = Path.GetExtension(cvFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Only PDF files are allowed." });
            }

            try
            {
                var fileUrl = await _fileStorageService.SaveCVAsync(cvFile);
                return Ok(new { fileUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Upload failed: {ex.Message}" });
            }
        }



    }
}
