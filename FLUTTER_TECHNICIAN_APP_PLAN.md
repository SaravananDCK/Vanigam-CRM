# Flutter Technician Mobile App - Project Plan

## 📋 Project Overview

A mobile application for field technicians to manage job assignments, track time, submit reports, and navigate to job sites. This is an online-only application that connects to the Vanigam CRM backend via OData APIs.

---

## 🎯 Target Platform
- **iOS**: iOS 12.0+
- **Android**: Android 5.0+ (API 21+)

---

## 🏗️ Backend Integration

### Authentication
- **Endpoint**: `POST /Account/AuthenticateToken`
- **Method**: JWT Bearer Token Authentication
- **Response**: `{ Token, Name, UserId, UserType, Roles }`
- **Token Lifetime**: 2 hours (configurable)
- **Multi-tenant**: Uses `TenantId` in JWT claims

### API Structure
- **Base URL**: `https://your-server.com/odata/VanigamAccountingService/`
- **Protocol**: OData v8
- **Features**: `$filter`, `$expand`, `$select`, `$orderby`, `$top`, `$skip`, `$count`

### Key Controllers
- `Jobs` - Job management
- `JobAssignments` - Technician assignments
- `Appointments` - Scheduled appointments
- `TimeSheets` - Time tracking
- `JobReports` - Job completion reports
- `Technicians` - Technician profile
- `MaterialUsages` - Materials/parts tracking
- `GPSPoints` - Location tracking
- `Customers` - Customer information
- `Attachments` - File uploads

---

## 🏗️ Architecture

### Design Pattern
**Clean Architecture + Feature-First Structure**

```
Presentation Layer (UI) → Business Logic (Cubit/Bloc) → Data Layer (Repository) → API Service
```

### Project Structure

```
vanigam_technician_app/
├── lib/
│   ├── core/                          # Core utilities and configurations
│   │   ├── constants/
│   │   │   ├── api_constants.dart     # API endpoints, base URLs
│   │   │   ├── app_constants.dart     # App-wide constants
│   │   │   └── storage_keys.dart      # Secure storage keys
│   │   ├── di/
│   │   │   └── injection.dart         # Dependency injection (GetIt)
│   │   ├── network/
│   │   │   ├── api_client.dart        # Dio HTTP client
│   │   │   ├── api_interceptor.dart   # Auth token interceptor
│   │   │   └── odata_query_builder.dart # OData query helper
│   │   ├── utils/
│   │   │   ├── date_utils.dart        # Date formatting utilities
│   │   │   ├── validators.dart        # Form validators
│   │   │   └── extensions.dart        # Dart extensions
│   │   └── theme/
│   │       ├── app_theme.dart         # Material theme configuration
│   │       └── app_colors.dart        # Color constants
│   ├── data/                          # Data layer
│   │   ├── models/                    # Data models with JSON serialization
│   │   │   ├── auth/
│   │   │   │   ├── login_request.dart
│   │   │   │   ├── login_response.dart
│   │   │   │   └── user.dart
│   │   │   ├── job.dart
│   │   │   ├── job_assignment.dart
│   │   │   ├── appointment.dart
│   │   │   ├── timesheet.dart
│   │   │   ├── job_report.dart
│   │   │   ├── material_usage.dart
│   │   │   ├── customer.dart
│   │   │   ├── attachment.dart
│   │   │   └── gps_point.dart
│   │   ├── repositories/              # Repository pattern implementations
│   │   │   ├── auth_repository.dart
│   │   │   ├── job_repository.dart
│   │   │   ├── timesheet_repository.dart
│   │   │   ├── report_repository.dart
│   │   │   └── material_repository.dart
│   │   └── services/                  # API and platform services
│   │       ├── api_service.dart       # REST API calls
│   │       ├── location_service.dart  # GPS tracking
│   │       └── notification_service.dart # Push notifications
│   ├── features/                      # Feature modules
│   │   ├── auth/
│   │   │   ├── screens/
│   │   │   │   ├── splash_screen.dart
│   │   │   │   └── login_screen.dart
│   │   │   ├── cubits/
│   │   │   │   ├── auth_cubit.dart
│   │   │   │   └── auth_state.dart
│   │   │   └── widgets/
│   │   │       ├── login_form.dart
│   │   │       └── logo_widget.dart
│   │   ├── dashboard/
│   │   │   ├── screens/
│   │   │   │   └── dashboard_screen.dart
│   │   │   ├── cubits/
│   │   │   │   ├── dashboard_cubit.dart
│   │   │   │   └── dashboard_state.dart
│   │   │   └── widgets/
│   │   │       ├── job_summary_card.dart
│   │   │       ├── active_timesheet_card.dart
│   │   │       └── quick_actions.dart
│   │   ├── jobs/
│   │   │   ├── screens/
│   │   │   │   ├── job_list_screen.dart
│   │   │   │   ├── job_detail_screen.dart
│   │   │   │   └── job_map_screen.dart
│   │   │   ├── cubits/
│   │   │   │   ├── job_list_cubit.dart
│   │   │   │   ├── job_list_state.dart
│   │   │   │   ├── job_detail_cubit.dart
│   │   │   │   └── job_detail_state.dart
│   │   │   └── widgets/
│   │   │       ├── job_card.dart
│   │   │       ├── status_badge.dart
│   │   │       ├── priority_indicator.dart
│   │   │       └── customer_info_card.dart
│   │   ├── timesheet/
│   │   │   ├── screens/
│   │   │   │   ├── clock_in_screen.dart
│   │   │   │   └── timesheet_history_screen.dart
│   │   │   ├── cubits/
│   │   │   │   ├── timesheet_cubit.dart
│   │   │   │   └── timesheet_state.dart
│   │   │   └── widgets/
│   │   │       ├── timer_widget.dart
│   │   │       └── timesheet_card.dart
│   │   ├── job_report/
│   │   │   ├── screens/
│   │   │   │   ├── create_report_screen.dart
│   │   │   │   └── signature_screen.dart
│   │   │   ├── cubits/
│   │   │   │   ├── report_cubit.dart
│   │   │   │   └── report_state.dart
│   │   │   └── widgets/
│   │   │       ├── signature_pad.dart
│   │   │       ├── photo_capture_widget.dart
│   │   │       └── report_form.dart
│   │   ├── materials/
│   │   │   ├── screens/
│   │   │   │   └── material_usage_screen.dart
│   │   │   ├── cubits/
│   │   │   │   ├── material_cubit.dart
│   │   │   │   └── material_state.dart
│   │   │   └── widgets/
│   │   │       └── material_item_card.dart
│   │   └── profile/
│   │       ├── screens/
│   │       │   └── profile_screen.dart
│   │       ├── cubits/
│   │       │   ├── profile_cubit.dart
│   │       │   └── profile_state.dart
│   │       └── widgets/
│   ├── shared/                        # Shared widgets and components
│   │   └── widgets/
│   │       ├── custom_button.dart
│   │       ├── custom_text_field.dart
│   │       ├── loading_indicator.dart
│   │       ├── error_widget.dart
│   │       ├── connectivity_banner.dart
│   │       └── empty_state_widget.dart
│   └── main.dart                      # App entry point
├── assets/                            # Asset files
│   ├── images/
│   ├── icons/
│   └── fonts/
├── test/                              # Unit and widget tests
├── integration_test/                  # Integration tests
├── android/                           # Android native code
├── ios/                               # iOS native code
├── pubspec.yaml                       # Package dependencies
└── README.md
```

---

## 🔧 Technology Stack

### State Management
- **flutter_bloc** (^8.1.3) - BLoC/Cubit pattern for predictable state management

### Networking
- **dio** (^5.4.0) - HTTP client for API calls
- **retrofit** (^4.0.3) - Type-safe REST client
- **json_annotation** (^4.8.1) - JSON serialization annotations
- **json_serializable** (^6.7.1) - JSON code generation
- **connectivity_plus** (^5.0.2) - Network connectivity monitoring

### Storage
- **flutter_secure_storage** (^9.0.0) - Secure storage for JWT tokens
- **shared_preferences** (^2.2.2) - Simple key-value storage for settings

### Dependency Injection
- **get_it** (^7.6.4) - Service locator pattern
- **injectable** (^2.3.2) - Code generation for dependency injection

### Location & Maps
- **geolocator** (^11.0.0) - GPS location services
- **google_maps_flutter** (^2.5.3) - Google Maps integration
- **location** (^5.0.3) - Continuous location tracking

### Media & Files
- **image_picker** (^1.0.5) - Camera and gallery access
- **signature** (^5.4.0) - Signature pad widget
- **file_picker** (^6.1.1) - File selection
- **path_provider** (^2.1.1) - File system paths

### UI Components
- **flutter_svg** (^2.0.9) - SVG rendering
- **cached_network_image** (^3.3.0) - Image caching
- **shimmer** (^3.0.0) - Loading skeleton screens
- **flutter_slidable** (^3.0.1) - Swipeable list items
- **badges** (^3.1.2) - Notification badges

### Utilities
- **intl** (^0.19.0) - Internationalization and date formatting
- **uuid** (^4.3.3) - UUID generation
- **permission_handler** (^11.1.0) - Runtime permissions
- **flutter_local_notifications** (^16.3.0) - Local notifications

### Development Tools
- **build_runner** (^2.4.7) - Code generation
- **flutter_lints** (^3.0.1) - Linting rules
- **mockito** (^5.4.4) - Mocking for tests

---

## 🎯 Core Features

### 1. Authentication
**Priority**: Critical
**Screens**: Splash, Login

**Functionality:**
- Username/password login
- JWT token management
- Secure token storage
- Auto token refresh
- Biometric authentication (optional)
- Remember me functionality
- Session management

**API Endpoints:**
- `POST /Account/AuthenticateToken` - Login

---

### 2. Dashboard
**Priority**: High
**Screens**: Dashboard

**Functionality:**
- Display today's job summary (Pending, In-Progress, Completed counts)
- Show active timesheet with running timer
- Quick action buttons (Jobs, Timesheet, Reports, Navigate)
- Notifications badge
- Pull-to-refresh

**API Endpoints:**
- `GET /odata/VanigamAccountingService/JobAssignments?$filter=TechnicianId eq {id}`
- `GET /odata/VanigamAccountingService/TimeSheets?$filter=TechnicianId eq {id} and EndAt eq null`

---

### 3. Job Management
**Priority**: Critical
**Screens**: Job List, Job Detail, Job Map

#### 3.1 Job List View
**Functionality:**
- Display assigned jobs with customer info
- Filter by status (All, Assigned, In-Progress, Completed)
- Search by customer name or job ID
- Sort by priority, date, distance
- Pull-to-refresh
- Tap to view details

**Job Card Display:**
- Priority indicator (High/Normal/Low)
- Job title and description
- Customer name
- Status badge
- Distance from current location
- Scheduled time
- Action buttons (Accept/Reject for new assignments)

**API Endpoints:**
- `GET /odata/VanigamAccountingService/JobAssignments?$filter=TechnicianId eq {id}&$expand=Job($expand=Customer,Contact)&$orderby=AssignedAt desc`

#### 3.2 Job Detail View
**Functionality:**
- Full job information display
- Customer contact details (call/email buttons)
- Job description and requirements
- Scheduled appointment time
- Current status and timeline
- Location on map with navigation button
- Action buttons based on status:
  - **Pending**: Accept/Reject
  - **Accepted**: Mark as En-Route
  - **En-Route**: Mark as Arrived
  - **Arrived**: Start Job (clock in + mark In-Progress)
  - **In-Progress**: Add Materials, Complete Job Report
  - **Finished**: View Report

**API Endpoints:**
- `GET /odata/VanigamAccountingService/Jobs({id})?$expand=Customer,Contact,Assignments,Appointments,JobReports,MaterialUsages`
- `PATCH /odata/VanigamAccountingService/JobAssignments({id})` - Update status

#### 3.3 Job Map View
**Functionality:**
- Show job location on map
- Show technician current location
- Calculate route and distance
- Navigate to job site (Google Maps integration)
- Track route while en-route

**API Endpoints:**
- `POST /odata/VanigamAccountingService/GPSPoints` - Track location points

---

### 4. Time Tracking
**Priority**: High
**Screens**: Clock In/Out, Timesheet History

#### 4.1 Clock In/Out
**Functionality:**
- Clock in when starting a job
- Display running timer
- Display elapsed time
- Show current job details
- Clock out when finishing
- Prevent multiple active timesheets
- GPS tracking while clocked in

**API Endpoints:**
- `POST /odata/VanigamAccountingService/TimeSheets` - Clock in
  ```json
  {
    "TechnicianId": "guid",
    "JobId": "guid",
    "StartAt": "2025-10-11T10:30:00Z"
  }
  ```
- `PATCH /odata/VanigamAccountingService/TimeSheets({id})` - Clock out
  ```json
  {
    "EndAt": "2025-10-11T14:30:00Z"
  }
  ```

#### 4.2 Timesheet History
**Functionality:**
- List all past timesheets
- Filter by date range
- Show duration calculations
- View associated job details

**API Endpoints:**
- `GET /odata/VanigamAccountingService/TimeSheets?$filter=TechnicianId eq {id}&$expand=Job&$orderby=StartAt desc`

---

### 5. Job Reports
**Priority**: High
**Screens**: Create Report, Signature Pad

**Functionality:**
- Rich text notes editor
- Photo capture from camera
- Select photos from gallery
- Multiple photo attachments
- Customer signature capture on canvas
- Review before submission
- Submit report to complete job

**API Endpoints:**
- `POST /odata/VanigamAccountingService/JobReports`
  ```json
  {
    "JobId": "guid",
    "Notes": "Completed AC repair. Replaced compressor unit.",
    "SignatureBase64": "data:image/png;base64,..."
  }
  ```
- `POST /odata/VanigamAccountingService/Attachments` - Upload photos

---

### 6. Material Usage
**Priority**: Medium
**Screens**: Material Usage

**Functionality:**
- Search inventory items
- Select materials/parts used
- Enter quantity used
- Add multiple line items
- Submit material usage for job

**API Endpoints:**
- `GET /odata/VanigamAccountingService/Items?$filter=Type eq 'InventoryItem'` - Search inventory
- `POST /odata/VanigamAccountingService/MaterialUsages`
  ```json
  {
    "JobId": "guid",
    "ItemId": "guid",
    "Quantity": 2,
    "UnitPrice": 150.00
  }
  ```

---

### 7. GPS & Navigation
**Priority**: Medium

**Functionality:**
- Get current location
- Calculate distance to job site
- Open in Google Maps for turn-by-turn navigation
- Track GPS points during job execution
- Location permission handling

**API Endpoints:**
- `POST /odata/VanigamAccountingService/GPSPoints` - Track location

---

### 8. Profile & Settings
**Priority**: Low
**Screens**: Profile

**Functionality:**
- View technician profile
- Update contact information
- Change status (Available/Busy/On-Leave)
- App settings
- Logout

**API Endpoints:**
- `GET /odata/VanigamAccountingService/Technicians({id})`
- `PATCH /odata/VanigamAccountingService/Technicians({id})`

---

### 9. Connectivity Management
**Priority**: High

**Functionality:**
- Monitor internet connectivity
- Show connectivity status banner when offline
- Disable actions requiring internet when offline
- Auto-retry failed requests
- Show appropriate error messages

---

## 📱 UI/UX Design Guidelines

### Color Scheme
```dart
Primary Color: #1976D2 (Blue)
Secondary Color: #FF9800 (Orange)
Error Color: #D32F2F (Red)
Success Color: #388E3C (Green)
Warning Color: #FFA000 (Amber)
Background: #FAFAFA (Light Gray)
```

### Status Colors
```dart
Pending: #FFA000 (Amber)
Assigned: #2196F3 (Blue)
En-Route: #9C27B0 (Purple)
Arrived: #00BCD4 (Cyan)
In-Progress: #FF9800 (Orange)
Completed: #4CAF50 (Green)
Cancelled: #757575 (Gray)
```

### Typography
```dart
Heading 1: 28px, Bold
Heading 2: 24px, SemiBold
Heading 3: 20px, Medium
Body Large: 16px, Regular
Body: 14px, Regular
Caption: 12px, Regular
```

### Spacing
```dart
Extra Small: 4px
Small: 8px
Medium: 16px
Large: 24px
Extra Large: 32px
```

---

## 🚀 Development Phases

### **Phase 1: Foundation (Week 1-2)**

#### Week 1: Project Setup
- [x] Create Flutter project
- [ ] Configure project structure
- [ ] Set up dependencies in pubspec.yaml
- [ ] Create folder structure (core, data, features, shared)
- [ ] Configure app icons and splash screen
- [ ] Set up Android/iOS configurations

#### Week 2: Core Infrastructure
- [ ] Implement API constants
- [ ] Create Dio HTTP client
- [ ] Implement API interceptor for JWT
- [ ] Create OData query builder utility
- [ ] Set up GetIt dependency injection
- [ ] Implement secure storage service
- [ ] Create connectivity monitoring service
- [ ] Set up app theme and colors
- [ ] Create base models (BaseResponse, ErrorResponse)

**Deliverables:**
- ✅ Working HTTP client with authentication
- ✅ Dependency injection configured
- ✅ Secure token storage
- ✅ Connectivity monitoring
- ✅ Base app theme

---

### **Phase 2: Authentication (Week 3)**

#### Data Layer
- [ ] Create auth models (LoginRequest, LoginResponse, User)
- [ ] Implement AuthRepository
- [ ] Implement AuthService API calls

#### Business Logic
- [ ] Create AuthCubit with states
- [ ] Implement login logic
- [ ] Implement logout logic
- [ ] Implement token refresh logic
- [ ] Implement auto-login check

#### Presentation
- [ ] Create SplashScreen with auto-login check
- [ ] Create LoginScreen with form validation
- [ ] Create login form widget
- [ ] Implement loading states
- [ ] Implement error handling

**Deliverables:**
- ✅ Functional login/logout
- ✅ Persistent authentication
- ✅ Token management

---

### **Phase 3: Dashboard & Jobs (Week 4-5)**

#### Week 4: Dashboard
- [ ] Create dashboard models
- [ ] Implement DashboardRepository
- [ ] Create DashboardCubit
- [ ] Build DashboardScreen
- [ ] Create job summary card widget
- [ ] Create active timesheet card widget
- [ ] Create quick actions widget
- [ ] Implement pull-to-refresh

#### Week 5: Job Management
- [ ] Create job models (Job, JobAssignment, Customer)
- [ ] Implement JobRepository
- [ ] Create JobListCubit and JobDetailCubit
- [ ] Build JobListScreen with filters
- [ ] Create job card widget
- [ ] Build JobDetailScreen
- [ ] Create status update actions
- [ ] Implement accept/reject job
- [ ] Create job map view with navigation

**Deliverables:**
- ✅ Working dashboard with real data
- ✅ Job list with filters and search
- ✅ Job details with actions
- ✅ Job status updates
- ✅ Google Maps integration

---

### **Phase 4: Timesheet & Reports (Week 6-7)**

#### Week 6: Time Tracking
- [ ] Create timesheet models
- [ ] Implement TimesheetRepository
- [ ] Create TimesheetCubit
- [ ] Build clock in/out screen
- [ ] Create timer widget
- [ ] Build timesheet history screen
- [ ] Implement GPS tracking during timesheet

#### Week 7: Job Reports
- [ ] Create job report models
- [ ] Implement ReportRepository
- [ ] Create ReportCubit
- [ ] Build create report screen
- [ ] Implement notes editor
- [ ] Create photo capture widget
- [ ] Build signature pad screen
- [ ] Implement image upload
- [ ] Create report submission

**Deliverables:**
- ✅ Time tracking with clock in/out
- ✅ Timesheet history
- ✅ Job report creation with photos
- ✅ Signature capture
- ✅ Report submission

---

### **Phase 5: Materials & Profile (Week 8)**

#### Material Usage
- [ ] Create material models
- [ ] Implement MaterialRepository
- [ ] Create MaterialCubit
- [ ] Build material usage screen
- [ ] Implement inventory search
- [ ] Create material line item widget
- [ ] Implement submission

#### Profile & Settings
- [ ] Create profile models
- [ ] Implement ProfileRepository
- [ ] Create ProfileCubit
- [ ] Build profile screen
- [ ] Implement status updates
- [ ] Create settings screen

**Deliverables:**
- ✅ Material usage tracking
- ✅ Profile management
- ✅ Settings configuration

---

### **Phase 6: Polish & Testing (Week 9-10)**

#### Week 9: UI/UX Polish
- [ ] Refine all UI screens
- [ ] Add loading states everywhere
- [ ] Implement proper error handling
- [ ] Add empty states
- [ ] Improve animations and transitions
- [ ] Add pull-to-refresh on all lists
- [ ] Implement retry mechanisms
- [ ] Add confirmation dialogs

#### Week 10: Testing
- [ ] Write unit tests for cubits
- [ ] Write unit tests for repositories
- [ ] Write widget tests for key screens
- [ ] Write integration tests for critical flows
- [ ] Performance testing
- [ ] User acceptance testing
- [ ] Bug fixes

**Deliverables:**
- ✅ Polished UI with smooth UX
- ✅ Comprehensive error handling
- ✅ Unit and integration tests
- ✅ Beta-ready application

---

## 🧪 Testing Strategy

### Unit Tests
- All Cubits/Blocs
- All Repositories
- Utility functions
- Validators

### Widget Tests
- Key screens (Login, Dashboard, Job List, Job Detail)
- Custom widgets
- Form validations

### Integration Tests
- Complete user flows:
  - Login → View Jobs → Accept Job → Clock In → Complete Job → Submit Report → Logout

### Test Coverage Target
- Minimum: 70%
- Goal: 80%+

---

## 📊 Performance Targets

### App Performance
- **Cold Start**: < 3 seconds
- **API Response Handling**: < 500ms (UI update after response)
- **Image Loading**: Progressive with shimmer
- **List Scrolling**: 60 FPS
- **Memory Usage**: < 150 MB

### Network
- **API Timeout**: 30 seconds
- **Retry Logic**: 3 attempts with exponential backoff
- **Image Compression**: Max 1MB per image

---

## 🔒 Security Considerations

### Authentication
- Store JWT in flutter_secure_storage
- Never log sensitive data
- Clear tokens on logout
- Implement token refresh before expiry

### API Communication
- HTTPS only
- Certificate pinning (optional for production)
- Request/response encryption

### Permissions
- Request only necessary permissions
- Explain permission usage to user
- Handle permission denials gracefully

### Data Protection
- No sensitive data in logs
- Clear cache on logout
- Secure file storage for images

---

## 📝 Configuration

### Environment Variables
```dart
class AppConfig {
  static const String baseUrl = String.fromEnvironment(
    'BASE_URL',
    defaultValue: 'https://localhost:61564',
  );

  static const String jwtIssuer = String.fromEnvironment(
    'JWT_ISSUER',
    defaultValue: 'Vanigam.CRM',
  );

  static const String googleMapsApiKey = String.fromEnvironment(
    'GOOGLE_MAPS_API_KEY',
  );
}
```

### Build Flavors
- **Development**: Dev server, debug logs enabled
- **Staging**: Staging server, limited logs
- **Production**: Production server, no debug logs

---

## 🚀 Deployment Checklist

### Pre-Release
- [ ] All features implemented and tested
- [ ] No critical bugs
- [ ] Performance benchmarks met
- [ ] Security audit passed
- [ ] Privacy policy added
- [ ] Terms of service added
- [ ] App icons configured
- [ ] Splash screens configured

### iOS
- [ ] Bundle ID configured
- [ ] Provisioning profiles set up
- [ ] App Store Connect listing created
- [ ] Screenshots prepared
- [ ] TestFlight beta testing completed

### Android
- [ ] Package name configured
- [ ] Signing keys generated
- [ ] Google Play Console listing created
- [ ] Screenshots prepared
- [ ] Internal testing completed

---

## 📚 Documentation

### Developer Documentation
- API integration guide
- State management patterns
- Code style guide
- Testing guidelines

### User Documentation
- User manual
- FAQ
- Troubleshooting guide
- Video tutorials

---

## 🎯 Success Metrics

### Technical Metrics
- App crash rate < 1%
- API error rate < 5%
- Average API response time < 2 seconds
- App store rating > 4.0

### Business Metrics
- Daily active users
- Jobs completed per day
- Average time to complete job
- User satisfaction score

---

## 🐛 Known Limitations

### Online-Only Application
- ⚠️ **No offline support**: All features require internet connection
- ⚠️ **No local data caching**: Data is always fetched fresh from server
- ⚠️ **No sync queue**: Failed operations must be retried manually

### Workarounds
- Display connectivity banner when offline
- Disable actions when no internet
- Implement retry mechanisms for failed requests
- Show clear error messages

---

## 📞 Support & Contact

### Technical Support
- Email: support@tekspear.com
- Phone: +1-XXX-XXX-XXXX

### Development Team
- Project Lead: [Name]
- Backend Developer: [Name]
- Mobile Developer: [Name]
- QA Engineer: [Name]

---

## 📄 License

Copyright © 2025 TekSpear Solutions. All rights reserved.

---

## 📅 Project Timeline

**Total Duration**: 10 weeks

- **Phase 1**: Week 1-2 (Foundation)
- **Phase 2**: Week 3 (Authentication)
- **Phase 3**: Week 4-5 (Dashboard & Jobs)
- **Phase 4**: Week 6-7 (Timesheet & Reports)
- **Phase 5**: Week 8 (Materials & Profile)
- **Phase 6**: Week 9-10 (Polish & Testing)

**Target Launch**: [Date]

---

_Last Updated: 2025-10-11_
