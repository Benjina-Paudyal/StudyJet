using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.DTOs.Course;
using StudyJet.API.DTOs.User;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Implementation;
using StudyJet.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ServiceTests
{
    public class CourseServiceTest
    {

        private readonly Mock<ICourseRepo> _mockCourseRepo;
        private readonly Mock<IFileStorageService> _mockFileStorageService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly CourseService _courseService;


        public CourseServiceTest()
        {
            // Mocking dependencies
            _mockCourseRepo = new Mock<ICourseRepo>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _mockNotificationService = new Mock<INotificationService>();


            // Creating the service instance
            _courseService = new CourseService(
                _mockCourseRepo.Object,
                _mockNotificationService.Object,
                _mockFileStorageService.Object
            );
        }



        [Fact]
        public async Task GetAllAsync_ReturnsCoursesWithUpdates_WhenCoursesAndUpdatesExist()
        {
            // Arrange
            var mockCourses = new List<Course>
            {
                new Course
                {
                    CourseID = 1,
                    Title = "Course 1",
                    Status = CourseStatus.Approved,
                    InstructorID = "Instructor1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now.AddDays(-1),
                    ImageUrl = "url1",
                    VideoUrl = "video1",
                },
                new Course
                {
                    CourseID = 2,
                    Title = "Course 2",
                    Status = CourseStatus.Pending,
                    InstructorID = "Instructor2",
                    CategoryID = 2,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now.AddDays(-2),
                    ImageUrl = "url2",
                    VideoUrl = "video2",
                }
            };

                var mockCourseUpdates = new List<CourseUpdate>
                {
                new CourseUpdate
                {
                    CourseID = 1,
                    Title = "Updated Course 1",
                    Description = "Updated Description 1",
                    ImageUrl = "updated_url1",
                    Price = 150,
                    SubmittedAt = DateTime.Now.AddDays(-1),
                    VideoUrl = "updated_video1",
                    Status = CourseStatus.Pending,
                }
                };

            _mockCourseRepo.Setup(repo => repo.SelectCourseByStatusAsync(It.IsAny<List<CourseStatus>>()))
                .ReturnsAsync(mockCourses);

            _mockCourseRepo.Setup(repo => repo.SelectCourseUpdatesByStatusAsync(It.IsAny<List<CourseStatus>>()))
                .ReturnsAsync(mockCourseUpdates);

            // Act
            var result = await _courseService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Updated Course 1", result[0].Title);  
            Assert.Equal("Course 2", result[1].Title);  
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoCoursesExist()
        {
            // Arrange
            _mockCourseRepo.Setup(repo => repo.SelectCourseByStatusAsync(It.IsAny<List<CourseStatus>>()))
                .ReturnsAsync(new List<Course>());  // No courses

            // Act
            var result = await _courseService.GetAllAsync();

            // Assert
            Assert.Empty(result);  // The result should be an empty list
        }





        [Fact]
        public async Task GetByInstructorIdAsync_ReturnsCourseWithPendingUpdate_WhenUpdateHasNullFields()
        {
            // Arrange
            var instructorId = "Instructor123";

            var courses = new List<Course>
            {
                new Course
                {
                    CourseID = 1,
                    Title = "Original Course",  // Original title
                    Description = "Original Description",  // Original description
                    InstructorID = instructorId,
                    Status = CourseStatus.Approved,
                    Instructor = new User { UserName = "Instructor 1" },
                    Category = new Category { Name = "Category 1" },
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    VideoUrl = "http://example.com/video1"
                }
            };

            var pendingUpdates = new List<CourseUpdate>
            {
                new CourseUpdate
                {
                    CourseID = 1,
                    Title = "Updated Title",  
                    Description = null,  
                    Price = null,  
                    Status = CourseStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    Course = courses.First() 
                }
            };

            _mockCourseRepo.Setup(repo => repo.SelectByInstructorIdAsync(instructorId))
                .ReturnsAsync(courses);
            _mockCourseRepo.Setup(repo => repo.SelectPendingUpdatesByInstructorIdAsync(instructorId))
                .ReturnsAsync(pendingUpdates);

            // Act
            var result = await _courseService.GetByInstructorIdAsync(instructorId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);  

            var courseResponse = result.FirstOrDefault(c => c.CourseID == 1);
            Assert.NotNull(courseResponse);

            Assert.Equal("Updated Title", courseResponse.Title);

            
            Assert.Equal("Original Description", courseResponse.Description);

            
            Assert.Equal(0, courseResponse.Price);  
            
            var sortedResult = result.OrderByDescending(c => c.Status == CourseStatus.Pending)
                                     .ThenByDescending(c => c.LastUpdatedDate)
                                     .ToList();

            Assert.Equal(sortedResult, result);  
            
            _mockCourseRepo.Verify(repo => repo.SelectByInstructorIdAsync(instructorId), Times.Once);
            _mockCourseRepo.Verify(repo => repo.SelectPendingUpdatesByInstructorIdAsync(instructorId), Times.Once);
        }

        [Fact]
        public async Task GetByInstructorIdAsync_ThrowsArgumentException_WhenInstructorIdIsNullOrEmpty()
        {
            // Arrange
            string instructorId = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _courseService.GetByInstructorIdAsync(instructorId));

            instructorId = "";
            await Assert.ThrowsAsync<ArgumentException>(() => _courseService.GetByInstructorIdAsync(instructorId));
        }




        [Fact]
        public async Task GetByIdAsync_ReturnsUpdatedCourse_WhenUpdateExists()
        {
            // Arrange
            int courseId = 1;
            var course = new Course
            {
                CourseID = courseId,
                Title = "Original Course",
                Description = "Original Description",
                ImageUrl = "http://example.com/image.jpg",
                Price = 100,
                InstructorID = "instructor-id",
                CategoryID = 1,
                CreationDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now,
                VideoUrl = "http://example.com/video.mp4",
                Status = CourseStatus.Approved
            };

            var update = new CourseUpdate
            {
                CourseID = courseId,
                Title = "Updated Course",  
                Description = "Updated Description",  
                Price = 120,  
                SubmittedAt = DateTime.Now,
                Status = CourseStatus.Pending  
            };

            _mockCourseRepo.Setup(repo => repo.SelectByIdAsync(courseId)).ReturnsAsync(course);
            _mockCourseRepo.Setup(repo => repo.SelectLatestCourseUpdateByCourseIdAsync(courseId)).ReturnsAsync(update);

            // Act
            var result = await _courseService.GetByIdAsync(courseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(courseId, result.CourseID);
            Assert.Equal("Updated Course", result.Title);  
            Assert.Equal("Updated Description", result.Description);  
            Assert.Equal(120, result.Price);  
            Assert.Equal(CourseStatus.Pending, result.Status); 
            Assert.True(result.IsUpdate);  
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCourseDoesNotExist()
        {
            // Arrange
            int courseId = 999;  
            _mockCourseRepo.Setup(repo => repo.SelectByIdAsync(courseId)).ReturnsAsync((Course)null);

            // Act
            var result = await _courseService.GetByIdAsync(courseId);

            // Assert
            Assert.Null(result);  
        }




        [Fact]
        public async Task GetCourseForUpdateAsync_ReturnsCourse_WhenCourseExistsAndInstructorOwnsIt()
        {
            // Arrange
            int courseId = 1;
            string instructorId = "instructor-id";

            var course = new Course
            {
                CourseID = courseId,
                InstructorID = instructorId, // Instructor is the owner
                Title = "Course Title",
                Description = "Course Description",
                Price = 100,
                Status = CourseStatus.Approved,
                CreationDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now
            };

            var courseUpdateDTO = new CourseUpdateDTO
            {
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                InstructorID = course.InstructorID
            };

            _mockCourseRepo.Setup(repo => repo.SelectCourseForUpdateAsync(courseId, instructorId)).ReturnsAsync(courseUpdateDTO);

            // Act
            var result = await _courseService.GetCourseForUpdateAsync(courseId, instructorId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Course Title", result.Title);
            Assert.Equal("Course Description", result.Description);
            Assert.Equal(100, result.Price);
        }

        [Fact]
        public async Task GetCourseForUpdateAsync_ReturnsNull_WhenCourseDoesNotExistOrInstructorDoesNotOwnIt()
        {
            // Arrange
            int courseId = 999;  
            string instructorId = "instructor-id";

            _mockCourseRepo.Setup(repo => repo.SelectCourseForUpdateAsync(courseId, instructorId)).ReturnsAsync((CourseUpdateDTO)null);

            // Act
            var result = await _courseService.GetCourseForUpdateAsync(courseId, instructorId);

            // Assert
            Assert.Null(result);  
        }




        [Fact]
        public async Task GetEnrolledStudentsByCourseIdAsync_ReturnsEnrolledStudents_WhenCourseHasStudents()
        {
            // Arrange
            int courseId = 1;

            var students = new List<IdentityUser>
            {
                 new IdentityUser { Id = "user1", UserName = "studentOne", Email = "student1@example.com" },
                 new IdentityUser { Id = "user2", UserName = "studentTwo", Email = "student2@example.com" }
            };

            _mockCourseRepo.Setup(repo => repo.SelectEnrolledStudentsByCourseIdAsync(It.IsAny<int>()))
               .ReturnsAsync(new List<User>
               {
                   new User { Id = "user1", UserName = "studentOne" },
                   new User { Id = "user2", UserName = "studentTwo" }
               });


            // Act
            var result = await _courseService.GetEnrolledStudentsByCourseIdAsync(courseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);  
            Assert.Contains(result, student => student.UserName == "studentOne");
            Assert.Contains(result, student => student.UserName == "studentTwo");
        }

        [Fact]
        public async Task GetEnrolledStudentsByCourseIdAsync_ReturnsEmptyList_WhenNoStudentsAreEnrolled()
        {
            // Arrange
            int courseId = 1;  

            _mockCourseRepo.Setup(repo => repo.SelectEnrolledStudentsByCourseIdAsync(courseId))
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _courseService.GetEnrolledStudentsByCourseIdAsync(courseId);

            // Assert
            Assert.NotNull(result);  // Ensure that the result is not null
            Assert.Empty(result);  // Ensure that the result is an empty list (no students enrolled)
        }




        [Fact]
        public async Task GetCoursesWithStudentsForInstructorAsync_ReturnsCoursesWithDetailedStudents()
        {
            // Arrange
            string instructorId = "instructor1";

            var coursesWithStudents = new List<CourseWithStudentsDTO>
            {
                new CourseWithStudentsDTO
                {
                    CourseID = 1,
                    Title = "ASP.NET Basics",
                    ImageUrl = "course1.png",
                    Students = new List<StudentDTO>
                {
                    new StudentDTO
                        {
                            FullName = "Alice Johnson",
                            UserName = "alicej",
                            Email = "alice@example.com",
                            ProfilePictureUrl = "alice.jpg",
                            PurchaseDate = DateTime.UtcNow.AddDays(-5)
                        }
                }
            }
         };

            _mockCourseRepo.Setup(repo => repo.SelectCoursesWithStudentsForInstructorAsync(instructorId))
                           .ReturnsAsync(coursesWithStudents);

            // Act
            var result = await _courseService.GetCoursesWithStudentsForInstructorAsync(instructorId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("ASP.NET Basics", result[0].Title);
            Assert.Single(result[0].Students);
            Assert.Equal("alicej", result[0].Students[0].UserName);
            Assert.Equal("alice@example.com", result[0].Students[0].Email);
        }

        [Fact]
        public async Task GetCoursesWithStudentsForInstructorAsync_ReturnsEmptyList_WhenInstructorHasNoCourses()
        {
            // Arrange
            string instructorId = "instructor1";

            _mockCourseRepo.Setup(repo => repo.SelectCoursesWithStudentsForInstructorAsync(instructorId))
                           .ReturnsAsync(new List<CourseWithStudentsDTO>()); 
            // Act
            var result = await _courseService.GetCoursesWithStudentsForInstructorAsync(instructorId);

            // Assert
            Assert.NotNull(result);          
            Assert.Empty(result);            
        }



        [Fact]
        public async Task GetPopularCoursesAsync_ReturnsPopularCourses_WhenCoursesExist()
        {
            // Arrange
            var popularCourses = new List<Course>
            {
                new Course
                {
                    CourseID = 1,
                    Title = "ASP.NET Core Basics",
                    Description = "Learn ASP.NET Core",
                    ImageUrl = "aspnetcore.png",
                    Price = 49.99M,
                    InstructorID = "instructor1",
                    Instructor = new User { FullName = "John Doe" },
                    CategoryID = 1,
                    Category = new Category { Name = "Programming" },
                    CreationDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    VideoUrl = "course1video.mp4",
                },
                new Course
                {
                    CourseID = 2,
                    Title = "JavaScript Essentials",
                    Description = "Master JavaScript",
                    ImageUrl = "javascript.png",
                    Price = 29.99M,
                    InstructorID = "instructor2",
                    Instructor = new User { FullName = "Jane Smith" },
                    CategoryID = 2,
                    Category = new Category { Name = "Web Development" },
                    CreationDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    VideoUrl = "course2video.mp4",
                }
            };

            _mockCourseRepo.Setup(repo => repo.SelectPopularCoursesAsync())
                           .ReturnsAsync(popularCourses);

            // Act
            var result = await _courseService.GetPopularCoursesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); 
            Assert.Contains(result, course => course.Title == "ASP.NET Core Basics");
            Assert.Contains(result, course => course.Title == "JavaScript Essentials");
        }

        [Fact]
        public async Task GetPopularCoursesAsync_ReturnsEmptyList_WhenNoPopularCoursesExist()
        {
            // Arrange
            _mockCourseRepo.Setup(repo => repo.SelectPopularCoursesAsync())
                           .ReturnsAsync(new List<Course>()); 

            // Act
            var result = await _courseService.GetPopularCoursesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); 
        }




        [Fact]
        public async Task SearchCoursesAsync_CallsRepositoryMethod()
        {
            // Arrange
            string searchQuery = "ASP.NET Core";

            var courseDTOs = new List<CourseResponseDTO>
            {
                new CourseResponseDTO { CourseID = 1, Title = "ASP.NET Core Basics" },
                new CourseResponseDTO { CourseID = 2, Title = "ASP.NET Core Advanced" }
            };

            _mockCourseRepo.Setup(repo => repo.SearchCoursesAsync(searchQuery))
                           .ReturnsAsync(courseDTOs); 
            // Act
            var result = await _courseService.SearchCoursesAsync(searchQuery); 

            // Assert
            _mockCourseRepo.Verify(repo => repo.SearchCoursesAsync(searchQuery), Times.Once);

            Assert.Equal(2, result.Count);  
            Assert.Equal("ASP.NET Core Basics", result[0].Title);
        }

        [Fact]
        public async Task SearchCoursesAsync_ReturnsEmptyList_WhenRepositoryReturnsEmpty()
        {
            // Arrange
            string searchQuery = "NonExistingQuery";

            _mockCourseRepo.Setup(repo => repo.SearchCoursesAsync(searchQuery))
                           .ReturnsAsync(new List<CourseResponseDTO>());  
            // Act
            var result = await _courseService.SearchCoursesAsync(searchQuery);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);  
        }



        [Fact]
        public async Task GetPendingCoursesAsync_ReturnsCourses_WhenRepositoryReturnsCourses()
        {
            // Arrange
            var pendingCourses = new List<Course>
            {
                new Course { CourseID = 1, Title = "Course 1", Status = CourseStatus.Pending },
                new Course { CourseID = 2, Title = "Course 2", Status = CourseStatus.Pending }
            };

            _mockCourseRepo.Setup(repo => repo.SelectPendingCoursesAsync())
                           .ReturnsAsync(pendingCourses);

            // Act
            var result = await _courseService.GetPendingCoursesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());  
            Assert.Contains(result, course => course.CourseID == 1);
            Assert.Contains(result, course => course.CourseID == 2);
        }

        [Fact]
        public async Task GetPendingCoursesAsync_ReturnsEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            var pendingCourses = new List<Course>(); 

            _mockCourseRepo.Setup(repo => repo.SelectPendingCoursesAsync())
                           .ReturnsAsync(pendingCourses);

            // Act
            var result = await _courseService.GetPendingCoursesAsync();

            // Assert
            Assert.NotNull(result);  
            Assert.Empty(result);  
        }



        [Fact]
        public async Task GetApprovedCoursesAsync_ReturnsCourses_WhenRepositoryReturnsCourses()
        {
            // Arrange
            var approvedCourses = new List<Course>
            {
                new Course { CourseID = 1, Title = "Approved Course 1", Status = CourseStatus.Approved },
                new Course { CourseID = 2, Title = "Approved Course 2", Status = CourseStatus.Approved }
            };

            _mockCourseRepo.Setup(repo => repo.SelectApprovedCoursesAsync())
                           .ReturnsAsync(approvedCourses);

            // Act
            var result = await _courseService.GetApprovedCoursesAsync();

            // Assert
            Assert.NotNull(result);  
            Assert.Equal(2, result.Count());  
            Assert.Contains(result, course => course.CourseID == 1);  
            Assert.Contains(result, course => course.CourseID == 2);  
        }

        [Fact]
        public async Task GetApprovedCoursesAsync_ReturnsEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            var approvedCourses = new List<Course>();  

            _mockCourseRepo.Setup(repo => repo.SelectApprovedCoursesAsync())
                           .ReturnsAsync(approvedCourses);

            // Act
            var result = await _courseService.GetApprovedCoursesAsync();

            // Assert
            Assert.NotNull(result);  
            Assert.Empty(result);  
        }



        [Fact]
        public async Task GetTotalCoursesByInstructorIdAsync_ReturnsTotalCourses_WhenInstructorHasCourses()
        {
            // Arrange
            string instructorId = "instructor1";
            int expectedTotalCourses = 5;  

            _mockCourseRepo.Setup(repo => repo.SelectTotalCoursesByInstructorIdAsync(instructorId))
                           .ReturnsAsync(expectedTotalCourses);

            // Act
            var result = await _courseService.GetTotalCoursesByInstructorIdAsync(instructorId);

            // Assert
            Assert.Equal(expectedTotalCourses, result);  
        }

        [Fact]
        public async Task GetTotalCoursesByInstructorIdAsync_ReturnsZero_WhenInstructorHasNoCourses()
        {
            // Arrange
            string instructorId = "instructor1";
            int expectedTotalCourses = 0;  

            _mockCourseRepo.Setup(repo => repo.SelectTotalCoursesByInstructorIdAsync(instructorId))
                           .ReturnsAsync(expectedTotalCourses);

            // Act
            var result = await _courseService.GetTotalCoursesByInstructorIdAsync(instructorId);

            // Assert
            Assert.Equal(expectedTotalCourses, result);  
        }



        [Fact]
        public async Task AddAsync_ReturnsCourseId_WhenCourseIsAddedSuccessfully()
        {
            // Arrange
            var courseDto = new CreateCourseRequestDTO
            {
                Title = "New Course",
                Description = "Course Description",
                Price = 99.99m,
                InstructorID = "instructor1",
                CategoryID = 1,
                VideoUrl = "http://example.com/video",
                ImageFile = null 
            };

            var course = new Course
            {
                CourseID = 1, 
                Title = courseDto.Title,
                Description = courseDto.Description,
                Price = courseDto.Price,
                InstructorID = courseDto.InstructorID,
                CategoryID = courseDto.CategoryID,
                VideoUrl = courseDto.VideoUrl,
                ImageUrl = null,
                Status = CourseStatus.Pending,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            _mockCourseRepo.Setup(repo => repo.InsertAsync(It.IsAny<Course>()))
                           .ReturnsAsync(course); 

            // Act
            var result = await _courseService.AddAsync(courseDto);

            // Assert
            Assert.Equal(1, result); 
        }

        [Fact]
        public async Task AddAsync_SavesImage_WhenImageFileIsProvided()
        {
            // Arrange
            var courseDto = new CreateCourseRequestDTO
            {
                Title = "New Course with Image",
                Description = "Course Description",
                Price = 99.99m,
                InstructorID = "instructor1",
                CategoryID = 1,
                VideoUrl = "http://example.com/video",
                ImageFile = new Mock<IFormFile>().Object 
            };

            var imagePath = "path/to/saved/image.jpg"; 
            var course = new Course
            {
                CourseID = 1,
                Title = courseDto.Title,
                Description = courseDto.Description,
                Price = courseDto.Price,
                InstructorID = courseDto.InstructorID,
                CategoryID = courseDto.CategoryID,
                VideoUrl = courseDto.VideoUrl,
                ImageUrl = imagePath,
                Status = CourseStatus.Pending,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            _mockFileStorageService.Setup(fs => fs.SaveImageAsync(courseDto.ImageFile))
                                   .ReturnsAsync(imagePath);

            
            _mockCourseRepo.Setup(repo => repo.InsertAsync(It.IsAny<Course>()))
                           .ReturnsAsync(course);

            // Act
            var result = await _courseService.AddAsync(courseDto);

            // Assert
            Assert.Equal(1, result); 
            _mockFileStorageService.Verify(fs => fs.SaveImageAsync(courseDto.ImageFile), Times.Once); 
        }



        [Fact]
        public async Task GetTotalCoursesAsync_ReturnsTotalCount_WhenCoursesExist()
        {
            // Arrange
            int expectedCount = 10; 
            _mockCourseRepo.Setup(repo => repo.SelectTotalCoursesAsync())
                           .ReturnsAsync(expectedCount); 

            // Act
            var result = await _courseService.GetTotalCoursesAsync();

            // Assert
            Assert.Equal(expectedCount, result); 
        }

        [Fact]
        public async Task GetTotalCoursesAsync_ReturnsZero_WhenNoCoursesExist()
        {
            // Arrange
            int expectedCount = 0; 
            _mockCourseRepo.Setup(repo => repo.SelectTotalCoursesAsync())
                           .ReturnsAsync(expectedCount); 

            // Act
            var result = await _courseService.GetTotalCoursesAsync();

            // Assert
            Assert.Equal(expectedCount, result); 
        }



        [Fact]
        public async Task SubmitCourseUpdateAsync_ReturnsTrue_WhenUpdateIsSuccessful()
        {
            // Arrange
            int courseId = 1;
            var updateDto = new UpdateCourseRequestDTO
            {
                Title = "Updated Course Title",
                Description = "Updated Course Description",
                Price = 99.99m,
                VideoUrl = "updated-video-url"
            };
            _mockCourseRepo.Setup(repo => repo.SubmitCourseUpdateAsync(courseId, updateDto, _mockFileStorageService.Object))
                           .ReturnsAsync(true); 

            // Act
            var result = await _courseService.SubmitCourseUpdateAsync(courseId, updateDto);

            // Assert
            Assert.True(result); 
        }
        
        [Fact]
        public async Task SubmitCourseUpdateAsync_ThrowsArgumentNullException_WhenUpdateDtoIsNull()
        {
            // Arrange
            int courseId = 1;
            UpdateCourseRequestDTO updateDto = null; 

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _courseService.SubmitCourseUpdateAsync(courseId, updateDto)
            );

            Assert.Equal("Update course request cannot be null. (Parameter 'updateDto')", exception.Message);
        }



        [Fact]
        public async Task ApproveCourseAsync_ReturnsTrue_WhenCourseExists()
        {
            // Arrange
            int courseId = 1;
            var course = new Course
            {
                CourseID = courseId,
                Title = "Course Title",
                Status = CourseStatus.Pending,
                LastUpdatedDate = DateTime.UtcNow
            };

            _mockCourseRepo.Setup(repo => repo.SelectByIdAsync(courseId))
                           .ReturnsAsync(course);
            _mockCourseRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Course>()))
                           .ReturnsAsync(true);

            // Act
            var result = await _courseService.ApproveCourseAsync(courseId);

            // Assert
            Assert.True(result);
            Assert.Equal(CourseStatus.Approved, course.Status); 
        }

        [Fact]
        public async Task ApproveCourseAsync_ReturnsFalse_WhenCourseDoesNotExist()
        {
            // Arrange
            int courseId = 1;

            _mockCourseRepo.Setup(repo => repo.SelectByIdAsync(courseId))
                           .ReturnsAsync((Course)null); 

            // Act
            var result = await _courseService.ApproveCourseAsync(courseId);

            // Assert
            Assert.False(result); 
        }




        [Fact]
        public async Task RejectCourseAsync_ReturnsTrue_WhenCourseExists()
        {
            // Arrange
            int courseId = 1;
            var course = new Course
            {
                CourseID = courseId,
                Title = "Course Title",
                Status = CourseStatus.Pending,
                LastUpdatedDate = DateTime.UtcNow
            };

            _mockCourseRepo.Setup(repo => repo.SelectByIdAsync(courseId))
                           .ReturnsAsync(course); 
            _mockCourseRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Course>()))
                           .ReturnsAsync(true); 

            // Act
            var result = await _courseService.RejectCourseAsync(courseId);

            // Assert
            Assert.True(result); 
            Assert.Equal(CourseStatus.Rejected, course.Status); 
        }

        [Fact]
        public async Task RejectCourseAsync_ReturnsFalse_WhenCourseDoesNotExist()
        {
            // Arrange
            int courseId = 999; 
            _mockCourseRepo.Setup(repo => repo.SelectByIdAsync(courseId))
                           .ReturnsAsync((Course)null); 

            // Act
            var result = await _courseService.RejectCourseAsync(courseId);

            // Assert
            Assert.False(result); 
        }



        [Fact]
        public async Task ApprovePendingUpdatesAsync_ReturnsTrue_WhenPendingUpdateExists()
        {
            // Arrange
            int courseId = 1;
            var pendingUpdate = new CourseUpdate
            {
                CourseID = courseId,
                Title = "Updated Course Title",
                Description = "Updated Description"
            };

            _mockCourseRepo.Setup(repo => repo.SelectPendingCourseUpdateAsync(courseId))
                           .ReturnsAsync(pendingUpdate); 
            _mockCourseRepo.Setup(repo => repo.ApprovePendingUpdatesAsync(courseId))
                           .ReturnsAsync(true); 

            // Act
            var result = await _courseService.ApprovePendingUpdatesAsync(courseId);

            // Assert
            Assert.True(result); 
        }

        [Fact]
        public async Task ApprovePendingUpdatesAsync_ThrowsInvalidOperationException_WhenNoPendingUpdateExists()
        {
            // Arrange
            int courseId = 999; 

            _mockCourseRepo.Setup(repo => repo.SelectPendingCourseUpdateAsync(courseId))
                           .ReturnsAsync((CourseUpdate)null); 

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseService.ApprovePendingUpdatesAsync(courseId)); 

            Assert.Equal("No pending updates found for the course.", exception.Message); 
        }



        [Fact]
        public async Task RejectPendingUpdatesAsync_ReturnsTrue_WhenPendingUpdateExists()
        {
            // Arrange
            int courseId = 1;
            var pendingUpdate = new CourseUpdate
            {
                CourseID = courseId,
                Title = "Updated Course Title",
                Description = "Updated Description"
            };

            _mockCourseRepo.Setup(repo => repo.SelectPendingCourseUpdateAsync(courseId))
                           .ReturnsAsync(pendingUpdate); 
            _mockCourseRepo.Setup(repo => repo.RejectPendingUpdatesAsync(courseId))
                           .ReturnsAsync(true); 

            _mockNotificationService.Setup(service => service.NotifyInstructorOnCourseUpdateRejectionAsync(courseId))
                                    .Returns(Task.CompletedTask); 

            // Act
            var result = await _courseService.RejectPendingUpdatesAsync(courseId);

            // Assert
            Assert.True(result); 
            _mockNotificationService.Verify(service => service.NotifyInstructorOnCourseUpdateRejectionAsync(courseId), Times.Once); // Verify notification
        }

        [Fact]
        public async Task RejectPendingUpdatesAsync_ThrowsInvalidOperationException_WhenNoPendingUpdateExists()
        {
            // Arrange
            int courseId = 999;

            // Mock repository methods
            _mockCourseRepo.Setup(repo => repo.SelectPendingCourseUpdateAsync(courseId))
                           .ReturnsAsync((CourseUpdate)null); 

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseService.RejectPendingUpdatesAsync(courseId)); 

            Assert.Equal("No pending updates found for the course.", exception.Message); 
        }



        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenCourseExists()
        {
            // Arrange
            int courseId = 1; 
            _mockCourseRepo.Setup(repo => repo.ExistsAsync(courseId))
                           .ReturnsAsync(true); 

            // Act
            var result = await _courseService.ExistsAsync(courseId);

            // Assert
            Assert.True(result); 
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenCourseDoesNotExist()
        {
            // Arrange
            int courseId = 999; 
            _mockCourseRepo.Setup(repo => repo.ExistsAsync(courseId))
                           .ReturnsAsync(false); 

            // Act
            var result = await _courseService.ExistsAsync(courseId);

            // Assert
            Assert.False(result); 
        }



        [Fact]
        public async Task UpdateAsync_UpdatesCourse_WhenCourseExists()
        {
            // Arrange
            int courseId = 1;
            var existingCourse = new Course
            {
                CourseID = courseId,
                Title = "Old Title",
                Description = "Old Description",
                Price = 100,
                VideoUrl = "old-video-url",
                ImageUrl = "old-image-url"
            };

            var updateDto = new UpdateCourseRequestDTO
            {
                Title = "New Title",
                Description = "New Description",
                Price = 120,
                VideoUrl = "new-video-url",
                ImageFile = new Mock<IFormFile>().Object 
            };

            _mockCourseRepo.Setup(repo => repo.SelectByIdAsync(courseId))
                           .ReturnsAsync(existingCourse);
            _mockFileStorageService.Setup(service => service.SaveImageAsync(It.IsAny<IFormFile>()))
                                   .ReturnsAsync("new-image-url");
            _mockCourseRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Course>()))
                           .ReturnsAsync(true);


            // Act
            var result = await _courseService.UpdateAsync(courseId, updateDto);

            // Assert
            Assert.True(result);
            Assert.Equal("New Title", existingCourse.Title);
            Assert.Equal("New Description", existingCourse.Description);
            Assert.Equal(120, existingCourse.Price);
            Assert.Equal("new-video-url", existingCourse.VideoUrl);
            Assert.Equal("new-image-url", existingCourse.ImageUrl);
        }


        [Fact]
        public async Task UpdateAsync_ThrowsException_WhenCourseNotFound()
        {
            // Arrange
            int courseId = 999; 
            var updateDto = new UpdateCourseRequestDTO
            {
                Title = "New Title"
            };

            _mockCourseRepo.Setup(repo => repo.SelectByIdAsync(courseId))
                           .ReturnsAsync((Course)null); 

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _courseService.UpdateAsync(courseId, updateDto));
        }



































    }
}



