import 'package:intl/intl.dart';
import '../constants/app_constants.dart';

/// Date formatting utilities
class AppDateUtils {
  AppDateUtils._();

  /// Format date to display format (MMM dd, yyyy)
  static String formatDisplayDate(DateTime? date) {
    if (date == null) return '-';
    return DateFormat(AppConstants.displayDateFormat).format(date);
  }

  /// Format datetime to display format (MMM dd, yyyy hh:mm a)
  static String formatDisplayDateTime(DateTime? dateTime) {
    if (dateTime == null) return '-';
    return DateFormat(AppConstants.displayDateTimeFormat).format(dateTime);
  }

  /// Format time (HH:mm)
  static String formatTime(DateTime? time) {
    if (time == null) return '-';
    return DateFormat(AppConstants.timeFormat).format(time);
  }

  /// Format time with AM/PM (hh:mm a)
  static String formatTimeWithPeriod(DateTime? time) {
    if (time == null) return '-';
    return DateFormat('hh:mm a').format(time);
  }

  /// Format date to API format (yyyy-MM-dd)
  static String formatApiDate(DateTime? date) {
    if (date == null) return '';
    return DateFormat(AppConstants.dateFormat).format(date);
  }

  /// Format datetime to ISO8601 (for API)
  static String formatApiDateTime(DateTime? dateTime) {
    if (dateTime == null) return '';
    return dateTime.toIso8601String();
  }

  /// Parse API date string (yyyy-MM-dd)
  static DateTime? parseApiDate(String? dateString) {
    if (dateString == null || dateString.isEmpty) return null;
    try {
      return DateFormat(AppConstants.dateFormat).parse(dateString);
    } catch (e) {
      return null;
    }
  }

  /// Parse ISO8601 datetime string
  static DateTime? parseApiDateTime(String? dateTimeString) {
    if (dateTimeString == null || dateTimeString.isEmpty) return null;
    try {
      return DateTime.parse(dateTimeString);
    } catch (e) {
      return null;
    }
  }

  /// Get relative time (e.g., "2 hours ago", "3 days ago")
  static String getRelativeTime(DateTime? dateTime) {
    if (dateTime == null) return '-';

    final now = DateTime.now();
    final difference = now.difference(dateTime);

    if (difference.inSeconds < 60) {
      return 'Just now';
    } else if (difference.inMinutes < 60) {
      final minutes = difference.inMinutes;
      return '$minutes ${minutes == 1 ? 'minute' : 'minutes'} ago';
    } else if (difference.inHours < 24) {
      final hours = difference.inHours;
      return '$hours ${hours == 1 ? 'hour' : 'hours'} ago';
    } else if (difference.inDays < 7) {
      final days = difference.inDays;
      return '$days ${days == 1 ? 'day' : 'days'} ago';
    } else if (difference.inDays < 30) {
      final weeks = (difference.inDays / 7).floor();
      return '$weeks ${weeks == 1 ? 'week' : 'weeks'} ago';
    } else if (difference.inDays < 365) {
      final months = (difference.inDays / 30).floor();
      return '$months ${months == 1 ? 'month' : 'months'} ago';
    } else {
      final years = (difference.inDays / 365).floor();
      return '$years ${years == 1 ? 'year' : 'years'} ago';
    }
  }

  /// Check if date is today
  static bool isToday(DateTime? date) {
    if (date == null) return false;
    final now = DateTime.now();
    return date.year == now.year && date.month == now.month && date.day == now.day;
  }

  /// Check if date is yesterday
  static bool isYesterday(DateTime? date) {
    if (date == null) return false;
    final yesterday = DateTime.now().subtract(const Duration(days: 1));
    return date.year == yesterday.year &&
        date.month == yesterday.month &&
        date.day == yesterday.day;
  }

  /// Format duration (e.g., "2h 30m")
  static String formatDuration(Duration? duration) {
    if (duration == null) return '-';

    final hours = duration.inHours;
    final minutes = duration.inMinutes.remainder(60);

    if (hours > 0) {
      return '${hours}h ${minutes}m';
    } else if (minutes > 0) {
      return '${minutes}m';
    } else {
      return '< 1m';
    }
  }

  /// Format duration in HH:MM:SS format
  static String formatDurationHMS(Duration? duration) {
    if (duration == null) return '00:00:00';

    final hours = duration.inHours.toString().padLeft(2, '0');
    final minutes = duration.inMinutes.remainder(60).toString().padLeft(2, '0');
    final seconds = duration.inSeconds.remainder(60).toString().padLeft(2, '0');

    return '$hours:$minutes:$seconds';
  }

  /// Get start of day
  static DateTime startOfDay(DateTime date) {
    return DateTime(date.year, date.month, date.day);
  }

  /// Get end of day
  static DateTime endOfDay(DateTime date) {
    return DateTime(date.year, date.month, date.day, 23, 59, 59, 999);
  }

  /// Get start of week
  static DateTime startOfWeek(DateTime date) {
    final weekday = date.weekday;
    return startOfDay(date.subtract(Duration(days: weekday - 1)));
  }

  /// Get end of week
  static DateTime endOfWeek(DateTime date) {
    final weekday = date.weekday;
    return endOfDay(date.add(Duration(days: 7 - weekday)));
  }

  /// Get start of month
  static DateTime startOfMonth(DateTime date) {
    return DateTime(date.year, date.month, 1);
  }

  /// Get end of month
  static DateTime endOfMonth(DateTime date) {
    return DateTime(date.year, date.month + 1, 0, 23, 59, 59, 999);
  }
}
