import 'package:get_it/get_it.dart';
import '../../core/network/api_client.dart';
import '../../data/services/storage_service.dart';
import '../../data/services/connectivity_service.dart';
import '../../data/repositories/auth_repository.dart';
import '../../data/repositories/job_repository.dart';
import '../../features/auth/cubit/auth_cubit.dart';
import '../../features/jobs/cubit/jobs_cubit.dart';

/// Service Locator
final getIt = GetIt.instance;

/// Setup Dependency Injection
Future<void> setupDependencyInjection() async {
  // Core Services
  getIt.registerLazySingleton<ApiClient>(() => ApiClient());
  getIt.registerLazySingleton<StorageService>(() => StorageService());
  getIt.registerLazySingleton<ConnectivityService>(() => ConnectivityService());

  // Initialize storage service
  await getIt<StorageService>().init();

  // Initialize connectivity service
  getIt<ConnectivityService>().initialize();

  // Repositories
  getIt.registerLazySingleton<AuthRepository>(
    () => AuthRepository(
      apiClient: getIt<ApiClient>(),
      storageService: getIt<StorageService>(),
    ),
  );

  getIt.registerLazySingleton<JobRepository>(
    () => JobRepository(
      apiClient: getIt<ApiClient>(),
    ),
  );

  // Cubits/Blocs (as factories for fresh instances)
  getIt.registerFactory<AuthCubit>(
    () => AuthCubit(
      authRepository: getIt<AuthRepository>(),
    ),
  );

  getIt.registerFactory<JobsCubit>(
    () => JobsCubit(
      jobRepository: getIt<JobRepository>(),
    ),
  );
}

/// Clean up dependency injection
Future<void> disposeDependencyInjection() async {
  getIt<ConnectivityService>().dispose();
  await getIt.reset();
}
