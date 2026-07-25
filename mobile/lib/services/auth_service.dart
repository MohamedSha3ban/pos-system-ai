import 'package:shared_preferences/shared_preferences.dart';
import 'api_client.dart';

class AuthService {
  final ApiClient _client = ApiClient();

  Future<bool> login(String email, String password) async {
    final data = await _client.post('/auth/login', {
      'email': email,
      'password': password,
    });
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('pos_access_token', data['accessToken']);
    await prefs.setString('pos_refresh_token', data['refreshToken']);
    await prefs.setString('pos_tenant_id', data['tenantId']);
    return true;
  }

  Future<bool> isLoggedIn() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('pos_access_token') != null;
  }

  /// Revokes the refresh token server-side (best-effort -- a network hiccup shouldn't trap
  /// the user logged in on this device), then clears local storage regardless.
  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    final refreshToken = prefs.getString('pos_refresh_token');

    if (refreshToken != null) {
      try {
        await _client.post('/auth/logout', {'refreshToken': refreshToken});
      } catch (_) {
        // best-effort; fall through to clearing local storage either way
      }
    }

    await prefs.remove('pos_access_token');
    await prefs.remove('pos_refresh_token');
    await prefs.remove('pos_tenant_id');
  }
}
