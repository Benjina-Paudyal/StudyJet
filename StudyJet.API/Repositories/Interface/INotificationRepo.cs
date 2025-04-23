using StudyJet.API.Data.Entities;

namespace StudyJet.API.Repositories.Interface
{
    public interface INotificationRepo
    {
        Task CreateAsync(Notification notification);    
        Task MarkAsReadAsync(int notificationId);      
        Task<Notification> SelectByIdAsync(int notificationId);  
        Task UpdateAsync(Notification notification);   
        Task<List<Notification>> SelectByUserIdAsync(string userId, int page = 1, int pageSize = 10); 
    }
}
