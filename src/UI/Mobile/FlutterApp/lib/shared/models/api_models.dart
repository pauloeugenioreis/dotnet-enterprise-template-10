/// Modelos Dart que espelham os DTOs do src/Shared/SharedModels.
/// Mantém paridade com: LoginRequestDto, AuthResponseDto, OrderResponseDto, etc.

// ─── Auth ────────────────────────────────────────────────────────

class LoginRequest {
  final String email;
  final String password;

  const LoginRequest({required this.email, required this.password});

  Map<String, dynamic> toJson() => {'email': email, 'password': password};
}

class RegisterRequest {
  final String name;
  final String email;
  final String password;

  const RegisterRequest({
    required this.name,
    required this.email,
    required this.password,
  });

  Map<String, dynamic> toJson() => {
        'name': name,
        'email': email,
        'password': password,
      };
}

class AuthResponse {
  final String token;
  final String userId;
  final String email;
  final String name;

  const AuthResponse({
    required this.token,
    required this.userId,
    required this.email,
    required this.name,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) => AuthResponse(
        token: json['token'] as String,
        userId: json['userId'] as String,
        email: json['email'] as String,
        name: json['name'] as String,
      );
}

// ─── Orders ──────────────────────────────────────────────────────

class OrderResponse {
  final String id;
  final String orderNumber;
  final String customerName;
  final String customerEmail;
  final String status;
  final double total;
  final double subtotal;
  final double shippingCost;
  final double tax;
  final String shippingAddress;
  final DateTime createdAt;
  final List<OrderItemResponse> items;

  const OrderResponse({
    required this.id,
    required this.orderNumber,
    required this.customerName,
    required this.customerEmail,
    required this.status,
    required this.total,
    required this.subtotal,
    required this.shippingCost,
    required this.tax,
    required this.shippingAddress,
    required this.createdAt,
    required this.items,
  });

  factory OrderResponse.fromJson(Map<String, dynamic> json) => OrderResponse(
        id: json['id'] as String,
        orderNumber: json['orderNumber'] as String,
        customerName: json['customerName'] as String,
        customerEmail: json['customerEmail'] as String,
        status: json['status'] as String,
        total: (json['total'] as num).toDouble(),
        subtotal: (json['subtotal'] as num).toDouble(),
        shippingCost: (json['shippingCost'] as num).toDouble(),
        tax: (json['tax'] as num).toDouble(),
        shippingAddress: json['shippingAddress'] as String,
        createdAt: DateTime.parse(json['createdAt'] as String),
        items: (json['items'] as List<dynamic>)
            .map((e) => OrderItemResponse.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

class OrderItemResponse {
  final String productId;
  final String productName;
  final int quantity;
  final double unitPrice;
  final double subtotal;

  const OrderItemResponse({
    required this.productId,
    required this.productName,
    required this.quantity,
    required this.unitPrice,
    required this.subtotal,
  });

  factory OrderItemResponse.fromJson(Map<String, dynamic> json) =>
      OrderItemResponse(
        productId: json['productId'] as String,
        productName: json['productName'] as String,
        quantity: json['quantity'] as int,
        unitPrice: (json['unitPrice'] as num).toDouble(),
        subtotal: (json['subtotal'] as num).toDouble(),
      );
}

class PagedResponse<T> {
  final List<T> items;
  final int totalCount;
  final int page;
  final int pageSize;
  final int totalPages;
  final bool hasNextPage;
  final bool hasPreviousPage;

  const PagedResponse({
    required this.items,
    required this.totalCount,
    required this.page,
    required this.pageSize,
    required this.totalPages,
    required this.hasNextPage,
    required this.hasPreviousPage,
  });
}

// ─── Dashboard Stats ─────────────────────────────────────────────

class TopProduct {
  final int productId;
  final String productName;
  final int quantitySold;
  final double revenue;

  const TopProduct({
    required this.productId,
    required this.productName,
    required this.quantitySold,
    required this.revenue,
  });

  factory TopProduct.fromJson(Map<String, dynamic> json) => TopProduct(
        productId: json['productId'] as int,
        productName: json['productName'] as String,
        quantitySold: json['quantitySold'] as int,
        revenue: (json['revenue'] as num).toDouble(),
      );
}

class OrderStatistics {
  final int totalOrders;
  final double totalRevenue;
  final double averageOrderValue;
  final List<TopProduct> topProducts;

  const OrderStatistics({
    required this.totalOrders,
    required this.totalRevenue,
    required this.averageOrderValue,
    required this.topProducts,
  });

  factory OrderStatistics.fromJson(Map<String, dynamic> json) =>
      OrderStatistics(
        totalOrders: json['totalOrders'] as int,
        totalRevenue: (json['totalRevenue'] as num).toDouble(),
        averageOrderValue: (json['averageOrderValue'] as num).toDouble(),
        topProducts: (json['topProducts'] as List<dynamic>)
            .map((e) => TopProduct.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}
