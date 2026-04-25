import 'dart:io';
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'auth_interceptor.dart';

/// Fábrica de clientes HTTP configurados com interceptors.
/// Equivalente ao AddHttpClient("ApiGateway").AddHttpMessageHandler<AuthTokenHandler>()
class ApiClient {
  static String get baseUrl {
    if (Platform.isAndroid) return 'http://10.0.2.2:5000'; // HTTP para evitar erros de SSL em dev
    return 'http://localhost:5000';
  }

  final Dio _dio;
  final AuthInterceptor _authInterceptor;

  ApiClient({
    required FlutterSecureStorage storage,
    String? customBaseUrl,
  })  : _authInterceptor = AuthInterceptor(storage),
        _dio = Dio(BaseOptions(
          baseUrl: customBaseUrl ?? baseUrl,
          connectTimeout: const Duration(seconds: 15),
          receiveTimeout: const Duration(seconds: 30),
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
          },
        )) {
    _dio.interceptors.add(_authInterceptor);
    _dio.interceptors.add(LogInterceptor(
      requestBody: true,
      responseBody: true,
    ));
  }

  Dio get dio => _dio;
  AuthInterceptor get auth => _authInterceptor;
}
