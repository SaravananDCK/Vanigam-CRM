/// Secure Storage and SharedPreferences Keys
class StorageKeys {
  StorageKeys._();

  // Secure Storage Keys (flutter_secure_storage)
  static const String authToken = 'auth_token';
  static const String refreshToken = 'refresh_token';
  static const String userId = 'user_id';
  static const String userName = 'user_name';
  static const String userEmail = 'user_email';
  static const String tenantId = 'tenant_id';
  static const String userType = 'user_type';

  // Shared Preferences Keys
  static const String isLoggedIn = 'is_logged_in';
  static const String rememberMe = 'remember_me';
  static const String savedUsername = 'saved_username';
  static const String biometricEnabled = 'biometric_enabled';
  static const String lastSyncTime = 'last_sync_time';
  static const String isDarkMode = 'is_dark_mode';
  static const String notificationsEnabled = 'notifications_enabled';
  static const String locationTrackingEnabled = 'location_tracking_enabled';
}
