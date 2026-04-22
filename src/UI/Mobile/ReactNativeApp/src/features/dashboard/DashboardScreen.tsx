import React from 'react';
import { View, Text, ScrollView, SafeAreaView, RefreshControl } from 'react-native';
import { colors } from '../../theme/colors';

export default function DashboardScreen() {
  const [refreshing, setRefreshing] = React.useState(false);

  const onRefresh = React.useCallback(() => {
    setRefreshing(true);
    setTimeout(() => setRefreshing(false), 1000);
  }, []);

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colors.gray[50] }}>
      <ScrollView 
        contentContainerStyle={{ padding: 20 }}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[colors.primary[600]]} />}
      >
        <Text style={{ fontSize: 22, fontWeight: '900', color: colors.gray[900], marginBottom: 4 }}>
          Bem-vindo de volta! 👋
        </Text>
        <Text style={{ fontSize: 14, color: colors.gray[500], marginBottom: 24 }}>
          Resumo do seu negócio hoje
        </Text>

        {/* Stats Row */}
        <View style={{ flexDirection: 'row', justifyContent: 'space-between', marginBottom: 16 }}>
          <StatCard label="Receita" value="R$ 25.000" icon="💰" color={colors.primary[600]} bgColor={colors.primary[50]} />
          <StatCard label="Pedidos" value="120" icon="📦" color={colors.success} bgColor="#F0FDF4" />
        </View>

        {/* Highlight Card */}
        <View style={{ 
          backgroundColor: colors.primary[600], borderRadius: 24, padding: 24, marginBottom: 20,
          shadowColor: colors.primary[600], shadowOffset: { width: 0, height: 8 },
          shadowOpacity: 0.3, shadowRadius: 10, elevation: 8
        }}>
          <View style={{ flexDirection: 'row', alignItems: 'center' }}>
            <Text style={{ fontSize: 40, marginRight: 16 }}>📈</Text>
            <View>
              <Text style={{ color: colors.primary[200], fontSize: 12, fontWeight: 'bold' }}>Ticket Médio</Text>
              <Text style={{ color: 'white', fontSize: 28, fontWeight: '900' }}>R$ 208,33</Text>
            </View>
          </View>
        </View>

        {/* Content Placeholder */}
        <View style={{ backgroundColor: 'white', borderRadius: 24, padding: 20, elevation: 2 }}>
          <Text style={{ fontSize: 16, fontWeight: 'bold', marginBottom: 16 }}>Produtos em Destaque</Text>
          <View style={{ height: 200, justifyContent: 'center', alignItems: 'center' }}>
            <Text style={{ color: colors.gray[400] }}>Lista de produtos aqui...</Text>
          </View>
        </View>

      </ScrollView>
    </SafeAreaView>
  );
}

function StatCard({ label, value, icon, color, bgColor }: any) {
  return (
    <View style={{ 
      backgroundColor: 'white', borderRadius: 20, padding: 20, width: '48%',
      elevation: 2, shadowColor: '#000', shadowOpacity: 0.05, shadowRadius: 5
    }}>
      <View style={{ backgroundColor: bgColor, width: 44, height: 44, borderRadius: 12, justifyContent: 'center', alignItems: 'center', marginBottom: 12 }}>
        <Text style={{ fontSize: 20 }}>{icon}</Text>
      </View>
      <Text style={{ fontSize: 11, fontWeight: 'bold', color: colors.gray[500] }}>{label}</Text>
      <Text style={{ fontSize: 20, fontWeight: '900', color: colors.gray[900] }}>{value}</Text>
    </View>
  );
}
