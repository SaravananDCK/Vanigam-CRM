import 'package:json_annotation/json_annotation.dart';

part 'login_request.g.dart';

/// Login request model
@JsonSerializable()
class LoginRequest {
  final String userName;
  final String password;

  const LoginRequest({
    required this.userName,
    required this.password,
  });

  factory LoginRequest.fromJson(Map<String, dynamic> json) =>
      _$LoginRequestFromJson(json);
  Map<String, dynamic> toJson() => _$LoginRequestToJson(this);
}
