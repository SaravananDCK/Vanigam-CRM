import 'package:equatable/equatable.dart';
import '../../../data/models/job.dart';
import '../../../data/repositories/job_repository.dart';

/// Jobs state
abstract class JobsState extends Equatable {
  const JobsState();

  @override
  List<Object?> get props => [];
}

/// Initial state
class JobsInitial extends JobsState {
  const JobsInitial();
}

/// Loading state
class JobsLoading extends JobsState {
  const JobsLoading();
}

/// Jobs loaded successfully
class JobsLoaded extends JobsState {
  final List<Job> jobs;
  final int totalCount;
  final JobStatus? currentFilter;

  const JobsLoaded({
    required this.jobs,
    required this.totalCount,
    this.currentFilter,
  });

  @override
  List<Object?> get props => [jobs, totalCount, currentFilter];

  JobsLoaded copyWith({
    List<Job>? jobs,
    int? totalCount,
    JobStatus? currentFilter,
  }) {
    return JobsLoaded(
      jobs: jobs ?? this.jobs,
      totalCount: totalCount ?? this.totalCount,
      currentFilter: currentFilter ?? this.currentFilter,
    );
  }
}

/// Job statistics loaded
class JobsStatisticsLoaded extends JobsState {
  final JobStatistics statistics;

  const JobsStatisticsLoaded(this.statistics);

  @override
  List<Object?> get props => [statistics];
}

/// Error state
class JobsError extends JobsState {
  final String message;

  const JobsError(this.message);

  @override
  List<Object?> get props => [message];
}

/// Job detail loaded
class JobDetailLoaded extends JobsState {
  final Job job;

  const JobDetailLoaded(this.job);

  @override
  List<Object?> get props => [job];
}

/// Job updating state
class JobUpdating extends JobsState {
  const JobUpdating();
}

/// Job updated successfully
class JobUpdated extends JobsState {
  final Job job;
  final String message;

  const JobUpdated(this.job, this.message);

  @override
  List<Object?> get props => [job, message];
}
