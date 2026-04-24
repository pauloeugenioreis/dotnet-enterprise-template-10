import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'core/network/api_client.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/data/auth_service.dart';
import 'features/auth/presentation/login_page.dart';
import 'features/dashboard/presentation/dashboard_page.dart';
import 'features/products/data/product_service.dart';
import 'features/orders/data/order_service.dart';
import 'shared/dependency_provider.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // ─── Dependências ──────────────────────────────────────────────
  const storage = FlutterSecureStorage();
  final apiClient = ApiClient(storage: storage);
  
  final authService = AuthService(apiClient);
  final productService = ProductService(apiClient);
  final orderService = OrderService(apiClient);

  final isLoggedIn = await authService.isAuthenticated();

  runApp(
    DependencyProvider(
      apiClient: apiClient,
      authService: authService,
      productService: productService,
      orderService: orderService,
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
