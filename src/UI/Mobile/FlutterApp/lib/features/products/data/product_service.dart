import '../../../core/network/api_client.dart';
import '../../../shared/models/api_models.dart';

abstract class IProductService {
  Future<PagedResponse<ProductResponse>> getProducts({
    int page = 1,
    int pageSize = 10,
    String? searchTerm,
    bool? isActive,
  });
}

class ProductService implements IProductService {
  final ApiClient _client;

  ProductService(this._client);

  @override
  Future<PagedResponse<ProductResponse>> getProducts({
    int page = 1,
    int pageSize = 10,
    String? searchTerm,
    bool? isActive,
  }) async {
    final response = await _client.dio.get(
      '/api/products',
      queryParameters: {
        'page': page,
        'pageSize': pageSize,
        if (searchTerm != null && searchTerm.isNotEmpty) 'searchTerm': searchTerm,
        if (isActive != null) 'isActive': isActive,
      },
    );

    return PagedResponse<ProductResponse>(
      items: (response.data['items'] as List)
          .map((e) => ProductResponse.fromJson(e as Map<String, dynamic>))
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
