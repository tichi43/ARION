using ARION.Models;
using Azure;
using Azure.Communication.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ARION.Components.Account;

internal sealed class AzureEmailSender : IEmailSender<IdentityUser>
{
    private readonly EmailClient _emailClient;
    private readonly string _fromAddress;

    public AzureEmailSender(EmailClient emailClient, IConfiguration configuration)
    {
        _emailClient = emailClient;
        _fromAddress = configuration["Email:FromAddress"]
            ?? throw new InvalidOperationException("Missing configuration value: Email:FromAddress");
    }

    public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink) =>
        SendAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink) =>
        SendAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode) =>
        SendAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");

    private async Task SendAsync(string toAddress, string subject, string htmlBody)
    {
        var message = new EmailMessage(
            senderAddress: _fromAddress,
            recipientAddress: toAddress,
            content: new EmailContent(subject) { Html = htmlBody });

        // WaitUntil.Completed ensures the send operation is accepted by the service
        try
        {
            var result = await _emailClient.SendAsync(
                Azure.WaitUntil.Completed,
                message);
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Status: {ex.Status}");
            Console.WriteLine(ex.Message);
        }
    }
}