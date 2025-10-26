import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Storage Service for secure storage and shared preferences
class StorageService {
  // Singleton pattern
  static final StorageService _instance = StorageService._internal();
  factory StorageService() => _instance;
  StorageService._internal();

  // Secure storage instance
  final FlutterSecureStorage _secureStorage = const FlutterSecureStorage(
    aOptions: AndroidOptions(
      encryptedSharedPreferences: true,
    ),
    iOptions: IOSOptions(
      accessibility: KeychainAccessibility.first_unlock,
    ),
  );

  // Shared preferences instance (lazy initialized)
  SharedPreferences? _prefs;

  /// Initialize shared preferences
  Future<void> init() async {
    _prefs ??= await SharedPreferences.getInstance();
  }

  // ====================
  // Secure Storage Methods (for sensitive data like tokens)
  // ====================

  /// Save secure data
  Future<void> saveSecureData(String key, String value) async {
    await _secureStorage.write(key: key, value: value);
  }

  /// Get secure data
  Future<String?> getSecureData(String key) async {
    return await _secureStorage.read(key: key);
  }

  /// Delete secure data
  Future<void> deleteSecureData(String key) async {
    await _secureStorage.delete(key: key);
  }

  /// Delete all secure data
  Future<void> deleteAllSecureData() async {
    await _secureStorage.deleteAll();
  }

  /// Check if secure key exists
  Future<bool> containsSecureKey(String key) async {
    final value = await _secureStorage.read(key: key);
    return value != null;
  }

  // ====================
  // Shared Preferences Methods (for non-sensitive data)
  // ====================

  /// Ensure preferences are initialized
  Future<SharedPreferences> get prefs async {
    if (_prefs == null) {
      await init();
    }
    return _prefs!;
  }

  /// Save string
  Future<bool> saveString(String key, String value) async {
    final preferences = await prefs;
    return await preferences.setString(key, value);
  }

  /// Get string
  Future<String?> getString(String key) async {
    final preferences = await prefs;
    return preferences.getString(key);
  }

  /// Save int
  Future<bool> saveInt(String key, int value) async {
    final preferences = await prefs;
    return await preferences.setInt(key, value);
  }

  /// Get int
  Future<int?> getInt(String key) async {
    final preferences = await prefs;
    return preferences.getInt(key);
  }

  /// Save bool
  Future<bool> saveBool(String key, bool value) async {
    final preferences = await prefs;
    return await preferences.setBool(key, value);
  }

  /// Get bool
  Future<bool?> getBool(String key) async {
    final preferences = await prefs;
    return preferences.getBool(key);
  }

  /// Save double
  Future<bool> saveDouble(String key, double value) async {
    final preferences = await prefs;
    return await preferences.setDouble(key, value);
  }

  /// Get double
  Future<double?> getDouble(String key) async {
    final preferences = await prefs;
    return preferences.getDouble(key);
  }

  /// Save string list
  Future<bool> saveStringList(String key, List<String> value) async {
    final preferences = await prefs;
    return await preferences.setStringList(key, value);
  }

  /// Get string list
  Future<List<String>?> getStringList(String key) async {
    final preferences = await prefs;
    return preferences.getStringList(key);
  }

  /// Remove key from preferences
  Future<bool> remove(String key) async {
    final preferences = await prefs;
    return await preferences.remove(key);
  }

  /// Check if key exists in preferences
  Future<bool> containsKey(String key) async {
    final preferences = await prefs;
    return preferences.containsKey(key);
  }

  /// Clear all preferences
  Future<bool> clearAll() async {
    final preferences = await prefs;
    return await preferences.clear();
  }

  /// Clear all data (both secure and preferences)
  Future<void> clearAllData() async {
    await deleteAllSecureData();
    await clearAll();
  }
}
