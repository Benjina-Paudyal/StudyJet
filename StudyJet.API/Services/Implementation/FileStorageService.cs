using Microsoft.Extensions.Options;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;

namespace StudyJet.API.Services.Implementation
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly string _cvPath;


        public FileStorageService(IConfiguration configuration, IOptions<FilePaths> filePaths)
        {
            _configuration = configuration;
            _cvPath = filePaths.Value.CvPath;
        }

        // Course image
        public async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only image files are allowed.");

            var maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                throw new ArgumentException("File is too large. Maximum size allowed is 5 MB.");
            }

            var courseImagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "courses");
            if (!Directory.Exists(courseImagesDirectory))
            {
                Directory.CreateDirectory(courseImagesDirectory);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(courseImagesDirectory, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Error saving the file to disk.", ex);
            }

            return $"/images/courses/{fileName}";


        }

        // Profile image
        public async Task<string> SaveProfilePictureAsync(IFormFile profilePicture)
        {
            var defaultPic = _configuration["DefaultPaths:ProfilePicture"];

            if (profilePicture == null || profilePicture.Length == 0)
            {
                if (string.IsNullOrEmpty(defaultPic))
                {
                    return "/images/profiles/default-profile.png";
                }

                return defaultPic;
            }


            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(profilePicture.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("invalid file type. Only image files are allowed.");

            var maxFileSize = 5 * 1024 * 1024; // 5 MB limit
            if (profilePicture.Length > maxFileSize)
            {
                throw new ArgumentException("File is too large. Maximum size allowed is 5 MB.");
            }

            var profileImagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
            if (!Directory.Exists(profileImagesDirectory))
            {
                Directory.CreateDirectory(profileImagesDirectory);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(profileImagesDirectory, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Error saving the file to disk.", ex);
            }
            return $"/images/profiles/{fileName}";

        }

        // CVs
        public async Task<string> SaveCVAsync(IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
                throw new ArgumentException("No CV file uploaded.");

            var allowedExtensions = new[] { ".pdf", ".docx", ".txt" };
            var extension = Path.GetExtension(cvFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only PDF, DOCX, and TXT files are allowed.");

            var maxFileSize = 10 * 1024 * 1024; // 10 MB
            if (cvFile.Length > maxFileSize)
            {
                throw new ArgumentException("File is too large. Maximum size allowed is 10 MB.");
            }

            var cvDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "CVs");
            if (!Directory.Exists(cvDirectory))
            {
                Directory.CreateDirectory(cvDirectory);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(cvDirectory, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(stream);
                }
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Error saving the CV file to disk.", ex);
            }

            return $"/uploads/CVs/{fileName}";
        }


        // Clean up CVs
        public Task CleanupCvFilesAsync()
        {
            var allCvFiles = Directory.GetFiles(_cvPath);
            foreach (var file in allCvFiles)
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.LastWriteTime < DateTime.Now.AddDays(-7))
                {
                    File.Delete(file);
                }
            }

            return Task.CompletedTask;
        }

    }
}
