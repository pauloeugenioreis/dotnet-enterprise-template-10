/// Modelos Dart que espelham os DTOs do src/Shared/Shared.Models.
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
  final String accessToken;
  final String refreshToken;
  final int userId;
  final String email;
  final String name;

  const AuthResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.userId,
    required this.email,
    required this.name,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    final userData = json['user'] as Map<String, dynamic>;
    return AuthResponse(
      accessToken: json['accessToken'] as String,
      refreshToken: json['refreshToken'] as String,
      userId: userData['id'] as int,
      email: userData['email'] as String,
      name: userData['fullName'] as String,
    );
  }
}

// ─── Orders ──────────────────────────────────────────────────────

class OrderResponse {
  final int id;
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
        id: json['id'] as int? ?? 0,
        orderNumber: json['orderNumber'] as String? ?? 'N/A',
        customerName: json['customerName'] as String? ?? 'Cliente',
        customerEmail: json['customerEmail'] as String? ?? '',
        status: json['status'] as String? ?? 'Pending',
        total: (json['total'] as num?)?.toDouble() ?? 0.0,
        subtotal: (json['subtotal'] as num?)?.toDouble() ?? 0.0,
        shippingCost: (json['shippingCost'] as num?)?.toDouble() ?? 0.0,
        tax: (json['tax'] as num?)?.toDouble() ?? 0.0,
        shippingAddress: json['shippingAddress'] as String? ?? '',
        createdAt: json['createdAt'] != null
            ? DateTime.parse(json['createdAt'] as String)
            : DateTime.now(),
        items: (json['items'] as List<dynamic>?)
                ?.map((e) => OrderItemResponse.fromJson(e as Map<String, dynamic>))
                .toList() ??
            [],
      );
}

class OrderItemResponse {
  final int productId;
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
        productId: json['productId'] as int? ?? 0,
        productName: json['productName'] as String? ?? 'Produto',
        quantity: json['quantity'] as int? ?? 0,
        unitPrice: (json['unitPrice'] as num?)?.toDouble() ?? 0.0,
        subtotal: (json['subtotal'] as num?)?.toDouble() ?? 0.0,
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

class ProductResponse {
  final int id;
  final String name;
  final String? description;
  final double price;
  final int stock;
  final String category;
  final bool isActive;

  const ProductResponse({
    required this.id,
    required this.name,
    this.description,
    required this.price,
    required this.stock,
    required this.category,
    required this.isActive,
  });

  factory ProductResponse.fromJson(Map<String, dynamic> json) => ProductResponse(
        id: json['id'] as int? ?? 0,
        name: json['name'] as String? ?? 'Produto Sem Nome',
        description: json['description'] as String?,
        price: (json['price'] as num?)?.toDouble() ?? 0.0,
        stock: json['stock'] as int? ?? 0,
        category: json['category'] as String? ?? 'Geral',
        isActive: json['isActive'] as bool? ?? true,
      );
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
        productId: json['productId'] as int? ?? 0,
        productName: json['productName'] as String? ?? 'Desconhecido',
        quantitySold: json['quantitySold'] as int? ?? 0,
        revenue: (json['revenue'] as num?)?.toDouble() ?? 0.0,
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
        totalOrders: json['totalOrders'] as int? ?? 0,
        totalRevenue: (json['totalRevenue'] as num?)?.toDouble() ?? 0.0,
        averageOrderValue: (json['averageOrderValue'] as num?)?.toDouble() ?? 0.0,
        topProducts: (json['topProducts'] as List<dynamic>?)
                ?.map((e) => TopProduct.fromJson(e as Map<String, dynamic>))
                .toList() ??
            [],
      );
}
