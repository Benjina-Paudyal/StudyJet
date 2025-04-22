using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;

namespace StudyJet.API.Data
{
    public class DbInitializer
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public DbInitializer(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            // Seed roles and users
            await SeedUsersRolesAsync();

            // Seed categories
            await SeedCategoriesAsync();

            // Seed courses
            await SeedCoursesAsync();

        }

        private async Task SeedUsersRolesAsync()
        {
            try
            {
                // Exit if users already exist
                if (_userManager.Users.Any()) return;

                // Create roles if they don't exist
                string[] roles = { "Admin", "Instructor", "Student" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Creating default users
                var users = new List<User>
                {
                    new User { UserName = "admin", FullName="Benjina Paudyal", Email = "admin@example.com", ProfilePictureUrl = "/images/profiles/admin.jpg",EmailConfirmed = true,},
                    new User { UserName = "gergely_orosz",FullName="Gergely Orosz", Email = "instructor1@example.com", ProfilePictureUrl = "/images/profiles/instructor1.jpg",EmailConfirmed = true,},
                    new User { UserName = "wes_bos", FullName = "Wes Bos", Email = "instructor2@example.com", ProfilePictureUrl = "/images/profiles/instructor2.jpg",EmailConfirmed = true,},
                    new User { UserName = "addy_osmani",FullName = "Addy Osmani" ,Email = "instructor3@example.com", ProfilePictureUrl = "/images/profiles/instructor3.jpg",EmailConfirmed = true,},
                    new User { UserName = "dan_abramov", FullName = "Dan Abramov", Email = "instructor4@example.com", ProfilePictureUrl = "/images/profiles/instructor4.jpg", EmailConfirmed = true},
                    new User { UserName = "sarah_drasner", FullName = "Sarah Drasner", Email = "instructor5@example.com", ProfilePictureUrl = "/images/profiles/instructor5.jpg",EmailConfirmed = true,},
                    new User { UserName = "bhaskar_khanal", FullName="Bhaskar Khanal", Email = "student1@example.com", ProfilePictureUrl = "/images/profiles/student1.jpg", EmailConfirmed = true},
                    new User { UserName = "sunniva_khanal", FullName = "Sunniva Khanal", Email = "student2@example.com", ProfilePictureUrl = "/images/profiles/student2.jpg",EmailConfirmed = true,},
                    new User { UserName = "sharvin_khanal", FullName="Sharvin Khanal",Email = "student3@example.com", ProfilePictureUrl = "/images/profiles/student3.jpg", EmailConfirmed = true},
                    new User { UserName = "madhav_paudyal", FullName="Madhav Paudyal", Email = "student4@example.com", ProfilePictureUrl = "/images/profiles/student4.jpg", EmailConfirmed = true},
                    new User { UserName = "buna_dahal", FullName="Buna Dahal",Email = "student5@example.com", ProfilePictureUrl = "/images/profiles/student5.jpg", EmailConfirmed = true},
                };

                foreach (var user in users)
                {
                    if (await _userManager.FindByEmailAsync(user.Email) == null)
                    {
                        var defaultPassword = Environment.GetEnvironmentVariable("DEFAULT_PASSWORD") ?? throw new Exception("DEFAULT_PASSWORD not set.");
                        Console.WriteLine($"Default password is: {defaultPassword}");

                        var result = await _userManager.CreateAsync(user, defaultPassword);
                        if (result.Succeeded)
                        {
                            string role = user.Email.Contains("instructor") ? "Instructor" : "Student";
                            if (user.Email == "admin@example.com")
                                role = "Admin";

                            await _userManager.AddToRoleAsync(user, role);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Failed to create user {user.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred during user/role seeding: {ex.Message}", ex);
            }
        }

        public async Task<List<Category>> SeedCategoriesAsync()
        {
            if (_context.Categories.Any()) return _context.Categories.ToList();

            try
            {
                Console.WriteLine("Seeding categories...");

                var categories = new List<Category>
        {
            new Category { Name = "Programming Languages" },
            new Category { Name = "Web Development" },
            new Category { Name = "Mobile Development" },
            new Category { Name = "Game Development" }
        };

                _context.Categories.AddRange(categories);

                await _context.SaveChangesAsync();

                return categories;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"An error occurred while saving the entity changes: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while seeding categories: {ex.Message}");
                throw;
            }
        }

        public async Task SeedCoursesAsync()
        {
            // Skip seeding if courses already exist
            if (_context.Courses.Any()) return;


            try
            {
                // Fetch and validate categories
                var categories = await SeedCategoriesAsync();


                // Fetch the Category IDs based on category names
                var programmingCategory = categories.FirstOrDefault(c => c.Name == "Programming Languages")?.CategoryID
                          ?? throw new Exception("Programming Languages category not found.");
                var webDevCategory = categories.FirstOrDefault(c => c.Name == "Web Development")?.CategoryID
                                     ?? throw new Exception("Web Development category not found.");
                var mobileDevCategory = categories.FirstOrDefault(c => c.Name == "Mobile Development")?.CategoryID
                                        ?? throw new Exception("Mobile Development category not found.");
                var gameDevCategory = categories.FirstOrDefault(c => c.Name == "Game Development")?.CategoryID
                                      ?? throw new Exception("Game Development category not found.");

                // Fetch and validate instructors
                var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
                if (instructors.Count < 5)
                {
                    throw new Exception("There are not enough instructors seeded to assign courses.");
                }

                // Create and add courses
                var courses = new List<Course>
                {
                    new Course
                    {
                        Title = "Complete C#",
                        Description = "A comprehensive course designed to take learners from beginner to expert in C#. This course covers the fundamentals of C# programming, including syntax, control structures, and data types, progressing to advanced topics such as object-oriented programming, LINQ, and asynchronous operations. Students will gain hands-on experience through practical projects, preparing them for real-world application development.",
                        ImageUrl = "/images/courses/Csharp.png",
                        Price = 599.99m,
                        InstructorID = instructors[0].Id,
                        CategoryID = programmingCategory,
                        CreationDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                        VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                        IsPopular = true,
                        Status = CourseStatus.Approved
                    },

                    new Course
                    {
                        Title = "Angular",
                        Description = "Master Angular from the ground up! This comprehensive course takes you through the core concepts of Angular, from setting up your development environment to building complex, scalable web applications. Learn about components, directives, services, routing, and more, with hands-on examples and real-world projects to solidify your understanding. Perfect for beginners and developers looking to enhance their front-end development skills.",
                        ImageUrl = "/images/courses/Angular.png",
                        Price = 399.99m,
                        InstructorID = instructors[0].Id,
                        CategoryID = webDevCategory ,
                        CreationDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                        VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                        IsPopular = true,
                        Status = CourseStatus.Approved
                    },

                    new Course
                    {

                        Title = "React",
                        Description = "\"Master React from the basics to advanced concepts! This comprehensive course covers everything you need to know to become proficient in React development. Start by setting up your development environment and get hands-on experience building dynamic and interactive user interfaces. Learn about components, props, state management, hooks, routing, and more, with practical examples and real-world projects. Ideal for beginners and developers.",
                        ImageUrl = "/images/courses/React.png",
                        Price = 599.99m,
                        InstructorID = instructors[0].Id,
                        CategoryID = webDevCategory ,
                        CreationDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                        VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                        IsPopular = false,
                        Status = CourseStatus.Approved
                    },

                    new Course
                    {

                        Title = "HTML/CSS/JS",
                        Description = "Build the foundation of web development with this in-depth course on HTML, CSS, and JavaScript. Start with the basics of structuring web pages using HTML, style them with CSS for a responsive and attractive design, and add interactivity with JavaScript. Ideal for beginners, this course provides practical examples and projects to help you create dynamic, user-friendly websites from scratch.",
                        ImageUrl = "/images/courses/HtmlCssJs.png",
                        Price = 299.99m,
                        InstructorID = instructors[1].Id,
                        CategoryID = webDevCategory ,
                        CreationDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                        VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                        IsPopular = true,
                        Status = CourseStatus.Approved

                     },

                     new Course
                     {

                         Title = "Python",
                         Description = "Embark on a journey to master Python, one of the most versatile and popular programming languages. This course covers everything from the fundamentals of Python syntax to advanced topics like data analysis, web development, and automation. Through hands-on exercises and real-world projects, you'll gain practical experience and build a strong foundation in Python, empowering you to tackle a wide range of programming challenges and applications.",
                         ImageUrl = "/images/courses/Python.png",
                         Price = 399.99m,
                         InstructorID = instructors[1].Id,
                         CategoryID = programmingCategory,
                         CreationDate = DateTime.UtcNow,
                         LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                         VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                         IsPopular = true,
                         Status = CourseStatus.Approved

                     },

                     new Course
                     {

                          Title = "JAVA",
                          Description = "Unlock the power of Java with this comprehensive course designed for both beginners and experienced developers. Start with the essentials of Java programming, including object-oriented concepts, syntax, and core libraries. Progress to advanced topics such as multithreading, networking, and database integration. With practical exercises and real-world projects, you'll develop a deep understanding of Java and its applications, preparing you for robust software development and enterprise solutions.",
                          ImageUrl = "/images/courses/Java.png",
                          Price = 599.99m,
                          InstructorID = instructors[2].Id,
                          CategoryID = programmingCategory,
                          CreationDate = DateTime.UtcNow,
                          LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                          VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                          IsPopular = true,
                          Status = CourseStatus.Approved

                     },

                     new Course
                     {

                           Title = "Unity",
                           Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                           ImageUrl = "/images/courses/Unity.png",
                           Price = 499.99m,
                           InstructorID = instructors[2].Id,
                           CategoryID =  gameDevCategory,
                           CreationDate = DateTime.UtcNow,
                           LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                           VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                           IsPopular = false,
                           Status = CourseStatus.Approved
                     },

                     new Course
                     {

                           Title = "2D Game",
                           Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                           ImageUrl = "/images/courses/Default.png",
                           Price = 499.99m,
                           InstructorID = instructors[3].Id,
                           CategoryID =  gameDevCategory,
                           CreationDate = DateTime.UtcNow,
                           LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                           VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                           IsPopular = false,
                           Status = CourseStatus.Approved

                     },

                     new Course
                     {

                           Title = "3D Game",
                           Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                           ImageUrl = "/images/courses/Default.png",
                           Price = 399.99m,
                           InstructorID = instructors[3].Id,
                           CategoryID =  gameDevCategory,
                           CreationDate = DateTime.UtcNow,
                           LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                           VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                           IsPopular = false,
                           Status = CourseStatus.Approved

                     },

                     new Course
                     {

                           Title = "Android",
                           Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                           ImageUrl = "/images/courses/Default.png",
                           Price = 699.99m,
                           InstructorID = instructors[4].Id,
                           CategoryID = mobileDevCategory,
                           CreationDate = DateTime.UtcNow,
                           LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                           VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                           IsPopular = false,
                           Status = CourseStatus.Approved

                     },

                     new Course
                     {

                            Title = "iOS",
                            Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                            ImageUrl = "/images/courses/Default.png",
                            Price = 699.99m,
                            InstructorID = instructors[4].Id,
                            CategoryID = mobileDevCategory,
                            CreationDate = DateTime.UtcNow,
                            LastUpdatedDate = DateTime.UtcNow.AddDays(45),
                            VideoUrl = "https://www.youtube.com/watch?v=49HmQ5DYVjU",
                            IsPopular = false,
                            Status = CourseStatus.Approved
                     }

                };

                // adding and saving courses
                _context.Courses.AddRange(courses);
                await _context.SaveChangesAsync();
            }

            catch (Exception ex)

            {
                throw new Exception($"An error occurred while seeding courses: {ex.Message}", ex);
            }
        }


    }
}

