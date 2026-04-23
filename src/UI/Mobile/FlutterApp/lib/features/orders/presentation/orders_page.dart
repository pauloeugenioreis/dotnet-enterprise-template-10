import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';
import '../../../shared/dependency_provider.dart';
import '../../../shared/models/api_models.dart';

class OrdersPage extends StatefulWidget {
  const OrdersPage({super.key});

  @override
  State<OrdersPage> createState() => _OrdersPageState();
}

class _OrdersPageState extends State<OrdersPage> {
  final TextEditingController _searchCtrl = TextEditingController();
  bool _loading = true;
  PagedResponse<OrderResponse>? _response;
  String? _statusFilter;
  DateTime? _fromDate;
  DateTime? _toDate;
  int _currentPage = 1;

  @override
  void initState() {
    super.initState();
    _loadOrders();
  }

  Future<void> _loadOrders() async {
    setState(() => _loading = true);
    try {
      final service = DependencyProvider.of(context).orderService;
      final res = await service.getOrders(
        page: _currentPage,
        searchTerm: _searchCtrl.text,
        status: _statusFilter,
        fromDate: _fromDate,
        toDate: _toDate,
      );
      setState(() {
        _response = res;
        _loading = false;
      });
    } catch (e) {
      setState(() => _loading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Erro ao carregar pedidos')),
        );
      }
    }
  }

  void _onSearch(String value) {
    _currentPage = 1;
    _loadOrders();
  }

  void _setStatus(String? value) {
    setState(() {
      _statusFilter = value;
      _currentPage = 1;
    });
    _loadOrders();
  }

  Future<void> _selectDateRange() async {
    final range = await showDateRangePicker(
      context: context,
      firstDate: DateTime(2020),
      lastDate: DateTime(2030),
      initialDateRange: _fromDate != null && _toDate != null
          ? DateTimeRange(start: _fromDate!, end: _toDate!)
          : null,
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(
              primary: AppTheme.primary600,
              onPrimary: Colors.white,
              surface: Colors.white,
              onSurface: AppTheme.gray900,
            ),
          ),
          child: child!,
        );
      },
    );

    if (range != null) {
      setState(() {
        _fromDate = range.start;
        _toDate = range.end;
        _currentPage = 1;
      });
      _loadOrders();
    }
  }

  void _clearFilters() {
    setState(() {
      _searchCtrl.clear();
      _statusFilter = null;
      _fromDate = null;
      _toDate = null;
      _currentPage = 1;
    });
    _loadOrders();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.gray50,
      appBar: AppBar(
        title: const Text('Pedidos', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 24)),
        actions: [
          IconButton(onPressed: _clearFilters, icon: const Icon(Icons.filter_list_off)),
          IconButton(onPressed: _loadOrders, icon: const Icon(Icons.refresh)),
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
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _searchCtrl,
                        onSubmitted: _onSearch,
                        decoration: InputDecoration(
                          hintText: 'Buscar pedidos...',
                          prefixIcon: const Icon(Icons.search, color: AppTheme.primary600),
                          filled: true,
                          fillColor: AppTheme.gray50,
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(20),
                            borderSide: BorderSide.none,
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(width: 12),
                    GestureDetector(
                      onTap: _selectDateRange,
                      child: Container(
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: AppTheme.primary50,
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: const Icon(Icons.calendar_today, color: AppTheme.primary600, size: 20),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      _FilterChip(
                        label: 'Todos',
                        isSelected: _statusFilter == null,
                        onTap: () => _setStatus(null),
                      ),
                      const SizedBox(width: 8),
                      _FilterChip(
                        label: 'Pendentes',
                        isSelected: _statusFilter == 'Pending',
                        onTap: () => _setStatus('Pending'),
                      ),
                      const SizedBox(width: 8),
                      _FilterChip(
                        label: 'Enviados',
                        isSelected: _statusFilter == 'Shipped',
                        onTap: () => _setStatus('Shipped'),
                      ),
                      const SizedBox(width: 8),
                      _FilterChip(
                        label: 'Entregues',
                        isSelected: _statusFilter == 'Delivered',
                        onTap: () => _setStatus('Delivered'),
                      ),
                    ],
                  ),
                ),
                if (_fromDate != null && _toDate != null) ...[
                  const SizedBox(height: 12),
                  Text(
                    'Intervalo: ${_fromDate!.day}/${_fromDate!.month} até ${_toDate!.day}/${_toDate!.month}',
                    style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppTheme.primary600),
                  ),
                ],
              ],
            ),
          ),

          // ─── Orders List ────────────────────────────────────────
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : _response == null || _response!.items.isEmpty
                    ? const Center(child: Text('Nenhum pedido encontrado'))
                    : ListView.builder(
                        padding: const EdgeInsets.all(20),
                        itemCount: _response!.items.length,
                        itemBuilder: (context, index) {
                          final order = _response!.items[index];
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
                              padding: const EdgeInsets.all(20),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: [
                                      Text(
                                        '#${order.orderNumber}',
                                        style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 18, color: AppTheme.gray900),
                                      ),
                                      _StatusBadge(status: order.status),
                                    ],
                                  ),
                                  const SizedBox(height: 12),
                                  Row(
                                    children: [
                                      const Icon(Icons.person_outline, size: 14, color: AppTheme.gray500),
                                      const SizedBox(width: 8),
                                      Text(
                                        order.customerName,
                                        style: const TextStyle(color: AppTheme.gray700, fontWeight: FontWeight.bold, fontSize: 13),
                                      ),
                                    ],
                                  ),
                                  const Padding(
                                    padding: EdgeInsets.symmetric(vertical: 16),
                                    child: Divider(height: 1, color: AppTheme.gray50),
                                  ),
                                  Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: [
                                      Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          const Text('TOTAL', style: TextStyle(color: AppTheme.gray400, fontSize: 9, fontWeight: FontWeight.black, letterSpacing: 1.1)),
                                          Text(
                                            'R\$ ${order.total.toStringAsFixed(2)}',
                                            style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 20, color: AppTheme.primary600),
                                          ),
                                        ],
                                      ),
                                      Text(
                                        '${order.createdAt.day}/${order.createdAt.month}/${order.createdAt.year}',
                                        style: const TextStyle(color: AppTheme.gray400, fontSize: 12, fontWeight: FontWeight.bold),
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
  final String status;
  const _StatusBadge({required this.status});

  @override
  Widget build(BuildContext context) {
    Color color;
    Color bgColor;

    switch (status.toLowerCase()) {
      case 'delivered' || 'entregue':
        color = const Color(0xFF059669);
        bgColor = const Color(0xFFECFDF5);
        break;
      case 'pending' || 'pendente':
        color = const Color(0xFFD97706);
        bgColor = const Color(0xFFFFFBEB);
        break;
      case 'cancelled' || 'cancelado':
        color = const Color(0xFFDC2626);
        bgColor = const Color(0xFFFEF2F2);
        break;
      default:
        color = AppTheme.primary600;
        bgColor = AppTheme.primary50;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(10),
      ),
      child: Text(
        status.toUpperCase(),
        style: TextStyle(
          color: color,
          fontSize: 9,
          fontWeight: FontWeight.w900,
          letterSpacing: 1.1,
        ),
      ),
    );
  }
}
