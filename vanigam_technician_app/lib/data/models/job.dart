import 'package:json_annotation/json_annotation.dart';

part 'job.g.dart';

/// Job model
@JsonSerializable()
class Job {
  final String oid;
  final String number;
  final String title;
  final String? description;
  final JobStatus status;
  final Priority priority;
  final DateTime voucherDate;
  final DateTime? dueDate;
  final String? reference;
  final String? notes;

  // Contact information
  final String? contactId;
  final String? contactName;
  final String? contactPhone;
  final String? contactEmail;

  // Location information
  final String? address;
  final String? city;
  final String? state;
  final String? postalCode;
  final double? latitude;
  final double? longitude;

  // Financial
  final double subTotal;
  final double taxAmount;
  final double discountAmount;
  final double totalAmount;

  const Job({
    required this.oid,
    required this.number,
    required this.title,
    this.description,
    required this.status,
    required this.priority,
    required this.voucherDate,
    this.dueDate,
    this.reference,
    this.notes,
    this.contactId,
    this.contactName,
    this.contactPhone,
    this.contactEmail,
    this.address,
    this.city,
    this.state,
    this.postalCode,
    this.latitude,
    this.longitude,
    this.subTotal = 0.0,
    this.taxAmount = 0.0,
    this.discountAmount = 0.0,
    this.totalAmount = 0.0,
  });

  factory Job.fromJson(Map<String, dynamic> json) => _$JobFromJson(json);
  Map<String, dynamic> toJson() => _$JobToJson(this);

  Job copyWith({
    String? oid,
    String? number,
    String? title,
    String? description,
    JobStatus? status,
    Priority? priority,
    DateTime? voucherDate,
    DateTime? dueDate,
    String? reference,
    String? notes,
    String? contactId,
    String? contactName,
    String? contactPhone,
    String? contactEmail,
    String? address,
    String? city,
    String? state,
    String? postalCode,
    double? latitude,
    double? longitude,
    double? subTotal,
    double? taxAmount,
    double? discountAmount,
    double? totalAmount,
  }) {
    return Job(
      oid: oid ?? this.oid,
      number: number ?? this.number,
      title: title ?? this.title,
      description: description ?? this.description,
      status: status ?? this.status,
      priority: priority ?? this.priority,
      voucherDate: voucherDate ?? this.voucherDate,
      dueDate: dueDate ?? this.dueDate,
      reference: reference ?? this.reference,
      notes: notes ?? this.notes,
      contactId: contactId ?? this.contactId,
      contactName: contactName ?? this.contactName,
      contactPhone: contactPhone ?? this.contactPhone,
      contactEmail: contactEmail ?? this.contactEmail,
      address: address ?? this.address,
      city: city ?? this.city,
      state: state ?? this.state,
      postalCode: postalCode ?? this.postalCode,
      latitude: latitude ?? this.latitude,
      longitude: longitude ?? this.longitude,
      subTotal: subTotal ?? this.subTotal,
      taxAmount: taxAmount ?? this.taxAmount,
      discountAmount: discountAmount ?? this.discountAmount,
      totalAmount: totalAmount ?? this.totalAmount,
    );
  }

  /// Get full address
  String get fullAddress {
    final parts = <String>[];
    if (address != null && address!.isNotEmpty) parts.add(address!);
    if (city != null && city!.isNotEmpty) parts.add(city!);
    if (state != null && state!.isNotEmpty) parts.add(state!);
    if (postalCode != null && postalCode!.isNotEmpty) parts.add(postalCode!);
    return parts.join(', ');
  }

  /// Check if job has location
  bool get hasLocation => latitude != null && longitude != null;

  /// Check if job is overdue
  bool get isOverdue {
    if (dueDate == null) return false;
    return DateTime.now().isAfter(dueDate!) &&
           status != JobStatus.completed &&
           status != JobStatus.cancelled;
  }
}

/// Job status enum
enum JobStatus {
  @JsonValue('Pending')
  pending,
  @JsonValue('Assigned')
  assigned,
  @JsonValue('Scheduled')
  scheduled,
  @JsonValue('InProgress')
  inProgress,
  @JsonValue('OnHold')
  onHold,
  @JsonValue('Completed')
  completed,
  @JsonValue('Cancelled')
  cancelled,
  @JsonValue('Closed')
  closed,
}

/// Priority enum
enum Priority {
  @JsonValue('Low')
  low,
  @JsonValue('Normal')
  normal,
  @JsonValue('High')
  high,
  @JsonValue('Critical')
  critical,
}

/// Job status extension
extension JobStatusExtension on JobStatus {
  String get displayName {
    switch (this) {
      case JobStatus.pending:
        return 'Pending';
      case JobStatus.assigned:
        return 'Assigned';
      case JobStatus.scheduled:
        return 'Scheduled';
      case JobStatus.inProgress:
        return 'In Progress';
      case JobStatus.onHold:
        return 'On Hold';
      case JobStatus.completed:
        return 'Completed';
      case JobStatus.cancelled:
        return 'Cancelled';
      case JobStatus.closed:
        return 'Closed';
    }
  }

  bool get isActive {
    return this == JobStatus.assigned ||
        this == JobStatus.scheduled ||
        this == JobStatus.inProgress;
  }

  bool get canStart {
    return this == JobStatus.assigned || this == JobStatus.scheduled;
  }

  bool get canComplete {
    return this == JobStatus.inProgress;
  }
}

/// Priority extension
extension PriorityExtension on Priority {
  String get displayName {
    switch (this) {
      case Priority.low:
        return 'Low';
      case Priority.normal:
        return 'Normal';
      case Priority.high:
        return 'High';
      case Priority.critical:
        return 'Critical';
    }
  }

  int get order {
    switch (this) {
      case Priority.low:
        return 0;
      case Priority.normal:
        return 1;
      case Priority.high:
        return 2;
      case Priority.critical:
        return 3;
    }
  }
}
