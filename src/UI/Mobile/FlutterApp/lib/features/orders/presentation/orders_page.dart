import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';
import '../../../shared/models/api_models.dart';

class OrdersPage extends StatefulWidget {
  const OrdersPage({super.key});

  @override
  State<OrdersPage> createState() => _OrdersPageState();
}

class _OrdersPageState extends State<OrdersPage> {
  bool _loading = true;
  List<OrderResponse> _orders = [];
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadOrders();
  }

  Future<void> _loadOrders() async {
    setState(() { _loading = true; _error = null; });
    try {
      // Simulando chamada de API para o template
      await Future.delayed(const Duration(seconds: 1));
      setState(() {
        _orders = List.generate(10, (i) => OrderResponse(
          id: 'order_$i',
          orderNumber: '100${i+1}',
          customerName: 'Cliente Exemplo $i',
          customerEmail: 'cliente$i@email.com',
          status: i % 3 == 0 ? 'Pendente' : (i % 3 == 1 ? 'Processando' : 'Entregue'),
          total: 150.0 + (i * 25),
          subtotal: 140.0 + (i * 25),
          shippingCost: 10.0,
          tax: 0.0,
          shippingAddress: 'Rua das Empresas, $i',
          createdAt: DateTime.now().subtract(Duration(days: i)),
          items: [],
        ));
      });
    } catch (e) {
      setState(() => _error = 'Falha ao carregar pedidos.');
    } finally {
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Gestão de Pedidos'),
      ),
      body: RefreshIndicator(
        onRefresh: _loadOrders,
        color: AppTheme.primary600,
        child: _loading 
          ? const Center(child: CircularProgressIndicator())
          : _error != null
            ? _buildError()
            : _orders.isEmpty
              ? _buildEmpty()
              : _buildList(),
      ),
    );
  }

  Widget _buildList() {
    return ListView.builder(
      padding: const EdgeInsets.all(20),
      itemCount: _orders.length,
      itemBuilder: (context, index) {
        final order = _orders[index];
        return Card(
          margin: const EdgeInsets.only(bottom: 12),
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text('Pedido #${order.orderNumber}',
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                    _StatusBadge(status: order.status),
                  ],
                ),
                const SizedBox(height: 8),
                Row(
                  children: [
                    const Icon(Icons.person_outline, size: 14, color: AppTheme.gray500),
                    const SizedBox(width: 4),
                    Text(order.customerName, style: const TextStyle(color: AppTheme.gray500, fontSize: 13)),
                  ],
                ),
                const Divider(height: 24, color: AppTheme.gray100),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text('R\$ ${order.total.toStringAsFixed(2)}',
                        style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 18, color: AppTheme.primary600)),
                    Text('${order.createdAt.day}/${order.createdAt.month}/${order.createdAt.year}',
                        style: const TextStyle(color: AppTheme.gray400, fontSize: 12)),
                  ],
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildEmpty() {
    return const Center(child: Text('Nenhum pedido encontrado.'));
  }

  Widget _buildError() {
    return Center(child: Text(_error!));
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
      case 'entregue':
        color = const Color(0xFF10B981);
        bgColor = const Color(0xFFF0FDF4);
        break;
      case 'pendente':
        color = const Color(0xFFF59E0B);
        bgColor = const Color(0xFFFFFBEB);
        break;
      default:
        color = AppTheme.primary600;
        bgColor = AppTheme.primary50;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(color: bgColor, borderRadius: BorderRadius.circular(8)),
      child: Text(status, style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.bold)),
    );
  }
}
