using Microsoft.AspNetCore.Identity.UI.Services;

namespace HotelHub.Project.Models
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email,
            string subject, 
            string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }
}
