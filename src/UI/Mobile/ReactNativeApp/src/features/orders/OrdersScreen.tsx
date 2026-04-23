import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, FlatList, SafeAreaView, TextInput, TouchableOpacity, ActivityIndicator, RefreshControl } from 'react-native';
import { colors } from '../../theme/colors';
import apiClient from '../../api/apiClient';

export default function OrdersScreen() {
  const [orders, setOrders] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<string | null>(null);

  const loadOrders = useCallback(async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);

    try {
      const response = await apiClient.get('/api/orders', {
        params: {
          searchTerm: search,
          status: status,
          pageSize: 20
        }
      });
      setOrders(response.data.items);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [search, status]);

  useEffect(() => {
    loadOrders();
  }, [loadOrders]);

  const renderStatusBadge = (orderStatus: string) => {
    let badgeColors = { bg: colors.gray[50], text: colors.primary[600] };
    
    switch (orderStatus.toLowerCase()) {
      case 'delivered':
        badgeColors = { bg: '#ECFDF5', text: '#059669' };
        break;
      case 'pending':
        badgeColors = { bg: '#FFFBEB', text: '#D97706' };
        break;
      case 'cancelled':
        badgeColors = { bg: '#FEF2F2', text: '#DC2626' };
        break;
    }

    return (
      <View style={{ backgroundColor: badgeColors.bg, paddingHorizontal: 12, paddingVertical: 6, borderRadius: 10 }}>
        <Text style={{ fontSize: 10, fontWeight: '900', color: badgeColors.text }}>{orderStatus.toUpperCase()}</Text>
      </View>
    );
  };

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colors.gray[50] }}>
      <View style={{ padding: 20, backgroundColor: 'white', borderBottomLeftRadius: 32, borderBottomRightRadius: 32, elevation: 4 }}>
        <Text style={{ fontSize: 28, fontWeight: '900', color: colors.gray[900], marginBottom: 16 }}>Pedidos</Text>
        
        <View style={{ flexDirection: 'row', alignItems: 'center' }}>
          <View style={{ flex: 1, backgroundColor: colors.gray[50], borderRadius: 20, flexDirection: 'row', alignItems: 'center', paddingHorizontal: 16 }}>
            <Text style={{ fontSize: 16 }}>🔍</Text>
            <TextInput
              style={{ flex: 1, padding: 12, fontSize: 14, fontWeight: 'bold' }}
              placeholder="Número ou cliente..."
              value={search}
              onChangeText={setSearch}
              onSubmitEditing={() => loadOrders()}
            />
          </View>
        </View>

        <View style={{ flexDirection: 'row', marginTop: 16, gap: 8 }}>
          {['Todos', 'Pending', 'Shipped', 'Delivered'].map((s) => (
            <TouchableOpacity 
              key={s}
              onTap={() => setStatus(s === 'Todos' ? null : s)}
              style={{ 
                paddingHorizontal: 16, paddingVertical: 8, borderRadius: 12,
                backgroundColor: (status === s || (s === 'Todos' && status === null)) ? colors.primary[600] : colors.gray[50]
              }}
            >
              <Text style={{ 
                fontSize: 11, fontWeight: 'bold', 
                color: (status === s || (s === 'Todos' && status === null)) ? 'white' : colors.gray[500] 
              }}>{s.toUpperCase()}</Text>
            </TouchableOpacity>
          ))}
        </View>
      </View>

      {loading && !refreshing ? (
        <ActivityIndicator style={{ marginTop: 40 }} color={colors.primary[600]} />
      ) : (
        <FlatList
          data={orders}
          keyExtractor={item => item.id}
          contentContainerStyle={{ padding: 20 }}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={() => loadOrders(true)} />}
          renderItem={({ item }) => (
            <View style={{ backgroundColor: 'white', borderRadius: 24, padding: 20, marginBottom: 16, elevation: 2 }}>
              <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text style={{ fontSize: 18, fontWeight: '900', color: colors.gray[900] }}>#{item.orderNumber}</Text>
                {renderStatusBadge(item.status)}
              </View>
              
              <View style={{ flexDirection: 'row', alignItems: 'center', marginTop: 12 }}>
                <Text style={{ fontSize: 14, marginRight: 8 }}>👤</Text>
                <Text style={{ fontSize: 13, fontWeight: 'bold', color: colors.gray[700] }}>{item.customerName}</Text>
              </View>

              <View style={{ height: 1, backgroundColor: colors.gray[50], marginVertical: 16 }} />

              <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-end' }}>
                <View>
                  <Text style={{ fontSize: 9, fontWeight: '900', color: colors.gray[400], letterSpacing: 1.1 }}>TOTAL</Text>
                  <Text style={{ fontSize: 20, fontWeight: '900', color: colors.primary[600] }}>R$ {item.total.toFixed(2)}</Text>
                </View>
                <Text style={{ fontSize: 12, fontWeight: 'bold', color: colors.gray[400] }}>
                  {new Date(item.createdAt).toLocaleDateString('pt-BR')}
                </Text>
              </View>
            </View>
          )}
        />
      )}
    </SafeAreaView>
  );
}
