import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Interceptor HTTP global.
/// Equivalente ao AuthTokenHandler do MauiApp e LoadingHandler do BlazorWebApp.
/// Injeta automaticamente o JWT em todas as requisições autenticadas.
class AuthInterceptor extends Interceptor {
  final FlutterSecureStorage _storage;

  static const _tokenKey = 'auth_token';

  AuthInterceptor(this._storage);

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await _storage.read(key: _tokenKey);

    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }

    handler.next(options);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (err.response?.statusCode == 401) {
      // Token expirado → tratar na camada de estado
      _storage.delete(key: _tokenKey);
    }
    handler.next(err);
  }

  // ─── Token management ────────────────────────────────────────────
  Future<void> saveToken(String token) async {
    await _storage.write(key: _tokenKey, value: token);
  }

  Future<String?> getToken() async {
    return await _storage.read(key: _tokenKey);
  }

  Future<void> clearToken() async {
    await _storage.delete(key: _tokenKey);
  }
}
