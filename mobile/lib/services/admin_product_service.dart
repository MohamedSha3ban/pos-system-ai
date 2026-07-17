import '../models/admin_product.dart';
import 'api_client.dart';

class AdminProductService {
  final ApiClient _client = ApiClient();

  Future<List<AdminProduct>> getCatalog(String locationId) async {
    final data = await _client.get('/products', query: {'locationId': locationId});
    return (data as List).map((e) => AdminProduct.fromJson(e)).toList();
  }

  Future<AdminProduct> create(Map<String, dynamic> product, String locationId, int initialQuantity) async {
    final data = await _client.post('/products', {
      'product': product,
      'locationId': locationId,
      'initialQuantity': initialQuantity,
    });
    return AdminProduct.fromJson(data);
  }

  Future<AdminProduct> update(String id, Map<String, dynamic> product, String locationId) async {
    final data = await _client.put('/products/$id', product, query: {'locationId': locationId});
    return AdminProduct.fromJson(data);
  }

  Future<void> delete(String id) async {
    await _client.delete('/products/$id');
  }
}
