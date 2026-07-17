namespace POS.Infrastructure.Modules.Payments;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; set; } = default!;
    public string PublishableKey { get; set; } = default!;
    public string WebhookSecret { get; set; } = default!;
}
