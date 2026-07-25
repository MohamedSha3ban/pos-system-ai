import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../app_navigation.dart';
import '../screens/login_screen.dart';

/// Central HTTP client: attaches the short-lived JWT access token to every request and
/// points at POS.Gateway.Mobile. Handles token refresh transparently -- a 401 triggers one
/// silent call to /auth/refresh and a single retry of the original request; if the refresh
/// token is also dead, the session is cleared and the app is bounced to the login screen.
class ApiClient {
  // TODO: swap for the deployed gateway URL (or 10.0.2.2 for Android emulator -> localhost).
  static const String baseUrl = 'https://localhost:5003/api';

  Future<String?> _accessToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('pos_access_token');
  }

  Future<String?> _refreshToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('pos_refresh_token');
  }

  Map<String, String> _headers(String? token) => {
        'Content-Type': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      };

  Future<dynamic> get(String path, {Map<String, String>? query}) => _send('GET', path, query: query);
  Future<dynamic> post(String path, Map<String, dynamic>? body) => _send('POST', path, body: body);
  Future<dynamic> put(String path, Map<String, dynamic> body, {Map<String, String>? query}) =>
      _send('PUT', path, body: body, query: query);
  Future<dynamic> patch(String path, {Map<String, String>? query}) => _send('PATCH', path, query: query);
  Future<dynamic> delete(String path) => _send('DELETE', path);

  Future<dynamic> _send(
    String method,
    String path, {
    Map<String, dynamic>? body,
    Map<String, String>? query,
    bool isRetryAfterRefresh = false,
  }) async {
    final uri = Uri.parse('$baseUrl$path').replace(queryParameters: query);
    final headers = _headers(await _accessToken());
    final encodedBody = body != null ? jsonEncode(body) : null;

    final http.Response res;
    switch (method) {
      case 'GET':
        res = await http.get(uri, headers: headers);
        break;
      case 'POST':
        res = await http.post(uri, headers: headers, body: encodedBody);
        break;
      case 'PUT':
        res = await http.put(uri, headers: headers, body: encodedBody);
        break;
      case 'PATCH':
        res = await http.patch(uri, headers: headers);
        break;
      case 'DELETE':
        res = await http.delete(uri, headers: headers);
        break;
      default:
        throw ArgumentError('Unsupported method: $method');
    }

    // /auth/* endpoints returning 401 means bad credentials or a dead refresh token --
    // never attempt to refresh-and-retry those (would loop or make no sense).
    final isAuthEndpoint = path.startsWith('/auth/');

    if (res.statusCode == 401 && !isRetryAfterRefresh && !isAuthEndpoint) {
      final refreshed = await _tryRefresh();
      if (refreshed) {
        return _send(method, path, body: body, query: query, isRetryAfterRefresh: true);
      }
      await _clearSessionAndRedirectToLogin();
      throw Exception('Session expired -- please log in again.');
    }

    return _handle(res);
  }

  Future<bool> _tryRefresh() async {
    final refreshToken = await _refreshToken();
    if (refreshToken == null) return false;

    final uri = Uri.parse('$baseUrl/auth/refresh');
    final res = await http.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'refreshToken': refreshToken}),
    );

    if (res.statusCode < 200 || res.statusCode >= 300) return false;

    final data = jsonDecode(res.body);
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('pos_access_token', data['accessToken']);
    await prefs.setString('pos_refresh_token', data['refreshToken']);
    return true;
  }

  Future<void> _clearSessionAndRedirectToLogin() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('pos_access_token');
    await prefs.remove('pos_refresh_token');
    await prefs.remove('pos_tenant_id');

    appNavigatorKey.currentState?.pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (route) => false,
    );
  }

  dynamic _handle(http.Response res) {
    if (res.statusCode >= 200 && res.statusCode < 300) {
      return res.body.isEmpty ? null : jsonDecode(res.body);
    }
    throw Exception('API error ${res.statusCode}: ${res.body}');
  }
}
