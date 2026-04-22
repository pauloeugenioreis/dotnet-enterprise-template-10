import React from 'react';
import { View, Text, FlatList, SafeAreaView } from 'react-native';
import { colors } from '../../theme/colors';

const MOCK_AUDIT = Array.from({ length: 15 }).map((_, i) => ({
  id: String(i),
  eventType: 'OrderCreated',
  entity: 'Order',
  user: 'admin@enterprise.com',
  date: '22/04 10:45',
  entityId: '550e8400-e29b-41d4-a716-446655440000'
}));

export default function AuditScreen() {
  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colors.gray[50] }}>
      <FlatList
        data={MOCK_AUDIT}
        keyExtractor={item => item.id}
        contentContainerStyle={{ padding: 20 }}
        ListHeaderComponent={<Text style={{ fontSize: 22, fontWeight: '900', color: colors.gray[900], marginBottom: 16 }}>Auditoria</Text>}
        renderItem={({ item }) => (
          <View style={{ backgroundColor: 'white', borderRadius: 20, padding: 16, marginBottom: 12, elevation: 2 }}>
            <View style={{ flexDirection: 'row', justifyContent: 'space-between' }}>
              <View style={{ backgroundColor: colors.primary[50], paddingHorizontal: 8, paddingVertical: 4, borderRadius: 8 }}>
                <Text style={{ color: colors.primary[600], fontSize: 11, fontWeight: 'bold' }}>{item.eventType}</Text>
              </View>
              <Text style={{ color: colors.gray[400], fontSize: 11 }}>{item.date}</Text>
            </View>
            <Text style={{ fontSize: 14, fontWeight: 'bold', marginTop: 12 }}>Entidade: {item.entity}</Text>
            <Text style={{ color: colors.gray[500], fontSize: 12, marginTop: 4 }}>Usuário: {item.user}</Text>
            <View style={{ height: 1, backgroundColor: colors.gray[100], marginVertical: 12 }} />
            <Text style={{ color: colors.gray[400], fontSize: 10, fontFamily: 'monospace' }}>ID: {item.entityId}</Text>
          </View>
        )}
      />
    </SafeAreaView>
  );
}
