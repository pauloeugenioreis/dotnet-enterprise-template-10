import '../../../core/network/api_client.dart';

class DocumentService {
  final ApiClient _client;

  DocumentService(this._client);

  Future<dynamic> getDocuments({int page = 1, int pageSize = 10}) async {
    final response = await _client.dio.get(
      '/api/documents',
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    return response.data;
  }
}
