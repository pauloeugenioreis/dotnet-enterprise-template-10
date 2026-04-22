export function useDocuments() {
  const docs = [
    { name: 'Contrato_Enterprise_2026.pdf', size: '2.4 MB', type: 'PDF' },
    { name: 'Relatorio_Q1_Final.pdf', size: '1.8 MB', type: 'PDF' },
    { name: 'Manual_Operacional.docx', size: '4.2 MB', type: 'DOC' },
    { name: 'Politica_Privacidade.pdf', size: '0.9 MB', type: 'PDF' },
  ];

  return {
    docs
  };
}
