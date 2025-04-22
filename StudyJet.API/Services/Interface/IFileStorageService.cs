namespace StudyJet.API.Services.Interface
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(IFormFile file);
        Task<string> SaveProfilePictureAsync(IFormFile file);
        Task<string> SaveCVAsync(IFormFile cvFile);
        Task CleanupCvFilesAsync();

    }
}
