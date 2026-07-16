class Product {
  final String id;
  final String name;
  final String sku;
  final double price;
  final int quantityOnHand;
  final String? categoryName;

  Product({
    required this.id,
    required this.name,
    required this.sku,
    required this.price,
    required this.quantityOnHand,
    this.categoryName,
  });

  factory Product.fromJson(Map<String, dynamic> json) => Product(
        id: json['id'],
        name: json['name'],
        sku: json['sku'],
        price: (json['price'] as num).toDouble(),
        quantityOnHand: json['quantityOnHand'] ?? 0,
        categoryName: json['categoryName'],
      );
}
