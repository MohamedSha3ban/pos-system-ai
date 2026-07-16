import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/product.dart';
import '../models/cart_line.dart';
import '../services/product_service.dart';
import '../services/order_service.dart';

// TODO: replace with the real active-location id from tenant/session context.
const String defaultLocationId = '00000000-0000-0000-0000-000000000000';

class PosScreen extends StatefulWidget {
  const PosScreen({super.key});

  @override
  State<PosScreen> createState() => _PosScreenState();
}

class _PosScreenState extends State<PosScreen> {
  final _productService = ProductService();
  final _orderService = OrderService();
  final _currency = NumberFormat.currency(symbol: '\$');

  List<Product> _products = [];
  final List<CartLine> _cart = [];
  String _paymentMethod = 'Cash';
  final List<String> _paymentMethods = ['Cash', 'CardPresent', 'ApplePay', 'GooglePay', 'QrBankTransfer'];
  bool _loading = true;
  String? _error;
  String? _successMessage;

  @override
  void initState() {
    super.initState();
    _loadCatalog();
  }

  Future<void> _loadCatalog() async {
    try {
      final products = await _productService.getCatalog(defaultLocationId);
      setState(() { _products = products; _loading = false; });
    } catch (e) {
      setState(() { _error = 'Could not load catalog.'; _loading = false; });
    }
  }

  void _addToCart(Product product) {
    setState(() {
      final existing = _cart.where((c) => c.product.id == product.id).toList();
      if (existing.isNotEmpty) {
        existing.first.quantity++;
      } else {
        _cart.add(CartLine(product: product));
      }
    });
  }

  double get _subtotal => _cart.fold(0, (sum, line) => sum + line.lineTotal);

  Future<void> _checkout() async {
    if (_cart.isEmpty) return;
    setState(() { _error = null; _successMessage = null; });
    try {
      final order = await _orderService.checkout(
        locationId: defaultLocationId,
        cart: _cart,
        paymentMethod: _paymentMethod,
        amount: _subtotal,
      );
      setState(() {
        _successMessage = 'Charged ${_currency.format(order.grandTotal)}';
        _cart.clear();
      });
    } catch (e) {
      setState(() => _error = 'Checkout failed. Please try again.');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('POS')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : Column(
              children: [
                Expanded(
                  child: GridView.builder(
                    padding: const EdgeInsets.all(12),
                    gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 2,
                      mainAxisSpacing: 10,
                      crossAxisSpacing: 10,
                      childAspectRatio: 1.4,
                    ),
                    itemCount: _products.length,
                    itemBuilder: (context, index) {
                      final p = _products[index];
                      return Card(
                        child: InkWell(
                          onTap: () => _addToCart(p),
                          child: Padding(
                            padding: const EdgeInsets.all(12),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(p.name, style: const TextStyle(fontWeight: FontWeight.bold)),
                                const Spacer(),
                                Text(_currency.format(p.price)),
                                Text('${p.quantityOnHand} in stock',
                                    style: TextStyle(color: p.quantityOnHand <= 5 ? Colors.red : Colors.grey)),
                              ],
                            ),
                          ),
                        ),
                      );
                    },
                  ),
                ),
                _buildCartPanel(),
              ],
            ),
    );
  }

  Widget _buildCartPanel() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border(top: BorderSide(color: Colors.grey.shade300)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          ..._cart.map((line) => Padding(
                padding: const EdgeInsets.symmetric(vertical: 2),
                child: Row(
                  children: [
                    Expanded(child: Text('${line.product.name} x${line.quantity}')),
                    Text(_currency.format(line.lineTotal)),
                  ],
                ),
              )),
          const Divider(),
          Text('Total: ${_currency.format(_subtotal)}', style: const TextStyle(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          DropdownButtonFormField<String>(
            value: _paymentMethod,
            items: _paymentMethods.map((m) => DropdownMenuItem(value: m, child: Text(m))).toList(),
            onChanged: (v) => setState(() => _paymentMethod = v ?? _paymentMethod),
            decoration: const InputDecoration(labelText: 'Payment method'),
          ),
          const SizedBox(height: 8),
          FilledButton(
            onPressed: _cart.isEmpty ? null : _checkout,
            child: const Text('Charge'),
          ),
          if (_error != null) Text(_error!, style: const TextStyle(color: Colors.red)),
          if (_successMessage != null) Text(_successMessage!, style: const TextStyle(color: Colors.green)),
        ],
      ),
    );
  }
}
