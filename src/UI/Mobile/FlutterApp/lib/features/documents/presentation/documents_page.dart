import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';

class DocumentsPage extends StatefulWidget {
  const DocumentsPage({super.key});

  @override
  State<DocumentsPage> createState() => _DocumentsPageState();
}

class _DocumentsPageState extends State<DocumentsPage> {
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadDocuments();
  }

  Future<void> _loadDocuments() async {
    setState(() => _loading = true);
    await Future.delayed(const Duration(seconds: 1)); // Simulação
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Documentos')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : ListView.builder(
              padding: const EdgeInsets.all(20),
              itemCount: 8,
              itemBuilder: (context, index) {
                return Card(
                  margin: const EdgeInsets.only(bottom: 12),
                  child: ListTile(
                    leading: const Icon(Icons.picture_as_pdf, color: AppTheme.error, size: 32),
                    title: Text('Relatório_Mensal_0${index + 1}.pdf', 
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                    subtitle: const Text('PDF | 2.4 MB | 22/04/2026'),
                    trailing: IconButton(
                      icon: const Icon(Icons.download_for_offline, color: AppTheme.primary600),
                      onPressed: () {},
                    ),
                  ),
                );
              },
            ),
    );
  }
}
