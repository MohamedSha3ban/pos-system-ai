class Category {
  final String id;
  final String name;
  final int productCount;

  Category({required this.id, required this.name, required this.productCount});

  factory Category.fromJson(Map<String, dynamic> json) => Category(
        id: json['id'],
        name: json['name'],
        productCount: json['productCount'] ?? 0,
      );
}
