using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using StudyJet.API.Services.Implementation;
using StudyJet.API.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ServiceTests
{
    public class FileStorageServiceTest
    {

        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IOptions<FilePaths>> _mockFilePaths;
        private FileStorageService _fileStorageService;

        public FileStorageServiceTest()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["DefaultPaths:ProfilePicture"]).Returns("defaultProfilePic.png");

            _mockFilePaths = new Mock<IOptions<FilePaths>>();
            _mockFilePaths.Setup(fp => fp.Value).Returns(new FilePaths { CvPath = "some/path/to/cvs" });

            _fileStorageService = new FileStorageService(_mockConfiguration.Object, _mockFilePaths.Object);
        }


        [Fact]
        public async Task SaveImageAsync_ShouldThrowArgumentException_WhenFileIsNullOrEmpty()
        {
            // Arrange
            IFormFile file = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _fileStorageService.SaveImageAsync(file));
        }


        [Fact]
        public async Task SaveImageAsync_ShouldThrowArgumentException_WhenInvalidFileType()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("image.txt");
            fileMock.Setup(f => f.Length).Returns(1024);  // 1 KB file

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _fileStorageService.SaveImageAsync(fileMock.Object));
        }


        [Fact]
        public async Task SaveProfilePictureAsync_ShouldReturnDefaultProfilePicture_WhenNoFileUploaded()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0); 

            _mockConfiguration.Setup(c => c["DefaultPaths:ProfilePicture"]).Returns("/images/profiles/profilepic.png");

            // Act
            var result = await _fileStorageService.SaveProfilePictureAsync(fileMock.Object);

            // Assert
            Assert.Equal("/images/profiles/profilepic.png", result);
        }



        [Fact]
        public async Task SaveCVAsync_ShouldThrowArgumentException_WhenInvalidFileType()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("resume.jpg");
            fileMock.Setup(f => f.Length).Returns(1024);  // 1 KB file

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _fileStorageService.SaveCVAsync(fileMock.Object));
        }


        [Fact]
        public async Task SaveCVAsync_ShouldSaveFile_WhenValidFile()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("resume.pdf");
            fileMock.Setup(f => f.Length).Returns(1024);  // 1 KB file

            // Act
            var result = await _fileStorageService.SaveCVAsync(fileMock.Object);

            // Assert
            Assert.Contains("/uploads/CVs/", result);  
        }






    }
}
