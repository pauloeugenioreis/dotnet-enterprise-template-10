import '../../../core/network/api_client.dart';

class AuditService {
  final ApiClient _client;

  AuditService(this._client);

  Future<dynamic> getAuditLogs({required String entityType, int page = 1, int pageSize = 10}) async {
    final response = await _client.dio.get(
      '/api/v1/audit/$entityType',
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    return response.data;
  }
}
