import React, { useEffect, useState, useCallback } from 'react';
import { View, Text, ScrollView, SafeAreaView, RefreshControl, ActivityIndicator } from 'react-native';
import { colors } from '../../theme/colors';
import apiClient from '../../api/apiClient';
import { formatCurrency } from '../../utils/formatters';

export default function DashboardScreen() {
  const [stats, setStats] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadStats = useCallback(async () => {
    try {
      const response = await apiClient.get('/api/v1/Order/statistics');
      setStats(response.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    loadStats();
  }, [loadStats]);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadStats();
  }, [loadStats]);

  if (loading && !refreshing) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.gray[50] }}>
        <ActivityIndicator size="large" color={colors.primary[600]} />
      </View>
    );
  }

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
          <StatCard label="Receita" value={formatCurrency(stats?.totalRevenue || 0)} icon="💰" color={colors.primary[600]} bgColor={colors.primary[50]} />
          <StatCard label="Pedidos" value={stats?.totalOrders?.toString() || '0'} icon="📦" color={colors.success} bgColor="#F0FDF4" />
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
              <Text style={{ color: 'white', fontSize: 28, fontWeight: '900' }}>{formatCurrency(stats?.averageOrderValue || 0)}</Text>
            </View>
          </View>
        </View>

        {/* Top Products */}
        <View style={{ backgroundColor: 'white', borderRadius: 24, padding: 20, elevation: 2 }}>
          <Text style={{ fontSize: 16, fontWeight: 'bold', marginBottom: 16 }}>Produtos em Destaque</Text>
          {stats?.topProducts?.map((item: any) => (
            <View key={item.productId} style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
              <View style={{ flexDirection: 'row', alignItems: 'center' }}>
                <View style={{ width: 40, height: 40, backgroundColor: colors.primary[50], borderRadius: 10, justifyContent: 'center', alignItems: 'center' }}>
                  <Text style={{ color: colors.primary[600], fontWeight: 'bold' }}>{item.productName[0]}</Text>
                </View>
                <View style={{ marginLeft: 12 }}>
                  <Text style={{ fontWeight: 'bold', color: colors.gray[900] }}>{item.productName}</Text>
                  <Text style={{ fontSize: 11, color: colors.gray[500] }}>{item.quantitySold} vendidos</Text>
                </View>
              </View>
              <Text style={{ fontWeight: 'bold', color: colors.primary[600] }}>{formatCurrency(item.revenue)}</Text>
            </View>
          ))}
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
