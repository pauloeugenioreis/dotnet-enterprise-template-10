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
      '/api/v1/Product',
      queryParameters: {
        'page': page,
        'pageSize': pageSize,
        if (searchTerm != null && searchTerm.isNotEmpty) 'searchTerm': searchTerm,
        if (isActive != null) 'isActive': isActive,
      },
    );

    final data = response.data as Map<String, dynamic>;
    return PagedResponse<ProductResponse>(
      items: (data['items'] as List?)
              ?.map((e) => ProductResponse.fromJson(e as Map<String, dynamic>))
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
}
