import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';

class CustomerReviewsPage extends StatefulWidget {
  const CustomerReviewsPage({super.key});

  @override
  State<CustomerReviewsPage> createState() => _CustomerReviewsPageState();
}

class _CustomerReviewsPageState extends State<CustomerReviewsPage> {
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadReviews();
  }

  Future<void> _loadReviews() async {
    setState(() => _loading = true);
    await Future.delayed(const Duration(seconds: 1)); // Simulação
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Avaliações')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : ListView.builder(
              padding: const EdgeInsets.all(20),
              itemCount: 5,
              itemBuilder: (context, index) {
                return Card(
                  margin: const EdgeInsets.only(bottom: 16),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            const Text('João Silva', style: TextStyle(fontWeight: FontWeight.bold)),
                            Row(
                              children: List.generate(5, (i) => Icon(
                                Icons.star, 
                                size: 16, 
                                color: i < 4 ? Colors.amber : AppTheme.gray200,
                              )),
                            ),
                          ],
                        ),
                        const SizedBox(height: 8),
                        const Text(
                          'Excelente produto! A entrega foi super rápida e a qualidade superou as expectativas.',
                          style: TextStyle(color: AppTheme.gray700, fontSize: 13),
                        ),
                        const SizedBox(height: 12),
                        Text(
                          'Avaliado em 22/04/2026',
                          style: TextStyle(color: AppTheme.gray400, fontSize: 11),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
    );
  }
}
