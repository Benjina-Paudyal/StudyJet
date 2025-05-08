using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyJet.API.Controllers;
using StudyJet.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ControllerTests
{
    public class UploadCVControllerTest
    {

        private readonly Mock<IFileStorageService> _mockFileStorageService;
        private readonly UploadCVController _controller;

        public UploadCVControllerTest()
        {
            _mockFileStorageService = new Mock<IFileStorageService>();
            _controller = new UploadCVController(_mockFileStorageService.Object);
        }



        [Fact]
        public async Task UploadCV_ShouldReturnBadRequest_WhenNoFileIsUploaded()
        {
            // Arrange
            IFormFile nullFile = null;

            // Act
            var result = await _controller.UploadCV(nullFile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var resultValue = badRequestResult.Value;
            var messageProperty = resultValue.GetType().GetProperty("message");

            Assert.NotNull(messageProperty); 
            var message = messageProperty.GetValue(resultValue)?.ToString();

            Assert.Equal("No file uploaded.", message);
        }

        [Fact]
        public async Task UploadCV_ShouldReturnBadRequest_WhenFileIsNotPDF()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.txt");
            mockFile.Setup(f => f.Length).Returns(1);

            // Act
            var result = await _controller.UploadCV(mockFile.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var resultValue = badRequestResult.Value;
            var messageProperty = resultValue.GetType().GetProperty("message");

            Assert.NotNull(messageProperty); 
            var message = messageProperty.GetValue(resultValue)?.ToString();

            Assert.Equal("Only PDF files are allowed.", message);
        }

        [Fact]
        public async Task UploadCV_ShouldReturnOk_WhenFileIsUploadedSuccessfully()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            mockFile.Setup(f => f.Length).Returns(1024);

            _mockFileStorageService.Setup(s => s.SaveCVAsync(mockFile.Object))
                                   .ReturnsAsync("https://example.com/file.pdf");

            // Act
            var result = await _controller.UploadCV(mockFile.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var resultValue = okResult.Value;
            var fileUrlProperty = resultValue.GetType().GetProperty("fileUrl");

            Assert.NotNull(fileUrlProperty); 
            var fileUrl = fileUrlProperty.GetValue(resultValue)?.ToString();

            Assert.Equal("https://example.com/file.pdf", fileUrl);
        }

        [Fact]
        public async Task UploadCV_ShouldReturnBadRequest_WhenUploadFails()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            mockFile.Setup(f => f.Length).Returns(1024);

            _mockFileStorageService.Setup(s => s.SaveCVAsync(mockFile.Object))
                                   .ThrowsAsync(new Exception("Upload failed"));

            // Act
            var result = await _controller.UploadCV(mockFile.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var resultValue = badRequestResult.Value;
            var messageProperty = resultValue.GetType().GetProperty("message");

            Assert.NotNull(messageProperty); 
            var message = messageProperty.GetValue(resultValue)?.ToString();

            Assert.Equal("Upload failed: Upload failed", message);
        }
    }
}
