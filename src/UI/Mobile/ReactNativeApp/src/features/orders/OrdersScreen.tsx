import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, FlatList, SafeAreaView, TextInput, TouchableOpacity, ActivityIndicator, RefreshControl, Alert, StyleSheet } from 'react-native';
import { colors } from '../../theme/colors';
import apiClient from '../../api/apiClient';
import OrderFormModal from './OrderFormModal';
import { formatCurrency, formatDate } from '../../utils/formatters';

export default function OrdersScreen() {
  const [orders, setOrders] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [modalVisible, setModalVisible] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<any>(null);

  const loadOrders = useCallback(async (isRefresh = false, newPage = 1) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(newPage === 1);

    try {
      const response = await apiClient.get('/api/v1/Order', {
        params: {
          searchTerm: search,
          status: status,
          page: newPage,
          pageSize: 10
        }
      });
      
      if (newPage === 1) {
        setOrders(response.data.items);
      } else {
        setOrders(prev => [...prev, ...response.data.items]);
      }
      
      setTotalPages(response.data.totalPages);
      setPage(newPage);
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

  const handleDetails = (item: any) => {
    const message = `Cliente: ${item.customerName}\nEmail: ${item.customerEmail}\nEndereço: ${item.shippingAddress}\n\nItens:\n` + 
                    item.items.map((i: any) => `- ${i.productName} (${i.quantity}x)`).join('\n');
    Alert.alert('Detalhes do Pedido', message);
  };

  const handleEdit = (item: any) => {
    setSelectedOrder(item);
    setModalVisible(true);
  };

  const handleCancel = (id: number) => {
    Alert.alert(
      'Cancelar Pedido',
      'Tem certeza que deseja cancelar este pedido?',
      [
        { text: 'Não', style: 'cancel' },
        { 
          text: 'Sim, Cancelar', 
          style: 'destructive',
          onPress: async () => {
            try {
              await apiClient.post(`/api/v1/Order/${id}/cancel`, '"Cancelado via App Mobile"');
              loadOrders(false, 1);
            } catch (err) {
              Alert.alert('Erro', 'Não foi possível cancelar o pedido');
            }
          }
        }
      ]
    );
  };

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
              onSubmitEditing={() => loadOrders(false, 1)}
            />
          </View>
        </View>

        <View style={{ flexDirection: 'row', marginTop: 16, gap: 8 }}>
          {['Todos', 'Pending', 'Shipped', 'Delivered'].map((s) => (
            <TouchableOpacity 
              key={s}
              onPress={() => setStatus(s === 'Todos' ? null : s)}
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

      <FlatList
        data={orders}
        keyExtractor={item => item.id.toString()}
        contentContainerStyle={{ padding: 20 }}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={() => loadOrders(true, 1)} />}
        onEndReached={() => {
          if (page < totalPages && !loading) {
            loadOrders(false, page + 1);
          }
        }}
        onEndReachedThreshold={0.5}
        ListFooterComponent={() => (
          loading && page > 1 ? <ActivityIndicator style={{ marginVertical: 20 }} color={colors.primary[600]} /> : null
        )}
        ListEmptyComponent={() => (
          !loading ? <Text style={{ textAlign: 'center', marginTop: 40, color: colors.gray[400] }}>Nenhum pedido encontrado</Text> : null
        )}
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
                <Text style={{ fontSize: 20, fontWeight: '900', color: colors.primary[600] }}>{formatCurrency(item.total)}</Text>
              </View>
              
              <View style={{ flexDirection: 'row', gap: 0 }}>
                <TouchableOpacity onPress={() => handleDetails(item)} style={{ padding: 8 }}>
                  <Text style={{ fontSize: 20 }}>🔍</Text>
                </TouchableOpacity>
                <TouchableOpacity onPress={() => handleEdit(item)} style={{ padding: 8 }}>
                  <Text style={{ fontSize: 20 }}>✏️</Text>
                </TouchableOpacity>
                {item.status !== 'Cancelled' && (
                  <TouchableOpacity onPress={() => handleCancel(item.id)} style={{ padding: 8 }}>
                    <Text style={{ fontSize: 20 }}>🗑️</Text>
                  </TouchableOpacity>
                )}
              </View>
            </View>
          </View>
        )}
      />
      
      {loading && page === 1 && (
        <View style={{ ...StyleSheet.absoluteFillObject, backgroundColor: 'rgba(255,255,255,0.7)', justifyContent: 'center', alignItems: 'center' }}>
          <ActivityIndicator size="large" color={colors.primary[600]} />
        </View>
      )}

      <TouchableOpacity 
        onPress={() => setModalVisible(true)}
        style={{ 
          position: 'absolute', right: 24, bottom: 24, 
          backgroundColor: colors.primary[600], paddingHorizontal: 20, paddingVertical: 16, 
          borderRadius: 30, flexDirection: 'row', alignItems: 'center', elevation: 8
        }}
      >
        <Text style={{ color: 'white', fontWeight: '900', fontSize: 16, marginRight: 8 }}>+</Text>
        <Text style={{ color: 'white', fontWeight: '900', fontSize: 12 }}>NOVO PEDIDO</Text>
      </TouchableOpacity>

      <OrderFormModal 
        visible={modalVisible} 
        onClose={(refresh) => {
          setModalVisible(false);
          if (refresh) loadOrders(false, 1);
        }} 
      />
    </SafeAreaView>
  );
}
