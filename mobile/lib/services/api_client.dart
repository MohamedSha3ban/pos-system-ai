import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

/// Central HTTP client: attaches the JWT (from AuthService) to every request
/// and points at the same POS.API backend the Angular web app talks to.
class ApiClient {
  // TODO: swap for the deployed API URL (or 10.0.2.2 for Android emulator -> localhost).
  static const String baseUrl = 'https://localhost:5001/api';

  Future<String?> _token() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('pos_token');
  }

  Future<Map<String, String>> _headers() async {
    final token = await _token();
    return {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  Future<dynamic> get(String path, {Map<String, String>? query}) async {
    final uri = Uri.parse('$baseUrl$path').replace(queryParameters: query);
    final res = await http.get(uri, headers: await _headers());
    return _handle(res);
  }

  Future<dynamic> post(String path, Map<String, dynamic>? body) async {
    final uri = Uri.parse('$baseUrl$path');
    final res = await http.post(uri, headers: await _headers(), body: body != null ? jsonEncode(body) : null);
    return _handle(res);
  }

  Future<dynamic> put(String path, Map<String, dynamic> body, {Map<String, String>? query}) async {
    final uri = Uri.parse('$baseUrl$path').replace(queryParameters: query);
    final res = await http.put(uri, headers: await _headers(), body: jsonEncode(body));
    return _handle(res);
  }

  Future<dynamic> patch(String path, {Map<String, String>? query}) async {
    final uri = Uri.parse('$baseUrl$path').replace(queryParameters: query);
    final res = await http.patch(uri, headers: await _headers());
    return _handle(res);
  }

  Future<dynamic> delete(String path) async {
    final uri = Uri.parse('$baseUrl$path');
    final res = await http.delete(uri, headers: await _headers());
    return _handle(res);
  }

  dynamic _handle(http.Response res) {
    if (res.statusCode >= 200 && res.statusCode < 300) {
      return res.body.isEmpty ? null : jsonDecode(res.body);
    }
    throw Exception('API error ${res.statusCode}: ${res.body}');
  }
}
