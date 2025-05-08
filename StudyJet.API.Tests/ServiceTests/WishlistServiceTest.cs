using Moq;
using StudyJet.API.DTOs.Wishlist;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ServiceTests
{
    public class WishlistServiceTest
    {

        private readonly Mock<IWishlistRepo> _mockWishlistRepo;
        private readonly WishlistService _wishlistService;


        public WishlistServiceTest()
        {
            _mockWishlistRepo = new Mock<IWishlistRepo>();
            _wishlistService = new WishlistService(_mockWishlistRepo.Object);
        }

        [Fact]
        public async Task GetWishlistAsync_UserHasWishlist_ReturnsWishlist()
        {
            // Arrange
            var userId = "user123";
            var wishlist = new List<WishlistCourseDTO>
        {
            new WishlistCourseDTO { CourseID = 10, Title = "Course 1" },
            new WishlistCourseDTO { CourseID = 20, Title = "Course 2" }
        };

            _mockWishlistRepo.Setup(repo => repo.SelectWishlistByIdAsync(userId)).ReturnsAsync(wishlist);

            // Act
            var result = await _wishlistService.GetWishlistAsync(userId);

            // Assert
            Assert.NotNull(result); 
            Assert.Equal(2, result.Count()); 
            Assert.Contains(result, course => course.CourseID == 10); 
            Assert.Contains(result, course => course.CourseID == 20); 
        }

        [Fact]
        public async Task GetWishlistAsync_UserHasNoWishlist_ReturnsEmptyList()
        {
            // Arrange
            var userId = "user123";

            _mockWishlistRepo.Setup(repo => repo.SelectWishlistByIdAsync(userId)).ReturnsAsync(new List<WishlistCourseDTO>());

            // Act
            var result = await _wishlistService.GetWishlistAsync(userId);

            // Assert
            Assert.NotNull(result); 
            Assert.Empty(result); 
        }





        [Fact]
        public async Task AddCourseToWishlistAsync_Success_ReturnsTrue()
        {
            // Arrange
            var userId = "user123";
            var courseId = 1;

            _mockWishlistRepo.Setup(repo => repo.InsertCourseToWishlistAsync(userId, courseId)).ReturnsAsync(true);

            // Act
            var result = await _wishlistService.AddCourseToWishlistAsync(userId, courseId);

            // Assert
            Assert.True(result); 
        }

        [Fact]
        public async Task AddCourseToWishlistAsync_Failure_ReturnsFalse()
        {
            // Arrange
            var userId = "user123";
            var courseId = 1;

            _mockWishlistRepo.Setup(repo => repo.InsertCourseToWishlistAsync(userId, courseId)).ReturnsAsync(false);

            // Act
            var result = await _wishlistService.AddCourseToWishlistAsync(userId, courseId);

            // Assert
            Assert.False(result); 
        }



        [Fact]
        public async Task RemoveCourseFromWishlistAsync_Success_ReturnsTrue()
        {
            // Arrange
            var userId = "user123";
            var courseId = 1;

            _mockWishlistRepo.Setup(repo => repo.DeleteCourseFromWishlistAsync(userId, courseId)).ReturnsAsync(true);

            // Act
            var result = await _wishlistService.RemoveCourseFromWishlistAsync(userId, courseId);

            // Assert
            Assert.True(result); 
        }

        [Fact]
        public async Task RemoveCourseFromWishlistAsync_Failure_ReturnsFalse()
        {
            // Arrange
            var userId = "user123";
            var courseId = 1;

            _mockWishlistRepo.Setup(repo => repo.DeleteCourseFromWishlistAsync(userId, courseId)).ReturnsAsync(false);

            // Act
            var result = await _wishlistService.RemoveCourseFromWishlistAsync(userId, courseId);

            // Assert
            Assert.False(result); 
        }



        [Fact]
        public async Task IsCourseInWishlistAsync_CourseInWishlist_ReturnsTrue()
        {
            // Arrange
            var userId = "user123";
            var courseId = 1;
            var wishlistItems = new List<WishlistCourseDTO>
        {
            new WishlistCourseDTO { CourseID = courseId, Title = "Course 1" },
            new WishlistCourseDTO { CourseID = 2, Title = "Course 2" }
        };

            _mockWishlistRepo.Setup(repo => repo.SelectWishlistByIdAsync(userId)).ReturnsAsync(wishlistItems);

            // Act
            var result = await _wishlistService.IsCourseInWishlistAsync(userId, courseId);

            // Assert
            Assert.True(result); 
        }

        [Fact]
        public async Task IsCourseInWishlistAsync_CourseNotInWishlist_ReturnsFalse()
        {
            // Arrange
            var userId = "user123";
            var courseId = 3;
            var wishlistItems = new List<WishlistCourseDTO>
        {
            new WishlistCourseDTO { CourseID = 1, Title = "Course 1" },
            new WishlistCourseDTO { CourseID = 2, Title = "Course 2" }
        };

            _mockWishlistRepo.Setup(repo => repo.SelectWishlistByIdAsync(userId)).ReturnsAsync(wishlistItems);

            // Act
            var result = await _wishlistService.IsCourseInWishlistAsync(userId, courseId);

            // Assert
            Assert.False(result);
        }




    }
}
