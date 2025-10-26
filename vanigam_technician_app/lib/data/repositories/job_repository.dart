import '../../core/network/api_client.dart';
import '../../core/constants/api_constants.dart';
import '../../core/network/odata_query_builder.dart';
import '../models/job.dart';
import '../models/base_response.dart';

/// Repository for job operations
class JobRepository {
  final ApiClient _apiClient;

  JobRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Get jobs with optional filtering
  Future<ODataResponse<Job>> getJobs({
    String? filter,
    String? orderBy,
    int? top,
    int? skip,
    String? expand,
    bool count = true,
  }) async {
    try {
      final queryParams = <String, dynamic>{
        if (filter != null) '\$filter': filter,
        if (orderBy != null) '\$orderby': orderBy,
        if (top != null) '\$top': top,
        if (skip != null) '\$skip': skip,
        if (expand != null) '\$expand': expand,
        if (count) '\$count': 'true',
      };

      final response = await _apiClient.get(
        '${ApiConstants.odataBasePath}${ApiConstants.jobs}',
        queryParameters: queryParams,
      );

      return ODataResponse<Job>.fromJson(
        response.data,
        (json) => Job.fromJson(json as Map<String, dynamic>),
      );
    } catch (e) {
      rethrow;
    }
  }

  /// Get job by ID
  Future<Job> getJobById(String id) async {
    try {
      final response = await _apiClient.get(
        '${ApiConstants.odataBasePath}${ApiConstants.jobs}($id)',
      );

      return Job.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Get jobs assigned to current technician
  Future<ODataResponse<Job>> getMyJobs({
    String? technicianId,
    JobStatus? status,
    String? orderBy = 'VoucherDate desc',
    int? top,
    int? skip,
  }) async {
    try {
      final queryBuilder = ODataQueryBuilder();

      // Filter by status if provided
      if (status != null) {
        queryBuilder.filterEq('Status', status.name);
      }

      // Filter by technician if provided (via JobAssignments)
      if (technicianId != null) {
        // Note: This requires expand on Assignments
        queryBuilder.filter(
          'Assignments/any(a: a/TechnicianId eq $technicianId)',
        );
      }

      if (orderBy != null) {
        queryBuilder.orderBy(orderBy);
      }

      if (top != null) {
        queryBuilder.top(top);
      }

      if (skip != null) {
        queryBuilder.skip(skip);
      }

      final params = queryBuilder.count(true).build();

      final response = await _apiClient.get(
        '${ApiConstants.odataBasePath}${ApiConstants.jobs}',
        queryParameters: params,
      );

      return ODataResponse<Job>.fromJson(
        response.data,
        (json) => Job.fromJson(json as Map<String, dynamic>),
      );
    } catch (e) {
      rethrow;
    }
  }

  /// Get today's jobs
  Future<ODataResponse<Job>> getTodaysJobs({
    String? technicianId,
  }) async {
    try {
      final now = DateTime.now();
      final today = DateTime(now.year, now.month, now.day);
      final tomorrow = today.add(const Duration(days: 1));

      final queryBuilder = ODataQueryBuilder()
          .filterGe('VoucherDate', today.toIso8601String())
          .filterLt('VoucherDate', tomorrow.toIso8601String())
          .orderBy('VoucherDate desc');

      if (technicianId != null) {
        queryBuilder.filter(
          'Assignments/any(a: a/TechnicianId eq $technicianId)',
        );
      }

      final params = queryBuilder.count(true).build();

      final response = await _apiClient.get(
        '${ApiConstants.odataBasePath}${ApiConstants.jobs}',
        queryParameters: params,
      );

      return ODataResponse<Job>.fromJson(
        response.data,
        (json) => Job.fromJson(json as Map<String, dynamic>),
      );
    } catch (e) {
      rethrow;
    }
  }

  /// Update job status
  Future<Job> updateJobStatus(String id, JobStatus status) async {
    try {
      final response = await _apiClient.patch(
        '${ApiConstants.odataBasePath}${ApiConstants.jobs}($id)',
        data: {'Status': status.name},
      );

      return Job.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Update job
  Future<Job> updateJob(String id, Map<String, dynamic> data) async {
    try {
      final response = await _apiClient.patch(
        '${ApiConstants.odataBasePath}${ApiConstants.jobs}($id)',
        data: data,
      );

      return Job.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Get job statistics
  Future<JobStatistics> getJobStatistics({String? technicianId}) async {
    try {
      // Get counts for different statuses
      final allJobs = await getMyJobs(technicianId: technicianId);
      final todayJobs = await getTodaysJobs(technicianId: technicianId);
      final pendingJobs = await getMyJobs(
        technicianId: technicianId,
        status: JobStatus.pending,
      );
      final inProgressJobs = await getMyJobs(
        technicianId: technicianId,
        status: JobStatus.inProgress,
      );
      final completedJobs = await getMyJobs(
        technicianId: technicianId,
        status: JobStatus.completed,
      );

      return JobStatistics(
        totalJobs: allJobs.count ?? 0,
        todayJobs: todayJobs.count ?? 0,
        pendingJobs: pendingJobs.count ?? 0,
        inProgressJobs: inProgressJobs.count ?? 0,
        completedJobs: completedJobs.count ?? 0,
      );
    } catch (e) {
      rethrow;
    }
  }
}

/// Job statistics model
class JobStatistics {
  final int totalJobs;
  final int todayJobs;
  final int pendingJobs;
  final int inProgressJobs;
  final int completedJobs;

  const JobStatistics({
    required this.totalJobs,
    required this.todayJobs,
    required this.pendingJobs,
    required this.inProgressJobs,
    required this.completedJobs,
  });
}
