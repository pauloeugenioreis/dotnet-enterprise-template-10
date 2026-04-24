import 'package:flutter/material.dart';
import '../core/network/api_client.dart';
import '../features/auth/data/auth_service.dart';
import '../features/products/data/product_service.dart';
import '../features/orders/data/order_service.dart';

class DependencyProvider extends InheritedWidget {
  final ApiClient apiClient;
  final IAuthService authService;
  final IProductService productService;
  final IOrderService orderService;

  const DependencyProvider({
    super.key,
    required this.apiClient,
    required this.authService,
    required this.productService,
    required this.orderService,
    required super.child,
  });

  static DependencyProvider of(BuildContext context) {
    return context.dependOnInheritedWidgetOfExactType<DependencyProvider>()!;
  }

  @override
  bool updateShouldNotify(DependencyProvider oldWidget) =>
      apiClient != oldWidget.apiClient ||
      authService != oldWidget.authService ||
      productService != oldWidget.productService ||
      orderService != oldWidget.orderService;
}
