import 'package:dio/dio.dart';
import '../../data/services/storage_service.dart';
import '../constants/storage_keys.dart';

/// API Interceptor for handling authentication tokens
class ApiInterceptor extends Interceptor {
  final StorageService _storageService = StorageService();

  @override
  void onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    // Add JWT token to headers if available
    final token = await _storageService.getSecureData(StorageKeys.authToken);
    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }

    // Add tenant ID if available
    final tenantId = await _storageService.getSecureData(StorageKeys.tenantId);
    if (tenantId != null && tenantId.isNotEmpty) {
      options.headers['X-Tenant-Id'] = tenantId;
    }

    super.onRequest(options, handler);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    // Handle successful responses
    super.onResponse(response, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    // Handle 401 Unauthorized - token expired
    if (err.response?.statusCode == 401) {
      // TODO: Implement token refresh logic here
      // For now, just clear tokens and force re-login
      await _storageService.deleteSecureData(StorageKeys.authToken);
      await _storageService.deleteSecureData(StorageKeys.refreshToken);
    }

    super.onError(err, handler);
  }
}
