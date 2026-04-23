import 'package:flutter/material.dart';
import '../features/auth/data/auth_service.dart';
import '../features/products/data/product_service.dart';
import '../features/orders/data/order_service.dart';

class DependencyProvider extends InheritedWidget {
  final IAuthService authService;
  final IProductService productService;
  final IOrderService orderService;

  const DependencyProvider({
    super.key,
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
      authService != oldWidget.authService ||
      productService != oldWidget.productService ||
      orderService != oldWidget.orderService;
}
