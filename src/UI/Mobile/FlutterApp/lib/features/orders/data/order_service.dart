import '../../../shared/models/api_models.dart';
import '../../../core/network/api_client.dart';

abstract class IOrderService {
  Future<PagedResponse<OrderResponse>> getOrders({int page = 1, int pageSize = 10});
  Future<OrderStatistics> getStatistics();
}

class OrderService implements IOrderService {
  final ApiClient _client;

  OrderService(this._client);

  @override
  Future<PagedResponse<OrderResponse>> getOrders({int page = 1, int pageSize = 10}) async {
    final response = await _client.dio.get(
      '/api/orders',
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    final data = response.data as Map<String, dynamic>;
    return PagedResponse<OrderResponse>(
      items: (data['items'] as List)
          .map((e) => OrderResponse.fromJson(e as Map<String, dynamic>))
          .toList(),
      totalCount: data['totalCount'] as int,
      page: data['page'] as int,
      pageSize: data['pageSize'] as int,
      totalPages: data['totalPages'] as int,
      hasNextPage: data['hasNextPage'] as bool,
      hasPreviousPage: data['hasPreviousPage'] as bool,
    );
  }

  @override
  Future<OrderStatistics> getStatistics() async {
    final response = await _client.dio.get('/api/orders/statistics');
    return OrderStatistics.fromJson(response.data as Map<String, dynamic>);
  }
}
