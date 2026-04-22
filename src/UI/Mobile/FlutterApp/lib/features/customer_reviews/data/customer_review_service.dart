import '../../../core/network/api_client.dart';

class CustomerReviewService {
  final ApiClient _client;

  CustomerReviewService(this._client);

  Future<dynamic> getReviews({int page = 1, int pageSize = 10}) async {
    final response = await _client.dio.get(
      '/api/customer-reviews',
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    return response.data;
  }
}
