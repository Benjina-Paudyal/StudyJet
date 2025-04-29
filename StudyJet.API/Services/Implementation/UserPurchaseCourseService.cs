using Stripe.Checkout;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Services.Implementation
{
    public class UserPurchaseCourseService : IUserPurchaseCourseService
    {
        private readonly IUserPurchaseCourseRepo _userPurchaseCourseRepo;
        private readonly ICourseRepo _courseRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserPurchaseCourseService> _logger;

        public UserPurchaseCourseService(IUserPurchaseCourseRepo userPurchaseCourseRepo, ICourseRepo courseRepo, IConfiguration configuration, ILogger<UserPurchaseCourseService> logger)
        {
            _userPurchaseCourseRepo = userPurchaseCourseRepo;
            _courseRepo = courseRepo;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<UserPurchaseCourseDTO>> GetPurchasedCoursesAsync(string userId)
        {
            return await _userPurchaseCourseRepo.SelectPurchasedCourseAsync(userId);
        }

        public async Task<List<CourseResponseDTO>> GetSuggestedCoursesAsync(string userId)
        {
            return await _userPurchaseCourseRepo.SelectSuggestedCoursesAsync(userId);
        }

        public async Task<bool> PurchaseCourseAsync(string userId, List<int> courseIds)
        {
            var user = await _userPurchaseCourseRepo.SelectUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var alreadyPurchasedCourseIds = await _userPurchaseCourseRepo.SelectPurchasedCourseIdsAsync(userId);
            var newCourseIds = courseIds.Where(courseId => !alreadyPurchasedCourseIds.Contains(courseId)).ToList();

            if (!newCourseIds.Any())
            {
                return false; 
            }

            var newPurchases = newCourseIds.Select(courseId => new UserPurchaseCourse
            {
                UserID = user.Id,
                UserName = user.UserName,
                CourseID = courseId,
                PurchaseDate = DateTime.UtcNow
            }).ToList();

            // Add the new purchases to the database
            foreach (var purchase in newPurchases)
            {
                await _userPurchaseCourseRepo.InsertPurchaseAsync(purchase); // adds to context
            }
            await _userPurchaseCourseRepo.SaveChangesAsync(); // saves all at once
            return true;
        }

        public async Task<string> CreateCheckoutSession(string userId, List<int> courseIds)
        {
            var courses = await _courseRepo.SelectByIdsAsync(courseIds);
            if (courses == null || !courses.Any()) return null;

            try
            {
                // Preparing the session options for Stripe
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = $"{_configuration["AppUrls:SuccessUrl"]}?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{_configuration["AppUrls:CancelUrl"]}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", userId },
                        { "courseIds", string.Join(",", courseIds) }
                    }
                };

                // Add each course as a line item in the session
                foreach (var course in courses)
                {
                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "dkk",
                            UnitAmount = (long)(course.Price * 100), 
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = course.Title,
                                Description = course.Description
                            }
                        },
                        Quantity = 1 
                    });
                }

                // Stripe session
                var service = new SessionService();
                Session session = await service.CreateAsync(options);
                
                return session?.Url ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe checkout session for userId: {UserId}", userId);
                return null;
            }
        }



    }
}
