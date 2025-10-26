/// API Constants for Vanigam CRM Backend
class ApiConstants {
  ApiConstants._();

  // Base URLs
  static const String baseUrl = String.fromEnvironment(
    'BASE_URL',
    defaultValue: 'https://localhost:5270',
  );

  static const String odataBasePath = '/odata/VanigamAccountingService';

  // Authentication Endpoints
  static const String authenticateToken = '/Account/AuthenticateToken';
  static const String logout = '/Account/Logout';
  static const String getUserById = '/Account/GetUserById';
  static const String getLoginUser = '/Account/GetLoginUser';

  // OData Entity Sets
  static const String jobs = '$odataBasePath/Jobs';
  static const String jobAssignments = '$odataBasePath/JobAssignments';
  static const String appointments = '$odataBasePath/Appointments';
  static const String timeSheets = '$odataBasePath/TimeSheets';
  static const String jobReports = '$odataBasePath/JobReports';
  static const String technicians = '$odataBasePath/Technicians';
  static const String materialUsages = '$odataBasePath/MaterialUsages';
  static const String gpsPoints = '$odataBasePath/GPSPoints';
  static const String customers = '$odataBasePath/Customers';
  static const String contacts = '$odataBasePath/Contacts';
  static const String items = '$odataBasePath/Items';
  static const String attachments = '$odataBasePath/Attachments';

  // JWT Configuration
  static const String jwtIssuer = String.fromEnvironment(
    'JWT_ISSUER',
    defaultValue: 'Vanigam.CRM',
  );

  // Request Timeouts
  static const Duration connectionTimeout = Duration(seconds: 30);
  static const Duration receiveTimeout = Duration(seconds: 30);

  // Pagination
  static const int defaultPageSize = 20;
  static const int maxPageSize = 100;

  // Image Upload
  static const int maxImageSizeMB = 1;
  static const int maxImageSizeBytes = maxImageSizeMB * 1024 * 1024;
}
