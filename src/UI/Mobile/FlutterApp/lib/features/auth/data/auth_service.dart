import 'package:dio/dio.dart';
import '../../../shared/models/api_models.dart';
import '../../../core/network/api_client.dart';

abstract class IAuthService {
  Future<AuthResponse?> login(LoginRequest request);
  Future<bool> register(RegisterRequest request);
  Future<bool> isAuthenticated();
  Future<void> logout();
}

class AuthService implements IAuthService {
  final ApiClient _client;

  AuthService(this._client);

  @override
  Future<AuthResponse?> login(LoginRequest request) async {
    try {
      final response = await _client.dio.post(
        '/api/auth/login',
        data: request.toJson(),
      );
      final result = AuthResponse.fromJson(response.data);
      await _client.auth.saveToken(result.accessToken);
      return result;
    } on DioException {
      return null;
    }
  }

  @override
  Future<bool> register(RegisterRequest request) async {
    try {
      await _client.dio.post('/api/auth/register', data: request.toJson());
      return true;
    } on DioException {
      return false;
    }
  }

  @override
  Future<bool> isAuthenticated() async {
    final token = await _client.auth.getToken();
    return token != null && token.isNotEmpty;
  }

  @override
  Future<void> logout() async {
    await _client.auth.clearToken();
  }
}
