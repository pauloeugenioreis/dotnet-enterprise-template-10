import '../../../core/network/api_client.dart';
import '../../../shared/models/api_models.dart';

abstract class IProductService {
  Future<PagedResponse<OrderResponse>> getProducts({int page = 1, int pageSize = 10});
}

class ProductService {
  final ApiClient _client;

  ProductService(this._client);

  Future<PagedResponse<OrderResponse>> getProducts({int page = 1, int pageSize = 10}) async {
    final response = await _client.dio.get(
      '/api/products',
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    // Nota: O mapeamento real usaria ProductResponse.fromJson, 
    // mas aqui seguimos a estrutura do PagedResponse.
    return PagedResponse<OrderResponse>(
      items: (response.data['items'] as List)
          .map((e) => OrderResponse.fromJson(e as Map<String, dynamic>))
          .toList(),
      totalCount: response.data['totalCount'],
      page: response.data['page'],
      pageSize: response.data['pageSize'],
      totalPages: response.data['totalPages'],
      hasNextPage: response.data['hasNextPage'],
      hasPreviousPage: response.data['hasPreviousPage'],
    );
  }
}
