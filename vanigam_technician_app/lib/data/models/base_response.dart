import 'package:json_annotation/json_annotation.dart';

part 'base_response.g.dart';

/// Base Response Model for API responses
@JsonSerializable(genericArgumentFactories: true)
class BaseResponse<T> {
  final bool success;
  final String? message;
  final T? data;
  final ErrorResponse? error;

  BaseResponse({
    required this.success,
    this.message,
    this.data,
    this.error,
  });

  factory BaseResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Object? json) fromJsonT,
  ) =>
      _$BaseResponseFromJson(json, fromJsonT);

  Map<String, dynamic> toJson(Object Function(T value) toJsonT) =>
      _$BaseResponseToJson(this, toJsonT);
}

/// Error Response Model
@JsonSerializable()
class ErrorResponse {
  final String? message;
  final String? code;
  final Map<String, dynamic>? details;

  ErrorResponse({
    this.message,
    this.code,
    this.details,
  });

  factory ErrorResponse.fromJson(Map<String, dynamic> json) =>
      _$ErrorResponseFromJson(json);

  Map<String, dynamic> toJson() => _$ErrorResponseToJson(this);
}

/// OData Response Model (for OData API calls)
@JsonSerializable(genericArgumentFactories: true)
class ODataResponse<T> {
  @JsonKey(name: '@odata.context')
  final String? context;

  @JsonKey(name: '@odata.count')
  final int? count;

  @JsonKey(name: 'value')
  final List<T>? value;

  ODataResponse({
    this.context,
    this.count,
    this.value,
  });

  factory ODataResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Object? json) fromJsonT,
  ) =>
      _$ODataResponseFromJson(json, fromJsonT);

  Map<String, dynamic> toJson(Object Function(T value) toJsonT) =>
      _$ODataResponseToJson(this, toJsonT);
}

/// Pagination Info
@JsonSerializable()
class PaginationInfo {
  final int currentPage;
  final int pageSize;
  final int totalCount;
  final int totalPages;
  final bool hasNextPage;
  final bool hasPreviousPage;

  PaginationInfo({
    required this.currentPage,
    required this.pageSize,
    required this.totalCount,
    required this.totalPages,
    required this.hasNextPage,
    required this.hasPreviousPage,
  });

  factory PaginationInfo.fromJson(Map<String, dynamic> json) =>
      _$PaginationInfoFromJson(json);

  Map<String, dynamic> toJson() => _$PaginationInfoToJson(this);

  /// Calculate pagination info from count and page params
  factory PaginationInfo.calculate({
    required int currentPage,
    required int pageSize,
    required int totalCount,
  }) {
    final totalPages = (totalCount / pageSize).ceil();
    return PaginationInfo(
      currentPage: currentPage,
      pageSize: pageSize,
      totalCount: totalCount,
      totalPages: totalPages,
      hasNextPage: currentPage < totalPages,
      hasPreviousPage: currentPage > 1,
    );
  }
}
