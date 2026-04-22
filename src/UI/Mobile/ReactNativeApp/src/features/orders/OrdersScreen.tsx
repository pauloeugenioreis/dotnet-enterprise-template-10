import React from 'react';
import { View, Text, FlatList, SafeAreaView } from 'react-native';
import { colors } from '../../theme/colors';

const MOCK_ORDERS = Array.from({ length: 10 }).map((_, i) => ({
  id: String(i),
  orderNumber: `100${i + 1}`,
  customerName: `Cliente Exemplo ${i}`,
  status: i % 3 === 0 ? 'Pendente' : 'Entregue',
  total: 150 + (i * 25),
  date: '22/04/2026'
}));

export default function OrdersScreen() {
  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colors.gray[50] }}>
      <FlatList
        data={MOCK_ORDERS}
        keyExtractor={item => item.id}
        contentContainerStyle={{ padding: 20 }}
        ListHeaderComponent={<Text style={{ fontSize: 22, fontWeight: '900', color: colors.gray[900], marginBottom: 16 }}>Meus Pedidos</Text>}
        renderItem={({ item }) => (
          <View style={{ backgroundColor: 'white', borderRadius: 20, padding: 20, marginBottom: 12, elevation: 2 }}>
            <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text style={{ fontSize: 16, fontWeight: 'bold' }}>Pedido #{item.orderNumber}</Text>
              <View style={{ backgroundColor: item.status === 'Entregue' ? '#F0FDF4' : '#FFFBEB', paddingHorizontal: 10, paddingVertical: 4, borderRadius: 8 }}>
                <Text style={{ fontSize: 11, fontWeight: 'bold', color: item.status === 'Entregue' ? colors.success : '#F59E0B' }}>{item.status}</Text>
              </View>
            </View>
            <Text style={{ color: colors.gray[500], fontSize: 13, marginTop: 4 }}>{item.customerName}</Text>
            <View style={{ height: 1, backgroundColor: colors.gray[100], marginVertical: 12 }} />
            <View style={{ flexDirection: 'row', justifyContent: 'space-between' }}>
              <Text style={{ fontSize: 18, fontWeight: '900', color: colors.primary[600] }}>R$ {item.total.toFixed(2)}</Text>
              <Text style={{ fontSize: 12, color: colors.gray[400] }}>{item.date}</Text>
            </View>
          </View>
        )}
      />
    </SafeAreaView>
  );
}
