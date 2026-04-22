import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'core/network/api_client.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/data/auth_service.dart';
import 'features/auth/presentation/login_page.dart';
import 'features/dashboard/presentation/dashboard_page.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // ─── Inicialização das Dependências ──────────────────────────
  const storage = FlutterSecureStorage();
  final apiClient = ApiClient(storage: storage);
  final authService = AuthService(apiClient);

  // Verificar se já está autenticado
  final isLoggedIn = await authService.isAuthenticated();

  runApp(
    AuthServiceProvider(
      service: authService,
      child: MyApp(initialPage: isLoggedIn ? const DashboardPage() : const LoginPage()),
    ),
  );
}

class MyApp extends StatelessWidget {
  final Widget initialPage;

  const MyApp({super.key, required this.initialPage});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Enterprise Template',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.lightTheme,
      home: initialPage,
    );
  }
}
