import 'package:flutter/material.dart';
import '../models/admin_product.dart';
import '../models/category.dart';
import '../services/admin_product_service.dart';

const String defaultLocationId = '00000000-0000-0000-0000-000000000000';

/// Add/edit form. Pass an existing [product] to edit, or null to create.
class ProductFormScreen extends StatefulWidget {
  final AdminProduct? product;
  final List<Category> categories;

  const ProductFormScreen({super.key, this.product, required this.categories});

  @override
  State<ProductFormScreen> createState() => _ProductFormScreenState();
}

class _ProductFormScreenState extends State<ProductFormScreen> {
  final _service = AdminProductService();
  final _formKey = GlobalKey<FormState>();

  late TextEditingController _name;
  late TextEditingController _sku;
  late TextEditingController _barcode;
  late TextEditingController _description;
  late TextEditingController _price;
  late TextEditingController _costPrice;
  late TextEditingController _initialQuantity;
  String? _categoryId;
  bool _isActive = true;
  bool _saving = false;
  String? _error;

  bool get _isEditing => widget.product != null;

  @override
  void initState() {
    super.initState();
    final p = widget.product;
    _name = TextEditingController(text: p?.name ?? '');
    _sku = TextEditingController(text: p?.sku ?? '');
    _barcode = TextEditingController(text: p?.barcode ?? '');
    _description = TextEditingController(text: p?.description ?? '');
    _price = TextEditingController(text: p?.price.toString() ?? '');
    _costPrice = TextEditingController(text: p?.costPrice?.toString() ?? '');
    _initialQuantity = TextEditingController(text: '0');
    _categoryId = p?.categoryId;
    _isActive = p?.isActive ?? true;
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _saving = true; _error = null; });

    final payload = {
      'name': _name.text,
      'description': _description.text.isEmpty ? null : _description.text,
      'sku': _sku.text,
      'barcode': _barcode.text.isEmpty ? null : _barcode.text,
      'price': double.tryParse(_price.text) ?? 0,
      'costPrice': _costPrice.text.isEmpty ? null : double.tryParse(_costPrice.text),
      'categoryId': _categoryId,
      'isActive': _isActive,
    };

    try {
      if (_isEditing) {
        await _service.update(widget.product!.id, payload, defaultLocationId);
      } else {
        final qty = int.tryParse(_initialQuantity.text) ?? 0;
        await _service.create(payload, defaultLocationId, qty);
      }
      if (!mounted) return;
      Navigator.of(context).pop(true);
    } catch (e) {
      setState(() => _error = 'Could not save product.');
    } finally {
      setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(_isEditing ? 'Edit product' : 'New product')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(controller: _name, decoration: const InputDecoration(labelText: 'Name'),
                validator: (v) => (v == null || v.isEmpty) ? 'Required' : null),
            TextFormField(controller: _sku, decoration: const InputDecoration(labelText: 'SKU'),
                validator: (v) => (v == null || v.isEmpty) ? 'Required' : null),
            TextFormField(controller: _barcode, decoration: const InputDecoration(labelText: 'Barcode')),
            TextFormField(controller: _description, decoration: const InputDecoration(labelText: 'Description'), maxLines: 3),
            TextFormField(controller: _price, decoration: const InputDecoration(labelText: 'Price'),
                keyboardType: const TextInputType.numberWithOptions(decimal: true),
                validator: (v) => (double.tryParse(v ?? '') == null) ? 'Enter a valid number' : null),
            TextFormField(controller: _costPrice, decoration: const InputDecoration(labelText: 'Cost price (optional)'),
                keyboardType: const TextInputType.numberWithOptions(decimal: true)),
            DropdownButtonFormField<String?>(
              value: _categoryId,
              decoration: const InputDecoration(labelText: 'Category'),
              items: [
                const DropdownMenuItem(value: null, child: Text('— None —')),
                ...widget.categories.map((c) => DropdownMenuItem(value: c.id, child: Text(c.name))),
              ],
              onChanged: (v) => setState(() => _categoryId = v),
            ),
            if (!_isEditing)
              TextFormField(controller: _initialQuantity, decoration: const InputDecoration(labelText: 'Initial stock'),
                  keyboardType: TextInputType.number),
            SwitchListTile(
              title: const Text('Active'),
              value: _isActive,
              onChanged: (v) => setState(() => _isActive = v),
            ),
            const SizedBox(height: 16),
            if (_error != null) Text(_error!, style: const TextStyle(color: Colors.red)),
            FilledButton(
              onPressed: _saving ? null : _save,
              child: _saving
                  ? const SizedBox(height: 18, width: 18, child: CircularProgressIndicator(strokeWidth: 2))
                  : const Text('Save'),
            ),
          ],
        ),
      ),
    );
  }
}
