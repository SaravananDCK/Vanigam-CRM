import 'package:json_annotation/json_annotation.dart';

part 'user.g.dart';

/// User model representing authenticated user
@JsonSerializable()
class User {
  final String id;
  final String email;
  final String? userName;
  final String? fullName;
  final String? phoneNumber;
  final String? tenantId;
  final List<String>? roles;

  const User({
    required this.id,
    required this.email,
    this.userName,
    this.fullName,
    this.phoneNumber,
    this.tenantId,
    this.roles,
  });

  factory User.fromJson(Map<String, dynamic> json) => _$UserFromJson(json);
  Map<String, dynamic> toJson() => _$UserToJson(this);

  User copyWith({
    String? id,
    String? email,
    String? userName,
    String? fullName,
    String? phoneNumber,
    String? tenantId,
    List<String>? roles,
  }) {
    return User(
      id: id ?? this.id,
      email: email ?? this.email,
      userName: userName ?? this.userName,
      fullName: fullName ?? this.fullName,
      phoneNumber: phoneNumber ?? this.phoneNumber,
      tenantId: tenantId ?? this.tenantId,
      roles: roles ?? this.roles,
    );
  }
}
