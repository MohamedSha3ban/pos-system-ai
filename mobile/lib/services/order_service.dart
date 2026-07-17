import '../models/cart_line.dart';
import '../models/order.dart';
import 'api_client.dart';

class OrderService {
  final ApiClient _client = ApiClient();

  Future<OrderResult> checkout({
    required String locationId,
    required List<CartLine> cart,
    required String paymentMethod,
    required double amount,
    String? paymentToken,
  }) async {
    final body = {
      'locationId': locationId,
      'customerId': null,
      'items': cart
          .map((c) => {
                'productId': c.product.id,
                'quantity': c.quantity,
                'lineDiscount': 0,
              })
          .toList(),
      'tenders': [
        {'method': paymentMethod, 'amount': amount, 'paymentToken': paymentToken}
      ],
      'tipTotal': 0,
    };
    final data = await _client.post('/orders/checkout', body);
    return OrderResult.fromJson(data);
  }
}
