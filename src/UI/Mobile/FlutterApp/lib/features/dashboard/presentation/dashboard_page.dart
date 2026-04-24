import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';
import '../../../shared/models/api_models.dart';
import '../../../shared/dependency_provider.dart';
import '../../auth/presentation/login_page.dart';
import '../../orders/presentation/orders_page.dart';
import '../../products/presentation/products_page.dart';
import '../../../core/utils/currency_formatter.dart';

class DashboardPage extends StatefulWidget {
  const DashboardPage({super.key});

  @override
  State<DashboardPage> createState() => _DashboardPageState();
}

class _DashboardPageState extends State<DashboardPage> {
  OrderStatistics? _stats;
  bool _loading = true;
  String? _error;
  int _selectedTab = 0;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadStats());
  }

  Future<void> _loadStats() async {
    setState(() { _loading = true; _error = null; });
    try {
      final service = DependencyProvider.of(context).orderService;
      final stats = await service.getStatistics();
      setState(() {
        _stats = stats;
      });
    } catch (e) {
      setState(() => _error = 'Erro ao conectar com o servidor');
    } finally {
      setState(() => _loading = false);
    }
  }

  Future<void> _handleLogout() async {
    try {
      final client = DependencyProvider.of(context).apiClient;
      await client.auth.clearToken();
    } catch (e) {
      // Mesmo se falhar a limpeza, queremos sair
      debugPrint('Erro ao limpar token: $e');
    } finally {
      if (mounted) {
        Navigator.of(context).pushAndRemoveUntil(
          MaterialPageRoute(builder: (_) => const LoginPage()),
          (route) => false,
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.gray50,
      body: NestedScrollView(
        headerSliverBuilder: (context, _) => [
          SliverAppBar(
            expandedHeight: 120,
            floating: true,
            pinned: true,
            backgroundColor: AppTheme.primary600,
            actions: [
              IconButton(
                onPressed: _handleLogout,
                icon: const Icon(Icons.logout_rounded, color: Colors.white),
                tooltip: 'Sair',
              ),
              const SizedBox(width: 8),
            ],
            flexibleSpace: FlexibleSpaceBar(
              titlePadding: const EdgeInsets.only(left: 20, bottom: 16),
              title: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: const [
                  Text('Bem-vindo de volta! 👋',
                      style: TextStyle(fontSize: 16, fontWeight: FontWeight.w900, color: Colors.white)),
                  Text('Resumo do seu negócio',
                      style: TextStyle(fontSize: 11, color: Color(0xFFBAE6FD))),
                ],
              ),
            ),
          ),
        ],
        body: RefreshIndicator(
          color: AppTheme.primary600,
          onRefresh: _loadStats,
          child: _loading
              ? _buildSkeleton()
              : _error != null
                  ? _buildError()
                  : _buildContent(),
        ),
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _selectedTab,
        onDestinationSelected: (i) {
          if (i == _selectedTab) return;
          
          if (i == 1) {
            Navigator.push(context, MaterialPageRoute(builder: (_) => const OrdersPage()));
          } else if (i == 2) {
            Navigator.push(context, MaterialPageRoute(builder: (_) => const ProductsPage()));
          } else {
            setState(() => _selectedTab = i);
          }
        },
        indicatorColor: AppTheme.primary100,
        destinations: const [
          NavigationDestination(icon: Icon(Icons.dashboard_outlined), selectedIcon: Icon(Icons.dashboard), label: 'Dashboard'),
          NavigationDestination(icon: Icon(Icons.receipt_long_outlined), selectedIcon: Icon(Icons.receipt_long), label: 'Pedidos'),
          NavigationDestination(icon: Icon(Icons.inventory_2_outlined), selectedIcon: Icon(Icons.inventory_2), label: 'Produtos'),
        ],
      ),
    );
  }

  Widget _buildSkeleton() {
    return ListView(padding: const EdgeInsets.all(20), children: [
      for (int i = 0; i < 4; i++)
        Container(
          margin: const EdgeInsets.only(bottom: 16),
          height: i < 2 ? 110 : 180,
          decoration: BoxDecoration(
            color: AppTheme.gray200,
            borderRadius: BorderRadius.circular(20),
          ),
        ),
    ]);
  }

  Widget _buildError() {
    return Center(
      child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
        const Icon(Icons.error_outline, size: 64, color: AppTheme.error),
        const SizedBox(height: 16),
        Text(_error!, textAlign: TextAlign.center, style: const TextStyle(color: AppTheme.gray500)),
        const SizedBox(height: 24),
        ElevatedButton(onPressed: _loadStats, child: const Text('Tentar novamente')),
      ]),
    );
  }

  Widget _buildContent() {
    final stats = _stats!;
    return ListView(
      padding: const EdgeInsets.all(20),
      children: [
        // ─── Stats Row ──────────────────────────────────────────
        Row(children: [
          Expanded(child: _StatCard(
            label: 'Receita Total',
            value: CurrencyFormatter.format(stats.totalRevenue),
            icon: Icons.attach_money_rounded,
            color: AppTheme.primary600,
            bgColor: AppTheme.primary50,
          )),
          const SizedBox(width: 12),
          Expanded(child: _StatCard(
            label: 'Pedidos',
            value: stats.totalOrders.toString(),
            icon: Icons.shopping_bag_outlined,
            color: const Color(0xFF10B981),
            bgColor: const Color(0xFFF0FDF4),
          )),
        ]),

        const SizedBox(height: 16),

        // ─── Ticket Médio (destaque) ─────────────────────────────
        Container(
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            gradient: const LinearGradient(
              colors: [AppTheme.primary600, AppTheme.primary700],
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
            ),
            borderRadius: BorderRadius.circular(24),
            boxShadow: [
              BoxShadow(
                color: AppTheme.primary600.withValues(alpha: 0.35),
                blurRadius: 20,
                offset: const Offset(0, 8),
              ),
            ],
          ),
          child: Row(children: [
            const Text('📈', style: TextStyle(fontSize: 40)),
            const SizedBox(width: 16),
            Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              const Text('Ticket Médio',
                  style: TextStyle(color: Color(0xFFBAE6FD), fontSize: 12, fontWeight: FontWeight.bold)),
              Text(
                CurrencyFormatter.format(stats.averageOrderValue),
                style: const TextStyle(color: Colors.white, fontSize: 28, fontWeight: FontWeight.w900),
              ),
              const Text('por pedido', style: TextStyle(color: Color(0xFF7DD3FC), fontSize: 11)),
            ]),
          ]),
        ),

        const SizedBox(height: 20),

        // ─── Top Products ─────────────────────────────────────────
        Card(
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('Produtos em Destaque',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppTheme.gray900)),
                const SizedBox(height: 16),
                ...stats.topProducts.map((p) => _ProductRow(product: p)),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

class _StatCard extends StatelessWidget {
  final String label;
  final String value;
  final IconData icon;
  final Color color;
  final Color bgColor;

  const _StatCard({
    required this.label,
    required this.value,
    required this.icon,
    required this.color,
    required this.bgColor,
  });

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(20),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Container(
          padding: const EdgeInsets.all(10),
          decoration: BoxDecoration(color: bgColor, borderRadius: BorderRadius.circular(12)),
          child: Icon(icon, color: color, size: 22),
        ),
        const SizedBox(height: 12),
        Text(label, style: const TextStyle(fontSize: 11, color: AppTheme.gray500, fontWeight: FontWeight.bold)),
        const SizedBox(height: 4),
        Text(value, style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w900, color: AppTheme.gray900)),
      ]),
    ),
  );
}

class _ProductRow extends StatelessWidget {
  final TopProduct product;
  const _ProductRow({required this.product});

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 8),
    child: Row(children: [
      Container(
        width: 44, height: 44,
        decoration: BoxDecoration(color: AppTheme.primary50, borderRadius: BorderRadius.circular(12)),
        child: Center(
          child: Text(
            product.productName[0],
            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: AppTheme.primary600),
          ),
        ),
      ),
      const SizedBox(width: 12),
      Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text(product.productName, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.bold)),
        Text('${product.quantitySold} vendidos', style: const TextStyle(fontSize: 12, color: AppTheme.gray500)),
      ])),
      Text(
        CurrencyFormatter.format(product.revenue),
        style: const TextStyle(fontSize: 14, fontWeight: FontWeight.bold, color: AppTheme.primary600),
      ),
    ]),
  );
}
