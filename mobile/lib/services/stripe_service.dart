import 'api_client.dart';

class StripeIntent {
  final String clientSecret;
  final String paymentIntentId;
  StripeIntent({required this.clientSecret, required this.paymentIntentId});

  factory StripeIntent.fromJson(Map<String, dynamic> json) => StripeIntent(
        clientSecret: json['clientSecret'],
        paymentIntentId: json['paymentIntentId'],
      );
}

/// Calls the backend to create a Stripe PaymentIntent. The clientSecret is what you'd
/// pass to the flutter_stripe SDK to collect card details and confirm on-device --
/// see README "Next steps" for wiring that up.
class StripeService {
  final ApiClient _client = ApiClient();

  Future<StripeIntent> createIntent(double amount, {String currency = 'usd'}) async {
    final data = await _client.post('/payments/stripe/create-intent', {
      'amount': amount,
      'currency': currency,
    });
    return StripeIntent.fromJson(data);
  }
}
