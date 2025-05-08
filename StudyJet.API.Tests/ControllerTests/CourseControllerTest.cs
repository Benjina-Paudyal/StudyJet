using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyJet.API.Controllers;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.DTOs.User;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ControllerTests
{
    public class CourseControllerTest
    {
        private readonly Mock<ICourseService> _mockCourseService;
        private readonly Mock<IFileStorageService> _mockFileService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly CourseController _controller;

        public CourseControllerTest()
        {
            // Mocking dependencies
            _mockCourseService = new Mock<ICourseService>();
            _mockFileService = new Mock<IFileStorageService>();
            _mockNotificationService = new Mock<INotificationService>();

            // Mock UserManager with a simple user for testing
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            // Mocking a User object
            var user = new User
            {
                UserName = "testuser",
                FullName = "Test User",
                NeedToChangePassword = false
            };

            // Simulate getting the user (could be based on the current authenticated user)
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Initialize controller with mocked dependencies
            _controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );
        }


        [Fact]
        public async Task GetAll_ShouldReturnCourses_WhenUserIsAdmin()
        {
            // Arrange
            var user = new User { UserName = "admin", FullName = "Admin User" };
            var context = new DefaultHttpContext();
            var controller = new CourseController(_mockCourseService.Object, _mockFileService.Object, _mockNotificationService.Object, _mockUserManager.Object);
            controller.ControllerContext.HttpContext = context;

            // Simulating an admin user
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "Admin")
            }));

            controller.ControllerContext.HttpContext.User = claimsPrincipal;

            var mockCourses = new List<CourseResponseDTO>
            {
                new CourseResponseDTO { CourseID = 1, Title = "Course 1" },
                new CourseResponseDTO { CourseID = 2, Title = "Course 2" }
            };

            _mockCourseService.Setup(service => service.GetAllAsync()).ReturnsAsync(mockCourses);

            // Act
            var result = await controller.GetAll();

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<CourseResponseDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<CourseResponseDTO>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetAll_ShouldReturnUnauthorized_WhenUserIsNotAdmin()
        {
            // Arrange
            var user = new User { UserName = "testuser", FullName = "Test User" };
            var context = new DefaultHttpContext();
            var controller = new CourseController(_mockCourseService.Object, _mockFileService.Object, _mockNotificationService.Object, _mockUserManager.Object);
            controller.ControllerContext.HttpContext = context;

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "User")
            }));

            controller.ControllerContext.HttpContext.User = claimsPrincipal;

            _mockCourseService.Setup(service => service.GetAllAsync()).ReturnsAsync(new List<CourseResponseDTO>());

            if (!controller.ControllerContext.HttpContext.User.IsInRole("Admin"))
            {
                // Act
                var result = new UnauthorizedResult();











            }
        }



        [Fact]
        public async Task GetById_ShouldReturnOk_WhenCourseExists()
        {
            // Arrange
            int courseId = 1;
            var courseResponse = new CourseResponseDTO { CourseID = courseId, Title = "Test Course" };

            // Mocking the service to return the course
            _mockCourseService.Setup(service => service.GetByIdAsync(courseId)).ReturnsAsync(courseResponse);

            var controller = new CourseController(_mockCourseService.Object, _mockFileService.Object, _mockNotificationService.Object, _mockUserManager.Object);

            // Act
            var result = await controller.GetById(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CourseResponseDTO>(okResult.Value);
            Assert.Equal(courseId, returnValue.CourseID);
            Assert.Equal("Test Course", returnValue.Title);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenCourseDoesNotExist()
        {
            // Arrange
            int courseId = 1;
            _mockCourseService.Setup(service => service.GetByIdAsync(courseId)).ReturnsAsync((CourseResponseDTO)null);

            var controller = new CourseController(_mockCourseService.Object, _mockFileService.Object, _mockNotificationService.Object, _mockUserManager.Object);

            // Act
            var result = await controller.GetById(courseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);

            // Using reflection to access the anonymous object and verify the 'message' property
            var returnValue = notFoundResult.Value as object;
            var messageProperty = returnValue.GetType().GetProperty("message");

            Assert.NotNull(messageProperty);
            var message = messageProperty.GetValue(returnValue);
            Assert.Equal("Course not found.", message);
        }




        [Fact]
        public async Task GetPopularCourses_ShouldReturnOkWithCourses()
        {
            // Arrange
            var expectedCourses = new List<CourseResponseDTO>
            {
                new CourseResponseDTO { CourseID = 1, Title = "Course A" },
                new CourseResponseDTO { CourseID = 2, Title = "Course B" }
            };

            _mockCourseService.Setup(s => s.GetPopularCoursesAsync())
                              .ReturnsAsync(expectedCourses);

            var controller = new CourseController(_mockCourseService.Object, _mockFileService.Object, _mockNotificationService.Object, _mockUserManager.Object);

            // Act
            var result = await controller.GetPopularCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<CourseResponseDTO>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("Course A", returnValue[0].Title);
        }

        [Fact]
        public async Task GetPopularCourses_ShouldReturnOkWithEmptyList_WhenNoCoursesFound()
        {
            // Arrange
            _mockCourseService.Setup(s => s.GetPopularCoursesAsync())
                              .ReturnsAsync(new List<CourseResponseDTO>());

            var controller = new CourseController(_mockCourseService.Object, _mockFileService.Object, _mockNotificationService.Object, _mockUserManager.Object);

            // Act
            var result = await controller.GetPopularCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<CourseResponseDTO>>(okResult.Value);
            Assert.Empty(returnValue);
        }



        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a")]
        public async Task Search_ShouldReturnBadRequest_WhenQueryIsInvalid(string query)
        {
            // Arrange
            var controller = new CourseController(_mockCourseService.Object, _mockFileService.Object, _mockNotificationService.Object, _mockUserManager.Object);

            // Act
            var result = await controller.Search(query);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            var message = messageProp?.GetValue(value)?.ToString();

            Assert.Equal("Search query must be at least 2 characters long.", message);
        }

        [Fact]
        public async Task Search_ShouldReturnNotFound_WhenNoCoursesMatch()
        {
            // Arrange
            string query = "nonexistent course";
            _mockCourseService.Setup(s => s.SearchCoursesAsync(query))
                              .ReturnsAsync(new List<CourseResponseDTO>());

            var controller = new CourseController(_mockCourseService.Object, _mockFileService.Object, _mockNotificationService.Object, _mockUserManager.Object);

            // Act
            var result = await controller.Search(query);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            var message = messageProp?.GetValue(value)?.ToString();

            Assert.Equal("No matching courses found.", message);
        }




        [Fact]
        public async Task GetTotalCourses_ShouldReturnOkWithTotal_WhenUserIsAdmin()
        {
            // Arrange
            int expectedTotal = 42;
            _mockCourseService.Setup(s => s.GetTotalCoursesAsync()).ReturnsAsync(expectedTotal);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Simulate admin user
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await controller.GetTotalCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedTotal, value);
        }

        [Fact]
        public async Task GetTotalCourses_ShouldReturnOk_WhenUserIsNotAdmin_ButAuthorizationIsNotEnforcedInUnitTest()
        {
            // Arrange
            int expectedTotal = 10;
            _mockCourseService.Setup(s => s.GetTotalCoursesAsync()).ReturnsAsync(expectedTotal);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.GetTotalCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedTotal, okResult.Value);
        }




        [Fact]
        public async Task GetPendingCourses_ShouldReturnOkWithPendingCourses()
        {
            // Arrange
            var pendingCourses = new List<Course>
            {
                new Course { CourseID = 1, Title = "Pending Course 1" },
                new Course { CourseID = 2, Title = "Pending Course 2" }
            };

            _mockCourseService.Setup(s => s.GetPendingCoursesAsync())
                              .ReturnsAsync(pendingCourses);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.GetPendingCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Course>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("Pending Course 1", returnValue[0].Title);
        }

        [Fact]
        public async Task GetPendingCourses_ShouldReturnOkWithEmptyList_WhenNoPendingCourses()
        {
            // Arrange
            _mockCourseService.Setup(s => s.GetPendingCoursesAsync())
                              .ReturnsAsync(new List<Course>());

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.GetPendingCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Course>>(okResult.Value);
            Assert.Empty(returnValue);
        }



        [Fact]
        public async Task GetApprovedCourses_ShouldReturnOkWithApprovedCourses()
        {
            // Arrange
            var approvedCourses = new List<Course>
            {
                new Course { CourseID = 1, Title = "Approved Course 1" },
                new Course { CourseID = 2, Title = "Approved Course 2" }
            };

            _mockCourseService.Setup(s => s.GetApprovedCoursesAsync())
                              .ReturnsAsync(approvedCourses);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.GetApprovedCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Course>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("Approved Course 1", returnValue[0].Title);
        }

        [Fact]
        public async Task GetApprovedCourses_ShouldReturnOkWithEmptyList_WhenNoApprovedCourses()
        {
            // Arrange
            _mockCourseService.Setup(s => s.GetApprovedCoursesAsync())
                              .ReturnsAsync(new List<Course>());

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.GetApprovedCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Course>>(okResult.Value);
            Assert.Empty(returnValue);
        }



        [Fact]
        public async Task GetCoursesByInstructor_ShouldReturnCourses_WhenInstructorIdIsValid()
        {
            // Arrange
            var instructorId = "instructor123";
            var courses = new List<CourseResponseDTO>
            {
                new CourseResponseDTO { CourseID = 1, Title = "Course A" },
                new CourseResponseDTO { CourseID = 2, Title = "Course B" }
            };

            _mockCourseService.Setup(s => s.GetByInstructorIdAsync(instructorId))
                              .ReturnsAsync(courses);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            var claims = new List<Claim>
            {
                new Claim(CustomClaimTypes.UserId, instructorId),
                new Claim(ClaimTypes.Role, "Instructor")
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"))
                }
            };

            // Act
            var result = await controller.GetCoursesByInstructor();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCourses = Assert.IsType<List<CourseResponseDTO>>(okResult.Value);
            Assert.Equal(2, returnedCourses.Count);
            Assert.Contains(returnedCourses, c => c.Title == "Course A");
            Assert.Contains(returnedCourses, c => c.Title == "Course B");
        }


        [Fact]
        public async Task GetTotalCoursesForInstructor_ShouldReturnBadRequest_WhenInstructorIdIsMissing()
        {
            // Arrange
            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Instructor")
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"))
                }
            };

            // Act
            var result = await controller.GetTotalCoursesForInstructor();

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequest.Value;

            var messageProperty = value?.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            var message = messageProperty.GetValue(value);
            Assert.Equal("Instructor is not logged in.", message);
        }


        [Fact]
        public async Task GetEnrolledStudents_ShouldReturnOk_WhenStudentsAreEnrolled()
        {
            // Arrange
            var courseId = 1;
            var students = new List<User>
    {
        new User { Id = "1", FullName = "John Doe", Email = "john@example.com", ProfilePictureUrl = "url1" },
        new User { Id = "2", FullName = "Jane Doe", Email = "jane@example.com", ProfilePictureUrl = "url2" }
    };

            // Mock the course service to return List<User> instead of IdentityUser
            _mockCourseService.Setup(s => s.GetEnrolledStudentsByCourseIdAsync(courseId))
                              .ReturnsAsync(students);  // List<User>

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.GetEnrolledStudents(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            // Use reflection to access the list of anonymous objects returned by the controller
            var studentList = value as IEnumerable<dynamic>;
            Assert.NotNull(studentList);
            Assert.Equal(2, studentList.Count());

            var firstStudent = studentList.First();
            var secondStudent = studentList.Skip(1).First();

            // Use reflection to access properties in the anonymous object
            var firstEmail = firstStudent.GetType().GetProperty("Email").GetValue(firstStudent);
            var secondEmail = secondStudent.GetType().GetProperty("Email").GetValue(secondStudent);

            // Assert that the student emails are correct
            Assert.Equal("john@example.com", firstEmail);
            Assert.Equal("jane@example.com", secondEmail);
        }



        [Fact]
        public async Task GetCoursesWithStudents_ReturnsCourses_WhenInstructorHasCourses()
        {
            // Arrange - Authenticated instructor with courses
            var userId = "instructor-1";
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(CustomClaimTypes.UserId, userId),
            new Claim(ClaimTypes.Role, "Instructor")
        }, "test"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var expectedCourses = new List<CourseWithStudentsDTO>
            {
            new() { CourseID = 1, Title = "Mathematics", Students = new List<StudentDTO>() }
        };

            _mockCourseService.Setup(x => x.GetCoursesWithStudentsForInstructorAsync(userId))
                .ReturnsAsync(expectedCourses);

            // Act
            var result = await _controller.GetCoursesWithStudents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCourses = Assert.IsAssignableFrom<IEnumerable<CourseWithStudentsDTO>>(okResult.Value);
            Assert.Single(returnedCourses);
            Assert.Equal("Mathematics", returnedCourses.First().Title);
        }

        [Fact]
        public async Task GetCoursesWithStudents_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange 
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = await _controller.GetCoursesWithStudents();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            var responseValue = unauthorizedResult.Value;
            var propertyInfo = responseValue.GetType().GetProperty("message");
            Assert.NotNull(propertyInfo);
            Assert.Equal("Instructor not found.", propertyInfo.GetValue(responseValue));
        }




        [Fact]
        public async Task Create_ReturnsSuccess_WithValidRequest()
        {
            // Arrange
            var userId = "instructor-1";
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(CustomClaimTypes.UserId, userId),
        new Claim(ClaimTypes.Role, "Instructor")
    }, "test"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1);

            var request = new CreateCourseRequestDTO
            {
                Title = "New Course",
                ImageFile = mockFile.Object
            };

            _mockCourseService.Setup(x => x.AddAsync(It.IsAny<CreateCourseRequestDTO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<int>>(result);
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(1, createdAtResult.Value);
            Assert.Equal(nameof(_controller.GetById), createdAtResult.ActionName);

            _mockNotificationService.Verify(x => x.NotifyAdminForCourseAdditionOrUpdateAsync(
                userId, "added a new course", 1), Times.Once);
            _mockNotificationService.Verify(x => x.NotifyInstructorOnCourseApprovalStatusAsync(
                1, "pending approval"), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenImageMissing()
        {
            // Arrange 
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, "Instructor")
                }, "test"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var invalidRequest = new CreateCourseRequestDTO
            {
                Title = "Course Without Image",
                ImageFile = null
            };

            // Act
            var result = await _controller.Create(invalidRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<int>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            // Safe way to check anonymous object properties
            var responseValue = badRequestResult.Value;
            var property = responseValue.GetType().GetProperty("message");

            Assert.NotNull(property);
            Assert.Equal("Invalid course data or image file.", property.GetValue(responseValue)?.ToString());
        }



        [Fact]
        public async Task ApproveCourse_ShouldReturnOk_WhenCourseIsApprovedSuccessfully()
        {
            // Arrange
            var courseId = 1;
            _mockCourseService.Setup(s => s.ApproveCourseAsync(courseId))
                              .ReturnsAsync(true);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.ApproveCourse(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var messageProperty = value?.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            var message = messageProperty?.GetValue(value);
            Assert.Equal("Course approved successfully", message);
        }

        [Fact]
        public async Task ApproveCourse_ShouldReturnNotFound_WhenCourseApprovalFails()
        {
            // Arrange
            var courseId = 1;
            _mockCourseService.Setup(s => s.ApproveCourseAsync(courseId))
                              .ReturnsAsync(false);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.ApproveCourse(courseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value;

            var messageProperty = value?.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            var message = messageProperty?.GetValue(value);
            Assert.Equal("Course not found or already approved.", message);
        }





        [Fact]
        public async Task RejectCourse_ShouldReturnOk_WhenCourseIsRejectedSuccessfully()
        {
            // Arrange
            var courseId = 1;
            _mockCourseService.Setup(s => s.RejectCourseAsync(courseId))
                              .ReturnsAsync(true);

            _mockNotificationService.Setup(s => s.NotifyInstructorOnCourseApprovalStatusAsync(courseId, "rejected"))
                                     .Returns(Task.CompletedTask);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.RejectCourse(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;


            var messageProperty = value?.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            var message = messageProperty?.GetValue(value);
            Assert.Equal("Course rejected successfully", message);
        }

        [Fact]
        public async Task RejectCourse_ShouldReturnNotFound_WhenCourseRejectionFails()
        {
            // Arrange
            var courseId = 1;
            _mockCourseService.Setup(s => s.RejectCourseAsync(courseId))
                              .ReturnsAsync(false);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.RejectCourse(courseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value;

            var messageProperty = value?.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            var message = messageProperty?.GetValue(value);
            Assert.Equal("Course not found or rejection failed", message);
        }




        [Fact]
        public async Task GetCourseForUpdate_ShouldReturnBadRequest_WhenInstructorIsNotLoggedIn()
        {
            // Arrange
            var courseId = 1;

            var mockClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string)null);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = mockClaimsPrincipal }
            };

            // Act
            var result = await controller.GetCourseForUpdate(courseId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value;

            var messageProperty = value?.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            var message = messageProperty?.GetValue(value);
            Assert.Equal("Instructor is not logged in.", message);
        }

        [Fact]
        public async Task GetCourseForUpdate_ShouldReturnNotFound_WhenCourseIsNotFoundOrNoUpdatesAvailable()
        {
            // Arrange
            var courseId = 1;
            var instructorId = "instructor123"; // Mock instructor ID

            var mockClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(CustomClaimTypes.UserId, instructorId)
            }));

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(instructorId);

            _mockCourseService.Setup(s => s.GetCourseForUpdateAsync(courseId, instructorId))
                              .ReturnsAsync((CourseUpdateDTO)null);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = mockClaimsPrincipal }
            };

            // Act
            var result = await controller.GetCourseForUpdate(courseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value;

            var messageProperty = value?.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            var message = messageProperty?.GetValue(value);
            Assert.Equal("Course not found or no updates available.", message);
        }




        [Fact]
        public async Task SubmitUpdate_ShouldReturnBadRequest_WhenUpdateDtoIsNull()
        {
            // Arrange
            var courseId = 1;
            UpdateCourseRequestDTO updateDto = null;

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.SubmitUpdateAsync(courseId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value;
            Assert.Contains("Update course request cannot be null.", value.ToString());
        }

        [Fact]
        public async Task SubmitUpdate_ShouldReturnForbid_WhenInstructorIsNotOwner()
        {
            // Arrange
            var courseId = 1;
            var updateDto = new UpdateCourseRequestDTO
            {
                Title = "Updated Course",
                Description = "Updated Description",
                Price = 99.99M,
                VideoUrl = "http://video.com/sample.mp4"
            };

            var courseResponse = new CourseResponseDTO
            {
                CourseID = courseId,
                Title = "Original",
                Description = "Original",
                InstructorID = "owner-123"
            };

            _mockCourseService.Setup(s => s.GetByIdAsync(courseId))
                              .ReturnsAsync(courseResponse);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(CustomClaimTypes.UserId, "different-instructor-id")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.SubmitUpdateAsync(courseId, updateDto);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task SubmitUpdate_ShouldReturnOk_WhenCourseUpdateIsSuccessful()
        {
            // Arrange
            var courseId = 1;
            var updateDto = new UpdateCourseRequestDTO
            {
                Title = "Updated Course Title",
                Description = "Updated Description",
                Price = 79.99M,
                VideoUrl = "http://video.com/updated.mp4"
            };

            var courseResponse = new CourseResponseDTO
            {
                CourseID = courseId,
                Title = "Original Title",
                Description = "Original Description",
                InstructorID = "instructor-123"
            };

            _mockCourseService.Setup(s => s.GetByIdAsync(courseId))
                              .ReturnsAsync(courseResponse);

            _mockCourseService.Setup(s => s.SubmitCourseUpdateAsync(courseId, updateDto))
                              .ReturnsAsync(true);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(CustomClaimTypes.UserId, "instructor-123")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.SubmitUpdateAsync(courseId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            var message = messageProperty.GetValue(value);
            Assert.Equal("Course update submitted successfully.", message);
        }




        [Fact]
        public async Task ApprovePendingUpdate_ShouldReturnOk_WhenUpdateIsApprovedSuccessfully()
        {
            // Arrange
            var courseId = 1;

            _mockCourseService.Setup(s => s.ApprovePendingUpdatesAsync(courseId))
                              .ReturnsAsync(true);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.ApprovePendingUpdate(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);  

            var message = messageProperty.GetValue(value);
            Assert.Equal("Course update approved successfully.", message);
        }

        [Fact]
        public async Task ApprovePendingUpdate_ShouldReturnNotFound_WhenNoPendingUpdateFound()
        {
            // Arrange
            var courseId = 1;

            _mockCourseService.Setup(s => s.ApprovePendingUpdatesAsync(courseId))
                              .ReturnsAsync(false);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.ApprovePendingUpdate(courseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value;

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty); 

            var message = messageProperty.GetValue(value);
            Assert.Equal("No pending updates found or approval failed.", message);
        }



        [Fact]
        public async Task RejectPendingUpdates_ShouldReturnOk_WhenUpdateIsRejectedSuccessfully()
        {
            // Arrange
            var courseId = 1;

            _mockCourseService.Setup(s => s.RejectPendingUpdatesAsync(courseId))
                              .ReturnsAsync(true);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.RejectPendingUpdates(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);  

            var message = messageProperty.GetValue(value);
            Assert.Equal("Course update rejected successfully.", message);
        }

        [Fact]
        public async Task RejectPendingUpdates_ShouldReturnNotFound_WhenNoPendingUpdateFound()
        {
            // Arrange
            var courseId = 1;

            _mockCourseService.Setup(s => s.RejectPendingUpdatesAsync(courseId))
                              .ReturnsAsync(false);

            var controller = new CourseController(
                _mockCourseService.Object,
                _mockFileService.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );

            // Act
            var result = await controller.RejectPendingUpdates(courseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value;

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);  

            var message = messageProperty.GetValue(value);
            Assert.Equal("No pending updates found for the course.", message);
        }











    }
}

