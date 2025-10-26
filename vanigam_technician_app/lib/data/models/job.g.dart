// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'job.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Job _$JobFromJson(Map<String, dynamic> json) => Job(
      oid: json['oid'] as String,
      number: json['number'] as String,
      title: json['title'] as String,
      description: json['description'] as String?,
      status: $enumDecode(_$JobStatusEnumMap, json['status']),
      priority: $enumDecode(_$PriorityEnumMap, json['priority']),
      voucherDate: DateTime.parse(json['voucherDate'] as String),
      dueDate: json['dueDate'] == null
          ? null
          : DateTime.parse(json['dueDate'] as String),
      reference: json['reference'] as String?,
      notes: json['notes'] as String?,
      contactId: json['contactId'] as String?,
      contactName: json['contactName'] as String?,
      contactPhone: json['contactPhone'] as String?,
      contactEmail: json['contactEmail'] as String?,
      address: json['address'] as String?,
      city: json['city'] as String?,
      state: json['state'] as String?,
      postalCode: json['postalCode'] as String?,
      latitude: (json['latitude'] as num?)?.toDouble(),
      longitude: (json['longitude'] as num?)?.toDouble(),
      subTotal: (json['subTotal'] as num?)?.toDouble() ?? 0.0,
      taxAmount: (json['taxAmount'] as num?)?.toDouble() ?? 0.0,
      discountAmount: (json['discountAmount'] as num?)?.toDouble() ?? 0.0,
      totalAmount: (json['totalAmount'] as num?)?.toDouble() ?? 0.0,
    );

Map<String, dynamic> _$JobToJson(Job instance) => <String, dynamic>{
      'oid': instance.oid,
      'number': instance.number,
      'title': instance.title,
      'description': instance.description,
      'status': _$JobStatusEnumMap[instance.status]!,
      'priority': _$PriorityEnumMap[instance.priority]!,
      'voucherDate': instance.voucherDate.toIso8601String(),
      'dueDate': instance.dueDate?.toIso8601String(),
      'reference': instance.reference,
      'notes': instance.notes,
      'contactId': instance.contactId,
      'contactName': instance.contactName,
      'contactPhone': instance.contactPhone,
      'contactEmail': instance.contactEmail,
      'address': instance.address,
      'city': instance.city,
      'state': instance.state,
      'postalCode': instance.postalCode,
      'latitude': instance.latitude,
      'longitude': instance.longitude,
      'subTotal': instance.subTotal,
      'taxAmount': instance.taxAmount,
      'discountAmount': instance.discountAmount,
      'totalAmount': instance.totalAmount,
    };

const _$JobStatusEnumMap = {
  JobStatus.pending: 'Pending',
  JobStatus.assigned: 'Assigned',
  JobStatus.scheduled: 'Scheduled',
  JobStatus.inProgress: 'InProgress',
  JobStatus.onHold: 'OnHold',
  JobStatus.completed: 'Completed',
  JobStatus.cancelled: 'Cancelled',
  JobStatus.closed: 'Closed',
};

const _$PriorityEnumMap = {
  Priority.low: 'Low',
  Priority.normal: 'Normal',
  Priority.high: 'High',
  Priority.critical: 'Critical',
};
