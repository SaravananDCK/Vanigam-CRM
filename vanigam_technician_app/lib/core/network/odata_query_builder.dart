/// OData Query Builder for constructing OData queries
class ODataQueryBuilder {
  final List<String> _filters = [];
  final List<String> _expands = [];
  final List<String> _selects = [];
  String? _orderBy;
  int? _top;
  int? _skip;
  bool _count = false;

  /// Add filter condition
  /// Example: filter('Status eq \'Assigned\'')
  ODataQueryBuilder filter(String condition) {
    _filters.add(condition);
    return this;
  }

  /// Filter by equality
  /// Example: filterEq('Status', 'Assigned')
  ODataQueryBuilder filterEq(String field, dynamic value) {
    if (value is String) {
      _filters.add("$field eq '$value'");
    } else {
      _filters.add("$field eq $value");
    }
    return this;
  }

  /// Filter by not equal
  /// Example: filterNe('Status', 'Cancelled')
  ODataQueryBuilder filterNe(String field, dynamic value) {
    if (value is String) {
      _filters.add("$field ne '$value'");
    } else {
      _filters.add("$field ne $value");
    }
    return this;
  }

  /// Filter by greater than
  ODataQueryBuilder filterGt(String field, dynamic value) {
    _filters.add("$field gt $value");
    return this;
  }

  /// Filter by greater than or equal
  ODataQueryBuilder filterGe(String field, dynamic value) {
    _filters.add("$field ge $value");
    return this;
  }

  /// Filter by less than
  ODataQueryBuilder filterLt(String field, dynamic value) {
    _filters.add("$field lt $value");
    return this;
  }

  /// Filter by less than or equal
  ODataQueryBuilder filterLe(String field, dynamic value) {
    _filters.add("$field le $value");
    return this;
  }

  /// Filter by contains (substring search)
  /// Example: filterContains('Name', 'Tech')
  ODataQueryBuilder filterContains(String field, String value) {
    _filters.add("contains($field, '$value')");
    return this;
  }

  /// Filter by startsWith
  ODataQueryBuilder filterStartsWith(String field, String value) {
    _filters.add("startswith($field, '$value')");
    return this;
  }

  /// Filter by endsWith
  ODataQueryBuilder filterEndsWith(String field, String value) {
    _filters.add("endswith($field, '$value')");
    return this;
  }

  /// Filter by null check
  ODataQueryBuilder filterIsNull(String field) {
    _filters.add("$field eq null");
    return this;
  }

  /// Filter by not null check
  ODataQueryBuilder filterIsNotNull(String field) {
    _filters.add("$field ne null");
    return this;
  }

  /// Add AND condition between filters
  ODataQueryBuilder and() {
    if (_filters.isNotEmpty) {
      _filters.add('and');
    }
    return this;
  }

  /// Add OR condition between filters
  ODataQueryBuilder or() {
    if (_filters.isNotEmpty) {
      _filters.add('or');
    }
    return this;
  }

  /// Expand related entities
  /// Example: expand('Customer,Contact')
  ODataQueryBuilder expand(String relations) {
    _expands.add(relations);
    return this;
  }

  /// Select specific fields
  /// Example: select('Id,Name,Status')
  ODataQueryBuilder select(String fields) {
    _selects.add(fields);
    return this;
  }

  /// Order by field (ascending by default)
  /// Example: orderBy('CreatedAt desc')
  ODataQueryBuilder orderBy(String field) {
    _orderBy = field;
    return this;
  }

  /// Order by ascending
  ODataQueryBuilder orderByAsc(String field) {
    _orderBy = '$field asc';
    return this;
  }

  /// Order by descending
  ODataQueryBuilder orderByDesc(String field) {
    _orderBy = '$field desc';
    return this;
  }

  /// Limit number of results
  ODataQueryBuilder top(int count) {
    _top = count;
    return this;
  }

  /// Skip number of results (for pagination)
  ODataQueryBuilder skip(int count) {
    _skip = count;
    return this;
  }

  /// Include count in response
  ODataQueryBuilder count([bool include = true]) {
    _count = include;
    return this;
  }

  /// Build the query parameters map
  Map<String, dynamic> build() {
    final Map<String, dynamic> params = {};

    // Build filter string
    if (_filters.isNotEmpty) {
      params['\$filter'] = _filters.join(' ');
    }

    // Build expand string
    if (_expands.isNotEmpty) {
      params['\$expand'] = _expands.join(',');
    }

    // Build select string
    if (_selects.isNotEmpty) {
      params['\$select'] = _selects.join(',');
    }

    // Add orderBy
    if (_orderBy != null) {
      params['\$orderby'] = _orderBy;
    }

    // Add top
    if (_top != null) {
      params['\$top'] = _top;
    }

    // Add skip
    if (_skip != null) {
      params['\$skip'] = _skip;
    }

    // Add count
    if (_count) {
      params['\$count'] = 'true';
    }

    return params;
  }

  /// Build query string (for URL)
  String buildQueryString() {
    final params = build();
    if (params.isEmpty) return '';

    final queryParts = params.entries
        .map((e) => '${e.key}=${Uri.encodeComponent(e.value.toString())}')
        .toList();

    return '?${queryParts.join('&')}';
  }

  /// Reset the builder
  void reset() {
    _filters.clear();
    _expands.clear();
    _selects.clear();
    _orderBy = null;
    _top = null;
    _skip = null;
    _count = false;
  }
}

/// OData Query Builder Extensions
extension ODataQueryBuilderExtensions on ODataQueryBuilder {
  /// Common filter for technician's assigned jobs
  ODataQueryBuilder forTechnician(String technicianId) {
    return filterEq('TechnicianId', technicianId);
  }

  /// Common filter for active jobs (not completed/cancelled)
  ODataQueryBuilder activeJobs() {
    return filter("Status ne 'Completed' and Status ne 'Cancelled'");
  }

  /// Common filter for today's jobs
  ODataQueryBuilder forToday() {
    final today = DateTime.now();
    final startOfDay = DateTime(today.year, today.month, today.day);
    final endOfDay = startOfDay.add(const Duration(days: 1));

    return filter(
        "CreatedAtUtc ge ${startOfDay.toIso8601String()} and CreatedAtUtc lt ${endOfDay.toIso8601String()}");
  }

  /// Pagination helper
  ODataQueryBuilder paginate(int page, int pageSize) {
    return skip((page - 1) * pageSize).top(pageSize);
  }
}
