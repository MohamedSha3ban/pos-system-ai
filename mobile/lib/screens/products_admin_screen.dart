import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/admin_product.dart';
import '../models/category.dart';
import '../services/admin_product_service.dart';
import '../services/category_service.dart';
import 'product_form_screen.dart';

const String defaultLocationId = '00000000-0000-0000-0000-000000000000';

/// Admin CRUD screen for products + categories -- the Flutter counterpart to the
/// Angular web app's features/products admin page.
class ProductsAdminScreen extends StatefulWidget {
  const ProductsAdminScreen({super.key});

  @override
  State<ProductsAdminScreen> createState() => _ProductsAdminScreenState();
}

class _ProductsAdminScreenState extends State<ProductsAdminScreen> with SingleTickerProviderStateMixin {
  final _productService = AdminProductService();
  final _categoryService = CategoryService();
  final _currency = NumberFormat.currency(symbol: '\$');

  List<AdminProduct> _products = [];
  List<Category> _categories = [];
  bool _loading = true;
  String? _error;
  late TabController _tabController;
  final _newCategoryController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _reload();
  }

  Future<void> _reload() async {
    setState(() => _loading = true);
    try {
      final products = await _productService.getCatalog(defaultLocationId);
      final categories = await _categoryService.getAll();
      setState(() { _products = products; _categories = categories; _loading = false; });
    } catch (e) {
      setState(() { _error = 'Could not load data.'; _loading = false; });
    }
  }

  Future<void> _openForm({AdminProduct? product}) async {
    final saved = await Navigator.of(context).push<bool>(
      MaterialPageRoute(builder: (_) => ProductFormScreen(product: product, categories: _categories)),
    );
    if (saved == true) _reload();
  }

  Future<void> _deleteProduct(AdminProduct p) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Delete product'),
        content: Text('Delete "${p.name}"? This can\'t be undone.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancel')),
          TextButton(onPressed: () => Navigator.pop(context, true), child: const Text('Delete')),
        ],
      ),
    );
    if (confirmed == true) {
      await _productService.delete(p.id);
      _reload();
    }
  }

  Future<void> _addCategory() async {
    if (_newCategoryController.text.trim().isEmpty) return;
    await _categoryService.create(_newCategoryController.text.trim());
    _newCategoryController.clear();
    _reload();
  }

  Future<void> _deleteCategory(Category c) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Delete category'),
        content: Text('Delete "${c.name}"? Products keep their data but lose this category.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancel')),
          TextButton(onPressed: () => Navigator.pop(context, true), child: const Text('Delete')),
        ],
      ),
    );
    if (confirmed == true) {
      await _categoryService.delete(c.id);
      _reload();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Catalog'),
        bottom: TabBar(controller: _tabController, tabs: const [Tab(text: 'Products'), Tab(text: 'Categories')]),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _tabController.index == 0 ? _openForm() : null,
        child: const Icon(Icons.add),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : TabBarView(
              controller: _tabController,
              children: [_buildProductsTab(), _buildCategoriesTab()],
            ),
    );
  }

  Widget _buildProductsTab() {
    if (_products.isEmpty) return const Center(child: Text('No products yet.'));
    return ListView.builder(
      itemCount: _products.length,
      itemBuilder: (context, index) {
        final p = _products[index];
        return ListTile(
          title: Text(p.name),
          subtitle: Text('${p.sku} · ${p.categoryName ?? 'No category'} · ${p.quantityOnHand} in stock'),
          trailing: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(_currency.format(p.price)),
              IconButton(icon: const Icon(Icons.edit, size: 20), onPressed: () => _openForm(product: p)),
              IconButton(icon: const Icon(Icons.delete, size: 20, color: Colors.red), onPressed: () => _deleteProduct(p)),
            ],
          ),
        );
      },
    );
  }

  Widget _buildCategoriesTab() {
    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.all(12),
          child: Row(
            children: [
              Expanded(
                child: TextField(
                  controller: _newCategoryController,
                  decoration: const InputDecoration(labelText: 'New category name'),
                  onSubmitted: (_) => _addCategory(),
                ),
              ),
              IconButton(icon: const Icon(Icons.add_circle), onPressed: _addCategory),
            ],
          ),
        ),
        Expanded(
          child: _categories.isEmpty
              ? const Center(child: Text('No categories yet.'))
              : ListView.builder(
                  itemCount: _categories.length,
                  itemBuilder: (context, index) {
                    final c = _categories[index];
                    return ListTile(
                      title: Text(c.name),
                      subtitle: Text('${c.productCount} products'),
                      trailing: IconButton(icon: const Icon(Icons.delete, color: Colors.red), onPressed: () => _deleteCategory(c)),
                    );
                  },
                ),
        ),
      ],
    );
  }
}
