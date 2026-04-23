import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';
import '../../../shared/dependency_provider.dart';
import '../../../shared/models/api_models.dart';

class ProductsPage extends StatefulWidget {
  const ProductsPage({super.key});

  @override
  State<ProductsPage> createState() => _ProductsPageState();
}

class _ProductsPageState extends State<ProductsPage> {
  final TextEditingController _searchCtrl = TextEditingController();
  bool _loading = true;
  PagedResponse<ProductResponse>? _response;
  bool? _isActiveFilter;
  int _currentPage = 1;

  @override
  void initState() {
    super.initState();
    _loadProducts();
  }

  Future<void> _loadProducts() async {
    setState(() => _loading = true);
    try {
      final service = DependencyProvider.of(context).productService;
      final res = await service.getProducts(
        page: _currentPage,
        searchTerm: _searchCtrl.text,
        isActive: _isActiveFilter,
      );
      setState(() {
        _response = res;
        _loading = false;
      });
    } catch (e) {
      setState(() => _loading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Erro ao carregar produtos')),
        );
      }
    }
  }

  void _onSearch(String value) {
    _currentPage = 1;
    _loadProducts();
  }

  void _toggleFilter(bool? value) {
    setState(() {
      _isActiveFilter = value;
      _currentPage = 1;
    });
    _loadProducts();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.gray50,
      appBar: AppBar(
        title: const Text('Produtos', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 24)),
        actions: [
          IconButton(onPressed: _loadProducts, icon: const Icon(Icons.refresh)),
        ],
      ),
      body: Column(
        children: [
          // ─── Filter Bar ─────────────────────────────────────────
          Container(
            padding: const EdgeInsets.all(20),
            color: Colors.white,
            child: Column(
              children: [
                TextField(
                  controller: _searchCtrl,
                  onSubmitted: _onSearch,
                  decoration: InputDecoration(
                    hintText: 'Buscar produtos...',
                    prefixIcon: const Icon(Icons.search, color: AppTheme.primary600),
                    filled: true,
                    fillColor: AppTheme.gray50,
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(20),
                      borderSide: BorderSide.none,
                    ),
                  ),
                ),
                const SizedBox(height: 16),
                SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      _FilterChip(
                        label: 'Todos',
                        isSelected: _isActiveFilter == null,
                        onTap: () => _toggleFilter(null),
                      ),
                      const SizedBox(width: 8),
                      _FilterChip(
                        label: 'Ativos',
                        isSelected: _isActiveFilter == true,
                        onTap: () => _toggleFilter(true),
                      ),
                      const SizedBox(width: 8),
                      _FilterChip(
                        label: 'Inativos',
                        isSelected: _isActiveFilter == false,
                        onTap: () => _toggleFilter(false),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),

          // ─── Products List ──────────────────────────────────────
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : _response == null || _response!.items.isEmpty
                    ? const Center(child: Text('Nenhum produto encontrado'))
                    : ListView.builder(
                        padding: const EdgeInsets.all(20),
                        itemCount: _response!.items.length,
                        itemBuilder: (context, index) {
                          final product = _response!.items[index];
                          return Container(
                            margin: const EdgeInsets.only(bottom: 16),
                            decoration: BoxDecoration(
                              color: Colors.white,
                              borderRadius: BorderRadius.circular(24),
                              boxShadow: [
                                BoxShadow(
                                  color: AppTheme.gray200.withValues(alpha: 0.3),
                                  blurRadius: 10,
                                  offset: const Offset(0, 4),
                                ),
                              ],
                            ),
                            child: Padding(
                              padding: const EdgeInsets.all(16),
                              child: Row(
                                children: [
                                  Container(
                                    width: 64,
                                    height: 64,
                                    decoration: BoxDecoration(
                                      color: AppTheme.primary50,
                                      borderRadius: BorderRadius.circular(18),
                                    ),
                                    child: const Center(child: Text('📦', style: TextStyle(fontSize: 24))),
                                  ),
                                  const SizedBox(width: 16),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          product.name,
                                          style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 16, color: AppTheme.gray900),
                                        ),
                                        Text(
                                          product.category,
                                          style: const TextStyle(color: AppTheme.gray500, fontSize: 12, fontWeight: FontWeight.bold),
                                        ),
                                        const SizedBox(height: 8),
                                        _StatusBadge(isActive: product.isActive),
                                      ],
                                    ),
                                  ),
                                  Column(
                                    crossAxisAlignment: CrossAxisAlignment.end,
                                    children: [
                                      Text(
                                        'R\$ ${product.price.toStringAsFixed(2)}',
                                        style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 18, color: AppTheme.primary600),
                                      ),
                                      Text(
                                        'Stock: ${product.stock}',
                                        style: const TextStyle(color: AppTheme.gray400, fontSize: 11, fontWeight: FontWeight.bold),
                                      ),
                                    ],
                                  ),
                                ],
                              ),
                            ),
                          );
                        },
                      ),
          ),
        ],
      ),
    );
  }
}

class _FilterChip extends StatelessWidget {
  final String label;
  final bool isSelected;
  final VoidCallback onTap;

  const _FilterChip({required this.label, required this.isSelected, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
        decoration: BoxDecoration(
          color: isSelected ? AppTheme.primary600 : AppTheme.gray50,
          borderRadius: BorderRadius.circular(14),
        ),
        child: Text(
          label,
          style: TextStyle(
            color: isSelected ? Colors.white : AppTheme.gray500,
            fontWeight: FontWeight.bold,
            fontSize: 12,
          ),
        ),
      ),
    );
  }
}

class _StatusBadge extends StatelessWidget {
  final bool isActive;
  const _StatusBadge({required this.isActive});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: isActive ? const Color(0xFFECFDF5) : const Color(0xFFFEF2F2),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(
        isActive ? 'ATIVO' : 'INATIVO',
        style: TextStyle(
          color: isActive ? const Color(0xFF059669) : const Color(0xFFDC2626),
          fontSize: 9,
          fontWeight: FontWeight.w900,
          letterSpacing: 1.1,
        ),
      ),
    );
  }
}
