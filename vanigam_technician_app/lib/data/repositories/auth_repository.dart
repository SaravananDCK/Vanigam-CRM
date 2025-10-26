import '../../core/network/api_client.dart';
import '../../core/constants/api_constants.dart';
import '../../core/constants/storage_keys.dart';
import '../models/login_request.dart';
import '../models/login_response.dart';
import '../models/user.dart';
import '../services/storage_service.dart';

/// Repository for authentication operations
class AuthRepository {
  final ApiClient _apiClient;
  final StorageService _storageService;

  AuthRepository({
    required ApiClient apiClient,
    required StorageService storageService,
  })  : _apiClient = apiClient,
        _storageService = storageService;

  /// Login with username and password
  Future<LoginResponse> login({
    required String userName,
    required String password,
  }) async {
    try {
      final request = LoginRequest(
        userName: userName,
        password: password,
      );

      final response = await _apiClient.post(
        ApiConstants.authenticateToken,
        data: request.toJson(),
      );

      final loginResponse = LoginResponse.fromJson(response.data);

      // Save token and user info if login successful
      if (loginResponse.success == true && loginResponse.token != null) {
        await _saveAuthData(loginResponse);
      }

      return loginResponse;
    } catch (e) {
      rethrow;
    }
  }

  /// Save authentication data to storage
  Future<void> _saveAuthData(LoginResponse loginResponse) async {
    if (loginResponse.token != null) {
      await _storageService.saveSecureData(
        StorageKeys.authToken,
        loginResponse.token!,
      );
    }

    if (loginResponse.user != null) {
      // Save user data
      await _storageService.saveString(
        StorageKeys.userId,
        loginResponse.user!.id,
      );

      await _storageService.saveString(
        StorageKeys.userEmail,
        loginResponse.user!.email,
      );

      if (loginResponse.user!.tenantId != null) {
        await _storageService.saveSecureData(
          StorageKeys.tenantId,
          loginResponse.user!.tenantId!,
        );
      }

      if (loginResponse.user!.fullName != null) {
        await _storageService.saveString(
          StorageKeys.userName,
          loginResponse.user!.fullName!,
        );
      }
    }

    // Mark as logged in
    await _storageService.saveBool(StorageKeys.isLoggedIn, true);
  }

  /// Logout and clear all stored data
  Future<void> logout() async {
    await _storageService.deleteSecureData(StorageKeys.authToken);
    await _storageService.deleteSecureData(StorageKeys.tenantId);
    await _storageService.remove(StorageKeys.userId);
    await _storageService.remove(StorageKeys.userEmail);
    await _storageService.remove(StorageKeys.userName);
    await _storageService.saveBool(StorageKeys.isLoggedIn, false);
  }

  /// Check if user is logged in
  Future<bool> isLoggedIn() async {
    final token = await _storageService.getSecureData(StorageKeys.authToken);
    final isLoggedIn = await _storageService.getBool(StorageKeys.isLoggedIn);
    return token != null && token.isNotEmpty && (isLoggedIn ?? false);
  }

  /// Get saved user data
  Future<User?> getSavedUser() async {
    final userId = await _storageService.getString(StorageKeys.userId);
    final userEmail = await _storageService.getString(StorageKeys.userEmail);
    final userName = await _storageService.getString(StorageKeys.userName);
    final tenantId = await _storageService.getSecureData(StorageKeys.tenantId);

    if (userId != null && userEmail != null) {
      return User(
        id: userId,
        email: userEmail,
        fullName: userName,
        tenantId: tenantId,
      );
    }

    return null;
  }

  /// Get saved auth token
  Future<String?> getAuthToken() async {
    return await _storageService.getSecureData(StorageKeys.authToken);
  }
}
