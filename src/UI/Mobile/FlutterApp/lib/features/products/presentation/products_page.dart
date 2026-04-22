import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';

class ProductsPage extends StatefulWidget {
  const ProductsPage({super.key});

  @override
  State<ProductsPage> createState() => _ProductsPageState();
}

class _ProductsPageState extends State<ProductsPage> {
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadProducts();
  }

  Future<void> _loadProducts() async {
    setState(() => _loading = true);
    await Future.delayed(const Duration(seconds: 1)); // Simulação
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Produtos')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : ListView.builder(
              padding: const EdgeInsets.all(20),
              itemCount: 10,
              itemBuilder: (context, index) {
                return Card(
                  margin: const EdgeInsets.only(bottom: 12),
                  child: ListTile(
                    leading: Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: AppTheme.primary50,
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: const Icon(Icons.inventory_2_outlined, color: AppTheme.primary600),
                    ),
                    title: Text('Produto Enterprise $index', style: const TextStyle(fontWeight: FontWeight.bold)),
                    subtitle: const Text('Categoria | SKU: 123456'),
                    trailing: Text('R\$ 299,90', style: const TextStyle(fontWeight: FontWeight.bold, color: AppTheme.primary600)),
                  ),
                );
              },
            ),
    );
  }
}
