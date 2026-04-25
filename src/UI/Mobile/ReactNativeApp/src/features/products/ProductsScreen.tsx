import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, FlatList, SafeAreaView, TextInput, TouchableOpacity, ActivityIndicator, RefreshControl } from 'react-native';
import { colors } from '../../theme/colors';
import apiClient from '../../api/apiClient';
import { formatCurrency } from '../../utils/formatters';

export default function ProductsScreen() {
  const [products, setProducts] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [search, setSearch] = useState('');
  const [isActive, setIsActive] = useState<boolean | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const loadProducts = useCallback(async (isRefresh = false, newPage = 1) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(newPage === 1);

    try {
      const response = await apiClient.get('/api/v1/Product', {
        params: {
          searchTerm: search,
          isActive: isActive,
          page: newPage,
          pageSize: 10
        }
      });
      
      if (newPage === 1) {
        setProducts(response.data.items);
      } else {
        setProducts(prev => [...prev, ...response.data.items]);
      }
      setTotalPages(response.data.totalPages);
      setPage(newPage);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [search, isActive]);

  useEffect(() => {
    loadProducts();
  }, [loadProducts]);

  const renderStatusBadge = (active: boolean) => {
    return (
      <View style={{ 
        backgroundColor: active ? '#ECFDF5' : '#FEF2F2', 
        paddingHorizontal: 10, paddingVertical: 4, borderRadius: 8 
      }}>
        <Text style={{ 
          fontSize: 9, fontWeight: '900', 
          color: active ? '#059669' : '#DC2626',
          letterSpacing: 1.1
        }}>{active ? 'ATIVO' : 'INATIVO'}</Text>
      </View>
    );
  };

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colors.gray[50] }}>
      <View style={{ padding: 20, backgroundColor: 'white', borderBottomLeftRadius: 32, borderBottomRightRadius: 32, elevation: 4 }}>
        <Text style={{ fontSize: 28, fontWeight: '900', color: colors.gray[900], marginBottom: 16 }}>Produtos</Text>
        
        <View style={{ backgroundColor: colors.gray[50], borderRadius: 20, flexDirection: 'row', alignItems: 'center', paddingHorizontal: 16 }}>
          <Text style={{ fontSize: 16 }}>🔍</Text>
          <TextInput
            style={{ flex: 1, padding: 12, fontSize: 14, fontWeight: 'bold' }}
            placeholder="Buscar produtos..."
            value={search}
            onChangeText={setSearch}
            onSubmitEditing={() => loadProducts()}
          />
        </View>

        <View style={{ flexDirection: 'row', marginTop: 16, gap: 8 }}>
          {['Todos', 'Ativos', 'Inativos'].map((label, idx) => {
            const val = idx === 0 ? null : (idx === 1);
            return (
              <TouchableOpacity 
                key={label}
                onPress={() => setIsActive(val)}
                style={{ 
                  paddingHorizontal: 20, paddingVertical: 10, borderRadius: 12,
                  backgroundColor: isActive === val ? colors.primary[600] : colors.gray[50]
                }}
              >
                <Text style={{ 
                  fontSize: 11, fontWeight: 'bold', 
                  color: isActive === val ? 'white' : colors.gray[500] 
                }}>{label.toUpperCase()}</Text>
              </TouchableOpacity>
            );
          })}
        </View>
      </View>

      {loading && !refreshing ? (
        <ActivityIndicator style={{ marginTop: 40 }} color={colors.primary[600]} />
      ) : (
        <FlatList
          data={products}
          keyExtractor={item => String(item.id)}
          contentContainerStyle={{ padding: 20 }}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={() => loadProducts(true, 1)} />}
          onEndReached={() => {
            if (page < totalPages && !loading) {
              loadProducts(false, page + 1);
            }
          }}
          onEndReachedThreshold={0.5}
          ListFooterComponent={() => (
            loading && page > 1 ? <ActivityIndicator style={{ marginVertical: 20 }} color={colors.primary[600]} /> : null
          )}
          renderItem={({ item }) => (
            <View style={{ backgroundColor: 'white', borderRadius: 24, padding: 16, marginBottom: 16, elevation: 2 }}>
              <View style={{ flexDirection: 'row', alignItems: 'center' }}>
                <View style={{ 
                  width: 64, height: 64, backgroundColor: colors.primary[50], 
                  borderRadius: 18, justifyContent: 'center', alignItems: 'center' 
                }}>
                  <Text style={{ fontSize: 24 }}>📦</Text>
                </View>
                <View style={{ flex: 1, marginLeft: 16 }}>
                  <Text style={{ fontSize: 16, fontWeight: '900', color: colors.gray[900] }}>{item.name}</Text>
                  <Text style={{ fontSize: 12, fontWeight: 'bold', color: colors.gray[500] }}>{item.category}</Text>
                  <View style={{ marginTop: 8, alignSelf: 'flex-start' }}>
                    {renderStatusBadge(item.isActive)}
                  </View>
                </View>
                <View style={{ alignItems: 'flex-end' }}>
                  <Text style={{ fontSize: 18, fontWeight: '900', color: colors.primary[600] }}>{formatCurrency(item.price)}</Text>
                  <Text style={{ fontSize: 11, fontWeight: 'bold', color: colors.gray[400] }}>Stock: {item.stock}</Text>
                </View>
              </View>
            </View>
          )}
        />
      )}
    </SafeAreaView>
  );
}
