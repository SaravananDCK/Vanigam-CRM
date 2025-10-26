/// Application-wide Constants
class AppConstants {
  AppConstants._();

  // App Info
  static const String appName = 'Vanigam Technician';
  static const String appVersion = '1.0.0';

  // Storage Keys - defined in storage_keys.dart

  // Date Formats
  static const String dateFormat = 'yyyy-MM-dd';
  static const String timeFormat = 'HH:mm';
  static const String dateTimeFormat = 'yyyy-MM-dd HH:mm';
  static const String displayDateFormat = 'MMM dd, yyyy';
  static const String displayDateTimeFormat = 'MMM dd, yyyy hh:mm a';

  // UI Constants
  static const double defaultPadding = 16.0;
  static const double smallPadding = 8.0;
  static const double largePadding = 24.0;
  static const double borderRadius = 8.0;
  static const double cardElevation = 2.0;

  // Animation Durations
  static const Duration shortAnimation = Duration(milliseconds: 200);
  static const Duration mediumAnimation = Duration(milliseconds: 300);
  static const Duration longAnimation = Duration(milliseconds: 500);

  // Retry Configuration
  static const int maxRetryAttempts = 3;
  static const Duration retryDelay = Duration(seconds: 2);

  // Location Update Configuration
  static const Duration locationUpdateInterval = Duration(seconds: 30);
  static const double locationDistanceFilter = 10.0; // meters

  // Timer Configuration
  static const Duration timerUpdateInterval = Duration(seconds: 1);

  // Google Maps
  static const String googleMapsApiKey =
      String.fromEnvironment('GOOGLE_MAPS_API_KEY', defaultValue: '');
  static const double defaultMapZoom = 15.0;
  static const double detailMapZoom = 17.0;
}
