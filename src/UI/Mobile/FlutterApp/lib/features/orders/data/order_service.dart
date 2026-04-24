import '../../../shared/models/api_models.dart';
import '../../../core/network/api_client.dart';

abstract class IOrderService {
  Future<PagedResponse<OrderResponse>> getOrders({
    int page = 1,
    int pageSize = 10,
    String? searchTerm,
    String? status,
    DateTime? fromDate,
    DateTime? toDate,
  });
  Future<OrderResponse> createOrder(Map<String, dynamic> orderData);
  Future<void> updateOrder(int id, Map<String, dynamic> orderData);
  Future<void> updateStatus(int id, String status, String reason);
  Future<OrderResponse> cancelOrder(int id, String reason);
  Future<OrderStatistics> getStatistics();
}

class OrderService implements IOrderService {
  final ApiClient _client;

  OrderService(this._client);

  @override
  Future<PagedResponse<OrderResponse>> getOrders({
    int page = 1,
    int pageSize = 10,
    String? searchTerm,
    String? status,
    DateTime? fromDate,
    DateTime? toDate,
  }) async {
    final response = await _client.dio.get(
      '/api/v1/Order',
      queryParameters: {
        'page': page,
        'pageSize': pageSize,
        if (searchTerm != null && searchTerm.isNotEmpty) 'searchTerm': searchTerm,
        if (status != null && status.isNotEmpty) 'status': status,
        if (fromDate != null) 'startDate': fromDate.toIso8601String(),
        if (toDate != null) 'endDate': toDate.toIso8601String(),
      },
    );
    final data = response.data as Map<String, dynamic>;
    return PagedResponse<OrderResponse>(
      items: (data['items'] as List?)
              ?.map((e) => OrderResponse.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      totalCount: data['totalCount'] as int? ?? 0,
      page: data['pageNumber'] as int? ?? data['page'] as int? ?? 1,
      pageSize: data['pageSize'] as int? ?? 10,
      totalPages: data['totalPages'] as int? ?? 1,
      hasNextPage: data['hasNextPage'] as bool? ?? false,
      hasPreviousPage: data['hasPreviousPage'] as bool? ?? false,
    );
  }

  @override
  Future<OrderResponse> createOrder(Map<String, dynamic> orderData) async {
    final response = await _client.dio.post('/api/v1/Order', data: orderData);
    return OrderResponse.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<void> updateOrder(int id, Map<String, dynamic> orderData) async {
    await _client.dio.put('/api/v1/Order/$id', data: orderData);
  }

  @override
  Future<void> updateStatus(int id, String status, String reason) async {
    await _client.dio.patch('/api/v1/Order/$id/status', data: {
      'status': status,
      'reason': reason,
    });
  }

  @override
  Future<OrderResponse> cancelOrder(int id, String reason) async {
    final response = await _client.dio.post('/api/v1/Order/$id/cancel', data: '"$reason"');
    return OrderResponse.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<OrderStatistics> getStatistics() async {
    final response = await _client.dio.get('/api/v1/Order/statistics');
    return OrderStatistics.fromJson(response.data as Map<String, dynamic>);
  }
}
