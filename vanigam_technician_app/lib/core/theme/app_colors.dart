import 'package:flutter/material.dart';

/// Application Color Palette
class AppColors {
  AppColors._();

  // Primary Colors
  static const Color primary = Color(0xFF1976D2); // Blue
  static const Color primaryDark = Color(0xFF1565C0);
  static const Color primaryLight = Color(0xFF42A5F5);

  // Secondary Colors
  static const Color secondary = Color(0xFFFF9800); // Orange
  static const Color secondaryDark = Color(0xFFF57C00);
  static const Color secondaryLight = Color(0xFFFFB74D);

  // Status Colors
  static const Color success = Color(0xFF388E3C); // Green
  static const Color error = Color(0xFFD32F2F); // Red
  static const Color warning = Color(0xFFFFA000); // Amber
  static const Color info = Color(0xFF1976D2); // Blue

  // Job Status Colors
  static const Color statusPending = Color(0xFFFFA000); // Amber
  static const Color statusAssigned = Color(0xFF2196F3); // Blue
  static const Color statusEnRoute = Color(0xFF9C27B0); // Purple
  static const Color statusArrived = Color(0xFF00BCD4); // Cyan
  static const Color statusInProgress = Color(0xFFFF9800); // Orange
  static const Color statusCompleted = Color(0xFF4CAF50); // Green
  static const Color statusCancelled = Color(0xFF757575); // Gray

  // Priority Colors
  static const Color priorityLow = Color(0xFF4CAF50); // Green
  static const Color priorityNormal = Color(0xFF2196F3); // Blue
  static const Color priorityHigh = Color(0xFFFF9800); // Orange
  static const Color priorityCritical = Color(0xFFD32F2F); // Red

  // Background Colors
  static const Color background = Color(0xFFFAFAFA); // Light Gray
  static const Color backgroundDark = Color(0xFF121212);
  static const Color surface = Color(0xFFFFFFFF);
  static const Color surfaceDark = Color(0xFF1E1E1E);

  // Text Colors
  static const Color textPrimary = Color(0xFF212121);
  static const Color textSecondary = Color(0xFF757575);
  static const Color textHint = Color(0xFFBDBDBD);
  static const Color textDisabled = Color(0xFF9E9E9E);
  static const Color textLight = Color(0xFFFFFFFF);

  // Divider & Border Colors
  static const Color divider = Color(0xFFE0E0E0);
  static const Color border = Color(0xFFCCCCCC);

  // Overlay Colors
  static const Color overlay = Color(0x40000000); // 25% black
  static const Color overlayLight = Color(0x1A000000); // 10% black

  // Connectivity Banner Colors
  static const Color connectedBanner = Color(0xFF4CAF50);
  static const Color disconnectedBanner = Color(0xFFD32F2F);
}
