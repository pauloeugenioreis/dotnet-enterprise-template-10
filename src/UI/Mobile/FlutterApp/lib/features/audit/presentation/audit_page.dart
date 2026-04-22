import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';

class AuditPage extends StatefulWidget {
  const AuditPage({super.key});

  @override
  State<AuditPage> createState() => _AuditPageState();
}

class _AuditPageState extends State<AuditPage> {
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadAudit();
  }

  Future<void> _loadAudit() async {
    setState(() => _loading = true);
    await Future.delayed(const Duration(seconds: 1)); // Simulação
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Logs de Auditoria')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : ListView.builder(
              padding: const EdgeInsets.all(20),
              itemCount: 15,
              itemBuilder: (context, index) {
                return Card(
                  margin: const EdgeInsets.only(bottom: 12),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                              decoration: BoxDecoration(
                                color: AppTheme.primary50,
                                borderRadius: BorderRadius.circular(8),
                              ),
                              child: const Text('OrderCreated', 
                                  style: TextStyle(color: AppTheme.primary600, fontSize: 11, fontWeight: FontWeight.bold)),
                            ),
                            const Text('22/04 10:45', 
                                style: TextStyle(color: AppTheme.gray400, fontSize: 11)),
                          ],
                        ),
                        const SizedBox(height: 12),
                        const Text('Entidade: Order', 
                            style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                        const SizedBox(height: 4),
                        const Text('Usuário: admin@enterprise.com', 
                            style: TextStyle(color: AppTheme.gray500, fontSize: 12)),
                        const Divider(height: 24, color: AppTheme.gray100),
                        const Text('ID: 550e8400-e29b-41d4-a716-446655440000', 
                            style: TextStyle(color: AppTheme.gray400, fontSize: 10, fontFamily: 'monospace')),
                      ],
                    ),
                  ),
                );
              },
            ),
    );
  }
}
