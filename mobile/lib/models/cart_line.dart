import 'product.dart';

class CartLine {
  final Product product;
  int quantity;

  CartLine({required this.product, this.quantity = 1});

  double get lineTotal => product.price * quantity;
}
