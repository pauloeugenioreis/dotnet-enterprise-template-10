import React, { useState, useEffect } from 'react';
import { View, Text, Modal, TextInput, TouchableOpacity, ScrollView, SafeAreaView, ActivityIndicator, Alert, KeyboardAvoidingView, Platform } from 'react-native';
import { colors } from '../../theme/colors';
import apiClient from '../../api/apiClient';

interface Props {
  visible: boolean;
  onClose: (refresh?: boolean) => void;
}

export default function OrderFormModal({ visible, onClose }: Props) {
  const [loading, setLoading] = useState(false);
  const [products, setProducts] = useState<any[]>([]);
  const [customerName, setCustomerName] = useState('');
  const [customerEmail, setCustomerEmail] = useState('');
  const [address, setAddress] = useState('');
  const [items, setItems] = useState<{ productId: number, quantity: number }[]>([]);

  useEffect(() => {
    if (visible) {
      loadProducts();
    }
  }, [visible]);

  const loadProducts = async () => {
    try {
      const res = await apiClient.get('/api/v1/Product', { params: { pageSize: 100, isActive: true } });
      setProducts(res.data.items);
    } catch (err) {
      console.error('Erro ao carregar produtos', err);
    }
  };

  const addItem = () => {
    setItems([...items, { productId: 0, quantity: 1 }]);
  };

  const removeItem = (index: number) => {
    setItems(items.filter((_, i) => i !== index));
  };

  const handleSubmit = async () => {
    if (!customerName || !customerEmail || !address || items.length === 0) {
      Alert.alert('Erro', 'Preencha todos os campos e adicione pelo menos um item.');
      return;
    }

    setLoading(true);
    try {
      await apiClient.post('/api/v1/Order', {
        customerName,
        customerEmail,
        shippingAddress: address,
        items: items.filter(i => i.productId !== 0)
      });
      Alert.alert('Sucesso', 'Pedido criado com sucesso!');
      onClose(true);
      // Reset form
      setCustomerName('');
      setCustomerEmail('');
      setAddress('');
      setItems([]);
    } catch (err) {
      Alert.alert('Erro', 'Não foi possível criar o pedido.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal visible={visible} animationType="slide" transparent={true}>
      <View style={{ flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' }}>
        <KeyboardAvoidingView 
          behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
          style={{ backgroundColor: 'white', borderTopLeftRadius: 32, borderTopRightRadius: 32, height: '90%', padding: 24 }}
        >
          <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
            <Text style={{ fontSize: 22, fontWeight: '900', color: colors.gray[900] }}>Novo Pedido</Text>
            <TouchableOpacity onPress={() => onClose()}>
              <Text style={{ fontSize: 24 }}>✕</Text>
            </TouchableOpacity>
          </View>

          <ScrollView showsVerticalScrollIndicator={false}>
            <Text style={{ fontSize: 12, fontWeight: '900', color: colors.gray[400], marginBottom: 8, letterSpacing: 1 }}>CLIENTE</Text>
            <TextInput 
              placeholder="Nome completo" 
              style={styles.input} 
              value={customerName}
              onChangeText={setCustomerName}
            />
            <TextInput 
              placeholder="E-mail" 
              style={styles.input} 
              keyboardType="email-address" 
              value={customerEmail}
              onChangeText={setCustomerEmail}
            />
            <TextInput 
              placeholder="Endereço de entrega" 
              style={styles.input} 
              value={address}
              onChangeText={setAddress}
            />

            <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: 24, marginBottom: 8 }}>
              <Text style={{ fontSize: 12, fontWeight: '900', color: colors.gray[400], letterSpacing: 1 }}>ITENS</Text>
              <TouchableOpacity onPress={addItem}>
                <Text style={{ color: colors.primary[600], fontWeight: 'bold' }}>+ Adicionar</Text>
              </TouchableOpacity>
            </View>

            {items.map((item, index) => (
              <View key={index} style={{ flexDirection: 'row', gap: 8, marginBottom: 12, alignItems: 'center' }}>
                <View style={{ flex: 3, backgroundColor: colors.gray[50], borderRadius: 12, paddingHorizontal: 12 }}>
                  <ScrollView horizontal={true} showsHorizontalScrollIndicator={false}>
                    {products.length === 0 ? (
                      <ActivityIndicator size="small" />
                    ) : (
                      <View style={{ flexDirection: 'row', paddingVertical: 12 }}>
                        {products.map(p => (
                          <TouchableOpacity 
                            key={p.id} 
                            onPress={() => {
                              const newItems = [...items];
                              newItems[index].productId = p.id;
                              setItems(newItems);
                            }}
                            style={{ 
                              paddingHorizontal: 12, paddingVertical: 4, borderRadius: 8, marginRight: 8,
                              backgroundColor: item.productId === p.id ? colors.primary[600] : 'white',
                              borderWidth: 1, borderColor: colors.gray[200]
                            }}
                          >
                            <Text style={{ fontSize: 10, color: item.productId === p.id ? 'white' : colors.gray[500], fontWeight: 'bold' }}>{p.name}</Text>
                          </TouchableOpacity>
                        ))}
                      </View>
                    )}
                  </ScrollView>
                </View>
                <TextInput 
                  placeholder="Qtd" 
                  keyboardType="number-pad" 
                  style={{ ...styles.input, flex: 1, marginBottom: 0 }} 
                  value={item.quantity.toString()}
                  onChangeText={(val) => {
                    const newItems = [...items];
                    newItems[index].quantity = parseInt(val) || 1;
                    setItems(newItems);
                  }}
                />
                <TouchableOpacity onPress={() => removeItem(index)}>
                  <Text style={{ fontSize: 20 }}>🗑️</Text>
                </TouchableOpacity>
              </View>
            ))}

            <View style={{ height: 40 }} />
          </ScrollView>

          <TouchableOpacity 
            onPress={handleSubmit}
            disabled={loading}
            style={{ 
              backgroundColor: colors.primary[600], 
              padding: 18, borderRadius: 20, 
              alignItems: 'center', marginBottom: 20,
              opacity: loading ? 0.7 : 1
            }}
          >
            {loading ? (
              <ActivityIndicator color="white" />
            ) : (
              <Text style={{ color: 'white', fontWeight: '900', letterSpacing: 1.2 }}>FINALIZAR PEDIDO</Text>
            )}
          </TouchableOpacity>
        </KeyboardAvoidingView>
      </View>
    </Modal>
  );
}

const styles = {
  input: {
    backgroundColor: colors.gray[50],
    padding: 16,
    borderRadius: 16,
    fontSize: 14,
    fontWeight: '600' as '600',
    color: colors.gray[900],
    marginBottom: 12
  }
};
