import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';
import '../../../shared/dependency_provider.dart';
import '../../../shared/models/api_models.dart';

class OrderFormDialog extends StatefulWidget {
  final OrderResponse? order;
  const OrderFormDialog({super.key, this.order});

  @override
  State<OrderFormDialog> createState() => _OrderFormDialogState();
}

class _OrderFormDialogState extends State<OrderFormDialog> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _customerNameCtrl;
  late final TextEditingController _customerEmailCtrl;
  late final TextEditingController _addressCtrl;
  
  List<OrderItemRequest> _items = [];
  List<ProductResponse> _availableProducts = [];
  bool _loadingProducts = true;
  bool _submitting = false;

  @override
  void initState() {
    super.initState();
    _customerNameCtrl = TextEditingController(text: widget.order?.customerName);
    _customerEmailCtrl = TextEditingController(text: widget.order?.customerEmail);
    _addressCtrl = TextEditingController(text: widget.order?.shippingAddress);
    
    if (widget.order != null) {
      _items = widget.order!.items.map((i) => OrderItemRequest(
        productId: i.productId,
        quantity: i.quantity,
      )).toList();
    }

    WidgetsBinding.instance.addPostFrameCallback((_) => _loadProducts());
  }

  Future<void> _loadProducts() async {
    try {
      final service = DependencyProvider.of(context).productService;
      final res = await service.getProducts(pageSize: 500);
      debugPrint('DEBUG: Loaded ${res.items.length} products');
      if (res.items.isNotEmpty) {
        debugPrint('DEBUG: First product ID: ${res.items.first.id}');
      }
      if (widget.order != null) {
        debugPrint('DEBUG: Order has ${widget.order!.items.length} items');
        for (var item in widget.order!.items) {
          debugPrint('DEBUG: Item ProductID: ${item.productId}');
        }
      }
      setState(() {
        _availableProducts = res.items;
        _loadingProducts = false;
      });
    } catch (e) {
      debugPrint('DEBUG: Error loading products: $e');
      setState(() => _loadingProducts = false);
    }
  }

  void _addItem() {
    setState(() {
      _items.add(OrderItemRequest(productId: 0, quantity: 1));
    });
  }

  void _removeItem(int index) {
    setState(() {
      _items.removeAt(index);
    });
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_items.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Adicione pelo menos um item')),
      );
      return;
    }

    setState(() => _submitting = true);
    try {
      final service = DependencyProvider.of(context).orderService;
      final data = {
        'customerName': _customerNameCtrl.text,
        'customerEmail': _customerEmailCtrl.text,
        'shippingAddress': _addressCtrl.text,
        'status': widget.order?.status ?? 'Pending',
        'items': _items.map((i) => {
          'productId': i.productId,
          'quantity': i.quantity,
        }).toList(),
      };

      if (widget.order != null) {
        await service.updateOrder(widget.order!.id, data);
      } else {
        await service.createOrder(data);
      }
      
      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      setState(() => _submitting = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Erro ao ${widget.order != null ? 'atualizar' : 'criar'} pedido')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(28)),
      insetPadding: const EdgeInsets.all(20),
      child: Container(
        width: double.infinity,
        padding: const EdgeInsets.all(24),
        child: Form(
          key: _formKey,
          child: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(widget.order != null ? 'Editar Pedido' : 'Novo Pedido', 
                      style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 24, color: AppTheme.gray900)),
                    IconButton(
                      onPressed: () => Navigator.pop(context), 
                      icon: Container(
                        padding: const EdgeInsets.all(4),
                        decoration: BoxDecoration(color: AppTheme.gray100, shape: BoxShape.circle),
                        child: const Icon(Icons.close, size: 20, color: AppTheme.gray500),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 32),
                
                // ─── Seção: Cliente ─────────────────────────────────────
                Row(
                  children: [
                    const Icon(Icons.person_outline, size: 18, color: AppTheme.primary600),
                    const SizedBox(width: 8),
                    const Text('DADOS DO CLIENTE', 
                      style: TextStyle(fontWeight: FontWeight.w900, fontSize: 11, letterSpacing: 1.2, color: AppTheme.primary600)),
                  ],
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _customerNameCtrl,
                  decoration: AppTheme.inputDecoration('Nome do Cliente'),
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                  validator: (v) => v!.isEmpty ? 'Obrigatório' : null,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: _customerEmailCtrl,
                  decoration: AppTheme.inputDecoration('E-mail do Cliente'),
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                  keyboardType: TextInputType.emailAddress,
                  validator: (v) => v!.isEmpty ? 'Obrigatório' : null,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: _addressCtrl,
                  decoration: AppTheme.inputDecoration('Endereço de Entrega'),
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                  maxLines: 2,
                  validator: (v) => v!.isEmpty ? 'Obrigatório' : null,
                ),
                
                const SizedBox(height: 40),

                // ─── Seção: Itens ───────────────────────────────────────
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.shopping_cart_outlined, size: 18, color: AppTheme.primary600),
                        const SizedBox(width: 8),
                        const Text('ITENS DO PEDIDO', 
                          style: TextStyle(fontWeight: FontWeight.w900, fontSize: 11, letterSpacing: 1.2, color: AppTheme.primary600)),
                      ],
                    ),
                    TextButton.icon(
                      onPressed: _addItem, 
                      icon: const Icon(Icons.add_circle_outline, size: 18),
                      label: const Text('Adicionar', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12)),
                      style: TextButton.styleFrom(foregroundColor: AppTheme.primary600),
                    ),
                  ],
                ),
                const Divider(height: 1),
                const SizedBox(height: 16),
                
                if (_loadingProducts)
                  const Center(
                    child: Padding(
                      padding: EdgeInsets.symmetric(vertical: 40),
                      child: CircularProgressIndicator(),
                    ),
                  )
                else if (_items.isEmpty)
                  const Padding(
                    padding: EdgeInsets.symmetric(vertical: 20),
                    child: Center(child: Text('Nenhum item adicionado', style: TextStyle(color: AppTheme.gray400))),
                  )
                else
                  ..._items.asMap().entries.map((entry) {
                    int idx = entry.key;
                    OrderItemRequest item = entry.value;
                    return Padding(
                      padding: const EdgeInsets.only(bottom: 12),
                      child: Row(
                        children: [
                          Expanded(
                            flex: 3,
                            child: DropdownButtonFormField<int>(
                                value: item.productId == 0 ? null : item.productId,
                                decoration: AppTheme.inputDecoration('Produto'),
                                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: AppTheme.gray900),
                                items: _availableProducts.map((p) => DropdownMenuItem(
                                  value: p.id,
                                  child: Text(p.name, style: const TextStyle(fontSize: 13, overflow: TextOverflow.ellipsis)),
                                )).toList(),
                                onChanged: (val) => setState(() => item.productId = val!),
                              ),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            flex: 1,
                            child: TextFormField(
                               initialValue: item.quantity.toString(),
                               decoration: AppTheme.inputDecoration('Qtd'),
                               style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                               textAlign: TextAlign.center,
                               keyboardType: TextInputType.number,
                               onChanged: (val) => item.quantity = int.tryParse(val) ?? 1,
                             ),
                          ),
                          const SizedBox(width: 4),
                          IconButton(
                            onPressed: () => _removeItem(idx), 
                            icon: const Icon(Icons.delete_sweep_outlined, color: AppTheme.error),
                            tooltip: 'Remover',
                          ),
                        ],
                      ),
                    );
                  }),
                
                const SizedBox(height: 40),
                SizedBox(
                  width: double.infinity,
                  height: 64,
                  child: ElevatedButton(
                    onPressed: _submitting ? null : _submit,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppTheme.primary600,
                      foregroundColor: Colors.white,
                      elevation: 8,
                      shadowColor: AppTheme.primary600.withValues(alpha: 0.3),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
                    ),
                    child: _submitting 
                      ? const CircularProgressIndicator(color: Colors.white)
                      : Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Icon(Icons.check_circle_outline, size: 20),
                            const SizedBox(width: 12),
                            Text(widget.order != null ? 'SALVAR ALTERAÇÕES' : 'FINALIZAR PEDIDO', 
                              style: const TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1.2, fontSize: 16)),
                          ],
                        ),
                  ),
                ),
                const SizedBox(height: 12),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class OrderItemRequest {
  int productId;
  int quantity;
  OrderItemRequest({required this.productId, required this.quantity});
}
