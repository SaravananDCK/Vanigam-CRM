import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../data/repositories/auth_repository.dart';
import '../../../core/network/api_client.dart';
import 'auth_state.dart';

/// Authentication cubit for managing auth state
class AuthCubit extends Cubit<AuthState> {
  final AuthRepository _authRepository;

  AuthCubit({
    required AuthRepository authRepository,
  })  : _authRepository = authRepository,
        super(const AuthInitial());

  /// Check if user is already logged in (auto-login)
  Future<void> checkAuthStatus() async {
    try {
      emit(const AuthLoading());

      final isLoggedIn = await _authRepository.isLoggedIn();

      if (isLoggedIn) {
        final user = await _authRepository.getSavedUser();
        if (user != null) {
          emit(AuthAuthenticated(user));
        } else {
          emit(const AuthUnauthenticated());
        }
      } else {
        emit(const AuthUnauthenticated());
      }
    } catch (e) {
      emit(const AuthUnauthenticated());
    }
  }

  /// Login with username and password
  Future<void> login({
    required String userName,
    required String password,
  }) async {
    try {
      emit(const AuthLoading());

      final response = await _authRepository.login(
        userName: userName,
        password: password,
      );

      if (response.success == true && response.user != null) {
        emit(AuthAuthenticated(response.user!));
      } else {
        emit(AuthError(response.message ?? 'Login failed. Please try again.'));
      }
    } on UnauthorizedException catch (e) {
      emit(AuthError(e.message));
    } on NetworkException catch (e) {
      emit(AuthError(e.message));
    } catch (e) {
      emit(AuthError('An unexpected error occurred. Please try again.'));
    }
  }

  /// Logout
  Future<void> logout() async {
    try {
      await _authRepository.logout();
      emit(const AuthUnauthenticated());
    } catch (e) {
      emit(AuthError('Failed to logout. Please try again.'));
    }
  }

  /// Clear error state
  void clearError() {
    emit(const AuthUnauthenticated());
  }
}
