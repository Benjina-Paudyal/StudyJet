using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Repositories.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.RepositoryTests
{
    public class UserRepoTest
    {

        private readonly ApplicationDbContext _context;
        private readonly UserRepo _userRepo;

        public UserRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _userRepo = new UserRepo(_context);


        }


        [Fact]
        public async Task SelectByEmailAsync_ReturnsUser_WhenEmailExists()
        {
            // Arrange
            var email = "testuser@example.com";
            var user = new User
            {
                UserName = email,
                Email = email,
                FullName = "Test User"
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.SelectByEmailAsync(email);

            // Assert
            Assert.NotNull(result); 
            Assert.Equal(email, result.Email); 
        }

        [Fact]
        public async Task SelectByEmailAsync_ReturnsNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = "nonexistentuser@example.com";

            // Act
            var result = await _userRepo.SelectByEmailAsync(email);

            // Assert
            Assert.Null(result); 
        }



        [Fact]
        public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
        {
            // Arrange
            var existingEmail = "test@example.com";
            _context.Users.Add(new User
            {
                Email = existingEmail,
                UserName = existingEmail,
                FullName = "Test User" 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.EmailExistsAsync(existingEmail);

            // Assert
            Assert.True(result);
        }


        [Fact]
        public async Task EmailExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            // Arrange
            var nonExistingEmail = "nonexistent@example.com";
            _context.Users.Add(new User
            {
                Email = "other@example.com",
                UserName = "other@example.com",
                FullName = "Other User" 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.EmailExistsAsync(nonExistingEmail);

            // Assert
            Assert.False(result);
        }



        [Fact]
        public async Task UsernameExistsAsync_ReturnsTrue_WhenUsernameExists()
        {
            // Arrange
            var username = "existinguser";
            var user = new User
            {
                UserName = username,
                Email = "existinguser@example.com",
                FullName = "Existing User"
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.UsernameExistsAsync(username);

            // Assert
            Assert.True(result); 
        }

        [Fact]
        public async Task UsernameExistsAsync_ReturnsFalse_WhenUsernameDoesNotExist()
        {
            // Arrange
            var username = "nonexistentuser";

            // Act
            var result = await _userRepo.UsernameExistsAsync(username);

            // Assert
            Assert.False(result); 
        }



        [Fact]
        public async Task SelectUsersByRoleAsync_ReturnsUsers_WhenRoleExists()
        {
            // Arrange
            var role = "Admin";
            var user1 = new User
            {
                UserName = "admin1",
                Email = "admin1@example.com",
                FullName = "Admin One"
            };
            var user2 = new User
            {
                UserName = "admin2",
                Email = "admin2@example.com",
                FullName = "Admin Two"
            };

            var roleEntity = new IdentityRole { Name = role };
            var roleId = roleEntity.Id;

            var userRole1 = new IdentityUserRole<string> { UserId = user1.Id, RoleId = roleId };
            var userRole2 = new IdentityUserRole<string> { UserId = user2.Id, RoleId = roleId };

            await _context.Users.AddRangeAsync(user1, user2);
            await _context.Roles.AddAsync(roleEntity);
            await _context.UserRoles.AddRangeAsync(userRole1, userRole2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.SelectUsersByRoleAsync(role);

            // Assert
            Assert.NotNull(result); 
            Assert.Equal(2, result.Count); 
            Assert.All(result, u => Assert.Contains(role, u.Roles)); 
        }

        [Fact]
        public async Task SelectUsersByRoleAsync_ReturnsEmptyList_WhenNoUsersWithRole()
        {
            // Arrange
            var role = "NonExistentRole";

            // Act
            var result = await _userRepo.SelectUsersByRoleAsync(role);

            // Assert
            Assert.NotNull(result); 
            Assert.Empty(result); 
        }



        [Fact]
        public async Task CountUsersByRoleAsync_ShouldReturnCorrectCount_WhenRoleExists()
        {
            // Arrange
            var testRole = "Student";
            var testUsers = new List<User>
            {
                new User { Email = "student1@test.com", UserName = "student1", FullName = "Student One" },
                new User { Email = "student2@test.com", UserName = "student2", FullName = "Student Two" },
                new User { Email = "teacher@test.com", UserName = "teacher", FullName = "Teacher One" }
            };

            var testRoles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Student", NormalizedName = "STUDENT" },
                new IdentityRole { Name = "Teacher", NormalizedName = "TEACHER" }
            };

            var testUserRoles = new List<IdentityUserRole<string>>
            {
                new IdentityUserRole<string> { UserId = testUsers[0].Id, RoleId = testRoles[0].Id },
                 new IdentityUserRole<string> { UserId = testUsers[1].Id, RoleId = testRoles[0].Id },
                new IdentityUserRole<string> { UserId = testUsers[2].Id, RoleId = testRoles[1].Id }
            };

            await _context.Users.AddRangeAsync(testUsers);
            await _context.Roles.AddRangeAsync(testRoles);
            await _context.UserRoles.AddRangeAsync(testUserRoles);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.CountUsersByRoleAsync(testRole);

            // Assert
            Assert.Equal(2, result); 
        }


        [Fact]
        public async Task CountUsersByRoleAsync_ShouldReturnZero_WhenRoleDoesNotExist()
        {
            // Arrange
            var nonExistentRole = "Admin";
            var testUsers = new List<User>
            {
                new User { Email = "user1@test.com", UserName = "user1", FullName = "User One" }
            };

            var testRoles = new List<IdentityRole>
            {
                new IdentityRole { Name = "User", NormalizedName = "USER" }
            };

            await _context.Users.AddRangeAsync(testUsers);
            await _context.Roles.AddRangeAsync(testRoles);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.CountUsersByRoleAsync(nonExistentRole);

            // Assert
            Assert.Equal(0, result);
        }


    }
}
