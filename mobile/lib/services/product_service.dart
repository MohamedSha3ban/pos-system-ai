import '../models/product.dart';
import 'api_client.dart';

class ProductService {
  final ApiClient _client = ApiClient();

  Future<List<Product>> getCatalog(String locationId) async {
    final data = await _client.get('/products', query: {'locationId': locationId});
    return (data as List).map((e) => Product.fromJson(e)).toList();
  }
}
