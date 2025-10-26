import 'package:dio/dio.dart';
import '../constants/api_constants.dart';
import 'api_interceptor.dart';

/// Dio HTTP Client Configuration
class ApiClient {
  late final Dio _dio;

  ApiClient() {
    _dio = Dio(
      BaseOptions(
        baseUrl: ApiConstants.baseUrl,
        connectTimeout: ApiConstants.connectionTimeout,
        receiveTimeout: ApiConstants.receiveTimeout,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );

    // Add interceptors
    _dio.interceptors.add(ApiInterceptor());

    // Add logging in debug mode
    _dio.interceptors.add(
      LogInterceptor(
        requestBody: true,
        responseBody: true,
        requestHeader: true,
        responseHeader: false,
        error: true,
      ),
    );
  }

  /// Get Dio instance
  Dio get dio => _dio;

  /// GET request
  Future<Response> get(
    String path, {
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      final response = await _dio.get(
        path,
        queryParameters: queryParameters,
        options: options,
      );
      return response;
    } on DioException catch (e) {
      throw _handleError(e);
    }
  }

  /// POST request
  Future<Response> post(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      final response = await _dio.post(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
      return response;
    } on DioException catch (e) {
      throw _handleError(e);
    }
  }

  /// PUT request
  Future<Response> put(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      final response = await _dio.put(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
      return response;
    } on DioException catch (e) {
      throw _handleError(e);
    }
  }

  /// PATCH request
  Future<Response> patch(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      final response = await _dio.patch(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
      return response;
    } on DioException catch (e) {
      throw _handleError(e);
    }
  }

  /// DELETE request
  Future<Response> delete(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      final response = await _dio.delete(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
      return response;
    } on DioException catch (e) {
      throw _handleError(e);
    }
  }

  /// Handle Dio errors
  Exception _handleError(DioException error) {
    switch (error.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return NetworkException('Connection timeout. Please check your internet connection.');

      case DioExceptionType.badResponse:
        return _handleResponseError(error.response);

      case DioExceptionType.cancel:
        return NetworkException('Request cancelled.');

      case DioExceptionType.connectionError:
        return NetworkException('No internet connection. Please check your network.');

      case DioExceptionType.badCertificate:
        return NetworkException('Certificate verification failed.');

      case DioExceptionType.unknown:
        return NetworkException('An unexpected error occurred: ${error.message}');
    }
  }

  /// Handle HTTP response errors
  Exception _handleResponseError(Response? response) {
    if (response == null) {
      return NetworkException('No response from server.');
    }

    final statusCode = response.statusCode ?? 0;
    final message = response.data is Map
        ? response.data['message'] ?? response.data['error'] ?? response.statusMessage
        : response.statusMessage;

    switch (statusCode) {
      case 400:
        return BadRequestException(message ?? 'Bad request.');
      case 401:
        return UnauthorizedException(message ?? 'Unauthorized. Please login again.');
      case 403:
        return ForbiddenException(message ?? 'Access forbidden.');
      case 404:
        return NotFoundException(message ?? 'Resource not found.');
      case 409:
        return ConflictException(message ?? 'Resource conflict.');
      case 500:
      case 502:
      case 503:
        return ServerException(message ?? 'Server error. Please try again later.');
      default:
        return NetworkException(message ?? 'Something went wrong (Status: $statusCode).');
    }
  }
}

/// Base Network Exception
class NetworkException implements Exception {
  final String message;
  NetworkException(this.message);

  @override
  String toString() => message;
}

/// Bad Request (400)
class BadRequestException extends NetworkException {
  BadRequestException(super.message);
}

/// Unauthorized (401)
class UnauthorizedException extends NetworkException {
  UnauthorizedException(super.message);
}

/// Forbidden (403)
class ForbiddenException extends NetworkException {
  ForbiddenException(super.message);
}

/// Not Found (404)
class NotFoundException extends NetworkException {
  NotFoundException(super.message);
}

/// Conflict (409)
class ConflictException extends NetworkException {
  ConflictException(super.message);
}

/// Server Error (5xx)
class ServerException extends NetworkException {
  ServerException(super.message);
}
