import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../auth/cubit/auth_cubit.dart';
import '../../auth/cubit/auth_state.dart';
import '../../auth/screens/login_screen.dart';
import '../../jobs/cubit/jobs_cubit.dart';
import '../../jobs/cubit/jobs_state.dart';
import '../../jobs/screens/jobs_list_screen.dart';
import '../../../core/theme/app_colors.dart';
import '../../../data/repositories/job_repository.dart';
import '../../../core/di/injection.dart';

/// Dashboard screen with job statistics
class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  late final JobsCubit _jobsCubit;

  @override
  void initState() {
    super.initState();
    _jobsCubit = JobsCubit(jobRepository: getIt<JobRepository>());
    _loadData();
  }

  @override
  void dispose() {
    _jobsCubit.close();
    super.dispose();
  }

  Future<void> _loadData() async {
    // Get user ID from auth state
    final authState = context.read<AuthCubit>().state;
    if (authState is AuthAuthenticated) {
      _jobsCubit.setTechnicianId(authState.user.id);
    }
    await _jobsCubit.loadStatistics();
  }

  void _handleLogout(BuildContext context) {
    context.read<AuthCubit>().logout();
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (_) => const LoginScreen(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Dashboard'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadData,
            tooltip: 'Refresh',
          ),
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () => _handleLogout(context),
            tooltip: 'Logout',
          ),
        ],
      ),
      body: BlocBuilder<AuthCubit, AuthState>(
        builder: (context, authState) {
          return RefreshIndicator(
            onRefresh: _loadData,
            child: SingleChildScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Welcome message
                    if (authState is AuthAuthenticated) ...[
                      Text(
                        'Welcome back,',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        authState.user.fullName ?? authState.user.email,
                        style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                      ),
                      const SizedBox(height: 24),
                    ],

                    // Statistics cards
                    BlocBuilder<JobsCubit, JobsState>(
                      bloc: _jobsCubit,
                      builder: (context, state) {
                        if (state is JobsLoading) {
                          return const Center(
                            child: Padding(
                              padding: EdgeInsets.all(32),
                              child: CircularProgressIndicator(),
                            ),
                          );
                        }

                        if (state is JobsError) {
                          return Center(
                            child: Padding(
                              padding: const EdgeInsets.all(32),
                              child: Column(
                                children: [
                                  const Icon(
                                    Icons.error_outline,
                                    size: 48,
                                    color: AppColors.error,
                                  ),
                                  const SizedBox(height: 16),
                                  Text(
                                    state.message,
                                    textAlign: TextAlign.center,
                                    style: const TextStyle(color: AppColors.error),
                                  ),
                                  const SizedBox(height: 16),
                                  ElevatedButton(
                                    onPressed: _loadData,
                                    child: const Text('Retry'),
                                  ),
                                ],
                              ),
                            ),
                          );
                        }

                        if (state is JobsStatisticsLoaded) {
                          final stats = state.statistics;
                          return Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              // Statistics grid
                              GridView.count(
                                shrinkWrap: true,
                                physics: const NeverScrollableScrollPhysics(),
                                crossAxisCount: 2,
                                crossAxisSpacing: 16,
                                mainAxisSpacing: 16,
                                childAspectRatio: 1.5,
                                children: [
                                  _StatCard(
                                    title: 'Today\'s Jobs',
                                    count: stats.todayJobs,
                                    icon: Icons.today,
                                    color: AppColors.info,
                                  ),
                                  _StatCard(
                                    title: 'In Progress',
                                    count: stats.inProgressJobs,
                                    icon: Icons.work,
                                    color: AppColors.warning,
                                  ),
                                  _StatCard(
                                    title: 'Pending',
                                    count: stats.pendingJobs,
                                    icon: Icons.pending_actions,
                                    color: AppColors.secondary,
                                  ),
                                  _StatCard(
                                    title: 'Completed',
                                    count: stats.completedJobs,
                                    icon: Icons.check_circle,
                                    color: AppColors.success,
                                  ),
                                ],
                              ),
                              const SizedBox(height: 32),

                              // Quick actions
                              Text(
                                'Quick Actions',
                                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                                      fontWeight: FontWeight.bold,
                                    ),
                              ),
                              const SizedBox(height: 16),
                              _QuickActionButton(
                                title: 'View All Jobs',
                                icon: Icons.list_alt,
                                color: AppColors.primary,
                                onTap: () {
                                  Navigator.push(
                                    context,
                                    MaterialPageRoute(
                                      builder: (_) => const JobsListScreen(),
                                    ),
                                  );
                                },
                              ),
                              const SizedBox(height: 12),
                              _QuickActionButton(
                                title: 'Today\'s Schedule',
                                icon: Icons.calendar_today,
                                color: AppColors.info,
                                onTap: () {
                                  Navigator.push(
                                    context,
                                    MaterialPageRoute(
                                      builder: (_) => const JobsListScreen(
                                        showTodayOnly: true,
                                      ),
                                    ),
                                  );
                                },
                              ),
                            ],
                          );
                        }

                        return const SizedBox.shrink();
                      },
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}

/// Statistics card widget
class _StatCard extends StatelessWidget {
  final String title;
  final int count;
  final IconData icon;
  final Color color;

  const _StatCard({
    required this.title,
    required this.count,
    required this.icon,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Icon(icon, color: color, size: 32),
                Text(
                  count.toString(),
                  style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: color,
                      ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              title,
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: AppColors.textSecondary,
                  ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ],
        ),
      ),
    );
  }
}

/// Quick action button widget
class _QuickActionButton extends StatelessWidget {
  final String title;
  final IconData icon;
  final Color color;
  final VoidCallback onTap;

  const _QuickActionButton({
    required this.title,
    required this.icon,
    required this.color,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 1,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(8),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: color.withAlpha(26),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Icon(icon, color: color, size: 24),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Text(
                  title,
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                ),
              ),
              Icon(
                Icons.arrow_forward_ios,
                size: 16,
                color: AppColors.textSecondary,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
