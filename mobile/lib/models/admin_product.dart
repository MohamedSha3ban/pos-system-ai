class AdminProduct {
  final String id;
  final String name;
  final String? description;
  final String sku;
  final String? barcode;
  final double price;
  final double? costPrice;
  final String? categoryId;
  final String? categoryName;
  final bool isActive;
  final int quantityOnHand;

  AdminProduct({
    required this.id,
    required this.name,
    this.description,
    required this.sku,
    this.barcode,
    required this.price,
    this.costPrice,
    this.categoryId,
    this.categoryName,
    required this.isActive,
    required this.quantityOnHand,
  });

  factory AdminProduct.fromJson(Map<String, dynamic> json) => AdminProduct(
        id: json['id'],
        name: json['name'],
        description: json['description'],
        sku: json['sku'],
        barcode: json['barcode'],
        price: (json['price'] as num).toDouble(),
        costPrice: json['costPrice'] != null ? (json['costPrice'] as num).toDouble() : null,
        categoryId: json['categoryId'],
        categoryName: json['categoryName'],
        isActive: json['isActive'] ?? true,
        quantityOnHand: json['quantityOnHand'] ?? 0,
      );

  Map<String, dynamic> toUpsertJson() => {
        'name': name,
        'description': description,
        'sku': sku,
        'barcode': barcode,
        'price': price,
        'costPrice': costPrice,
        'categoryId': categoryId,
        'isActive': isActive,
      };
}
