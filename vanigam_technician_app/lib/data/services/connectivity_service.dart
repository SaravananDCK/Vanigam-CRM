import 'dart:async';
import 'package:connectivity_plus/connectivity_plus.dart';

/// Connectivity Service for monitoring internet connection
class ConnectivityService {
  // Singleton pattern
  static final ConnectivityService _instance = ConnectivityService._internal();
  factory ConnectivityService() => _instance;
  ConnectivityService._internal();

  final Connectivity _connectivity = Connectivity();
  StreamController<bool>? _connectionStatusController;

  /// Stream of connection status
  Stream<bool> get connectionStatus {
    _connectionStatusController ??= StreamController<bool>.broadcast();
    return _connectionStatusController!.stream;
  }

  /// Initialize connectivity monitoring
  void initialize() {
    _connectivity.onConnectivityChanged.listen((ConnectivityResult result) {
      final isConnected = _checkConnectivity(result);
      _connectionStatusController?.add(isConnected);
    });
  }

  /// Check if device is connected
  bool _checkConnectivity(ConnectivityResult result) {
    // Device is connected if result is not 'none'
    return result != ConnectivityResult.none;
  }

  /// Get current connectivity status
  Future<bool> get isConnected async {
    try {
      final result = await _connectivity.checkConnectivity();
      return _checkConnectivity(result);
    } catch (e) {
      return false;
    }
  }

  /// Get connectivity result details
  Future<ConnectivityResult> get connectivityResult async {
    return await _connectivity.checkConnectivity();
  }

  /// Dispose
  void dispose() {
    _connectionStatusController?.close();
    _connectionStatusController = null;
  }
}
