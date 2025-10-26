import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../data/repositories/job_repository.dart';
import '../../../data/models/job.dart';
import '../../../core/network/api_client.dart';
import 'jobs_state.dart';

/// Jobs cubit for managing job state
class JobsCubit extends Cubit<JobsState> {
  final JobRepository _jobRepository;
  String? _currentTechnicianId;

  JobsCubit({
    required JobRepository jobRepository,
  })  : _jobRepository = jobRepository,
        super(const JobsInitial());

  /// Set current technician ID
  void setTechnicianId(String technicianId) {
    _currentTechnicianId = technicianId;
  }

  /// Load all jobs
  Future<void> loadJobs({
    JobStatus? status,
    String? orderBy = 'VoucherDate desc',
    int? top,
    int? skip,
  }) async {
    try {
      emit(const JobsLoading());

      final response = await _jobRepository.getMyJobs(
        technicianId: _currentTechnicianId,
        status: status,
        orderBy: orderBy,
        top: top,
        skip: skip,
      );

      emit(JobsLoaded(
        jobs: response.value ?? [],
        totalCount: response.count ?? 0,
        currentFilter: status,
      ));
    } on NetworkException catch (e) {
      emit(JobsError(e.message));
    } catch (e) {
      emit(JobsError('Failed to load jobs. Please try again.'));
    }
  }

  /// Load today's jobs
  Future<void> loadTodaysJobs() async {
    try {
      emit(const JobsLoading());

      final response = await _jobRepository.getTodaysJobs(
        technicianId: _currentTechnicianId,
      );

      emit(JobsLoaded(
        jobs: response.value ?? [],
        totalCount: response.count ?? 0,
      ));
    } on NetworkException catch (e) {
      emit(JobsError(e.message));
    } catch (e) {
      emit(JobsError('Failed to load today\'s jobs. Please try again.'));
    }
  }

  /// Load job statistics
  Future<void> loadStatistics() async {
    try {
      emit(const JobsLoading());

      final statistics = await _jobRepository.getJobStatistics(
        technicianId: _currentTechnicianId,
      );

      emit(JobsStatisticsLoaded(statistics));
    } on NetworkException catch (e) {
      emit(JobsError(e.message));
    } catch (e) {
      emit(JobsError('Failed to load statistics. Please try again.'));
    }
  }

  /// Load job detail
  Future<void> loadJobDetail(String id) async {
    try {
      emit(const JobsLoading());

      final job = await _jobRepository.getJobById(id);

      emit(JobDetailLoaded(job));
    } on NotFoundException catch (e) {
      emit(JobsError(e.message));
    } on NetworkException catch (e) {
      emit(JobsError(e.message));
    } catch (e) {
      emit(JobsError('Failed to load job details. Please try again.'));
    }
  }

  /// Update job status
  Future<void> updateJobStatus(String id, JobStatus status) async {
    try {
      emit(const JobUpdating());

      final updatedJob = await _jobRepository.updateJobStatus(id, status);

      emit(JobUpdated(
        updatedJob,
        'Job status updated to ${status.displayName}',
      ));
    } on NetworkException catch (e) {
      emit(JobsError(e.message));
    } catch (e) {
      emit(JobsError('Failed to update job status. Please try again.'));
    }
  }

  /// Start job (change status to InProgress)
  Future<void> startJob(String id) async {
    await updateJobStatus(id, JobStatus.inProgress);
  }

  /// Complete job (change status to Completed)
  Future<void> completeJob(String id) async {
    await updateJobStatus(id, JobStatus.completed);
  }

  /// Pause job (change status to OnHold)
  Future<void> pauseJob(String id) async {
    await updateJobStatus(id, JobStatus.onHold);
  }

  /// Refresh current list
  Future<void> refresh() async {
    if (state is JobsLoaded) {
      final currentState = state as JobsLoaded;
      await loadJobs(status: currentState.currentFilter);
    } else {
      await loadJobs();
    }
  }

  /// Clear error state
  void clearError() {
    emit(const JobsInitial());
  }
}
