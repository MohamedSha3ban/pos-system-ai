import '../models/category.dart';
import 'api_client.dart';

class CategoryService {
  final ApiClient _client = ApiClient();

  Future<List<Category>> getAll() async {
    final data = await _client.get('/categories');
    return (data as List).map((e) => Category.fromJson(e)).toList();
  }

  Future<Category> create(String name) async {
    final data = await _client.post('/categories', {'name': name});
    return Category.fromJson(data);
  }

  Future<Category> update(String id, String name) async {
    final data = await _client.put('/categories/$id', {'name': name});
    return Category.fromJson(data);
  }

  Future<void> delete(String id) async {
    await _client.delete('/categories/$id');
  }
}
