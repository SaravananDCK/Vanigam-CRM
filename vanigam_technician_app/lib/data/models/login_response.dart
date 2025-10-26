import 'package:json_annotation/json_annotation.dart';
import 'user.dart';

part 'login_response.g.dart';

/// Login response model from API
@JsonSerializable()
class LoginResponse {
  final String? token;
  final User? user;
  final String? message;
  final bool? success;

  const LoginResponse({
    this.token,
    this.user,
    this.message,
    this.success,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) =>
      _$LoginResponseFromJson(json);
  Map<String, dynamic> toJson() => _$LoginResponseToJson(this);
}
