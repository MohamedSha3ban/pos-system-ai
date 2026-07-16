class OrderResult {
  final String id;
  final String status;
  final double grandTotal;

  OrderResult({required this.id, required this.status, required this.grandTotal});

  factory OrderResult.fromJson(Map<String, dynamic> json) => OrderResult(
        id: json['id'],
        status: json['status'],
        grandTotal: (json['grandTotal'] as num).toDouble(),
      );
}
