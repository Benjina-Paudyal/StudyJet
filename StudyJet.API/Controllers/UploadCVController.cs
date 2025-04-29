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
            if (cvFile == null)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                // Save the CV file and get the URL
                var fileUrl = await _fileStorageService.SaveCVAsync(cvFile);

                // Return the file URL or any other response you need
                return Ok(new { fileUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
