import 'package:flutter/material.dart';
import 'app_navigation.dart';
import 'services/auth_service.dart';
import 'screens/login_screen.dart';
import 'screens/pos_screen.dart';

void main() {
  runApp(const PosApp());
}

class PosApp extends StatelessWidget {
  const PosApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'POS Mobile',
      navigatorKey: appNavigatorKey,
      theme: ThemeData(useMaterial3: true, colorSchemeSeed: Colors.indigo),
      home: const _StartupGate(),
    );
  }
}

/// Routes to POS if a token is already stored, otherwise Login.
class _StartupGate extends StatelessWidget {
  const _StartupGate();

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<bool>(
      future: AuthService().isLoggedIn(),
      builder: (context, snapshot) {
        if (!snapshot.hasData) {
          return const Scaffold(body: Center(child: CircularProgressIndicator()));
        }
        return snapshot.data! ? const PosScreen() : const LoginScreen();
      },
    );
  }
}
