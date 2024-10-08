using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Sciencetopia.Services;

public class SmsSender : ISmsSender
{
    private readonly IConfiguration _configuration;

    public SmsSender(IConfiguration configuration)
    {
        _configuration = configuration;
        TwilioClient.Init(
            _configuration["Twilio:AccountSid"],
            _configuration["Twilio:AuthToken"]
        );
    }

    public async Task SendSmsAsync(string number, string message)
    {
        var from = new PhoneNumber(_configuration["Twilio:FromPhoneNumber"]);
        var to = new PhoneNumber(number);

        var messageOptions = new CreateMessageOptions(to)
        {
            From = from,
            Body = message
        };

        await MessageResource.CreateAsync(messageOptions);
    }
}
