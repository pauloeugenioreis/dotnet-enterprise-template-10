import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, ActivityIndicator, SafeAreaView } from 'react-native';
import { colors } from '../../theme/colors';
import apiClient from '../../api/apiClient';
import * as SecureStore from 'expo-secure-store';

export default function LoginScreen({ navigation }: any) {
  const [email, setEmail] = useState('admin@projecttemplate.com');
  const [password, setPassword] = useState('Admin@2026!Secure');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleLogin = async () => {
    if (!email || !password) {
      setError('Preencha todos os campos');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const response = await apiClient.post('/api/auth/login', { email, password });
      const { token } = response.data;
      
      await SecureStore.setItemAsync('auth_token', token);
      navigation.replace('MainTabs');
    } catch (err) {
      setError('Credenciais inválidas');
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colors.primary[50] }}>
      <View style={{ flex: 1, padding: 32, justifyContent: 'center' }}>
        
        {/* Logo */}
        <View style={{ 
          width: 80, height: 80, backgroundColor: colors.primary[600], 
          borderRadius: 24, alignSelf: 'center', marginBottom: 24,
          justifyContent: 'center', alignItems: 'center',
          shadowColor: colors.primary[600], shadowOffset: { width: 0, height: 8 },
          shadowOpacity: 0.3, shadowRadius: 10, elevation: 8
        }}>
          <Text style={{ color: 'white', fontSize: 32, fontWeight: '900' }}>ET</Text>
        </View>

        <Text style={{ fontSize: 28, fontWeight: '900', color: colors.primary[900], textAlign: 'center' }}>
          Enterprise Template
        </Text>
        <Text style={{ fontSize: 14, color: colors.gray[500], textAlign: 'center', marginBottom: 40 }}>
          Acesse sua conta corporativa
        </Text>

        {/* Inputs */}
        <Text style={{ fontSize: 12, fontWeight: 'bold', color: colors.gray[700], marginBottom: 8 }}>E-mail</Text>
        <TextInput
          style={{ 
            backgroundColor: 'white', borderRadius: 16, padding: 16, 
            borderWidth: 1, borderColor: colors.gray[200], marginBottom: 20 
          }}
          placeholder="seu@email.com"
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          keyboardType="email-address"
        />

        <Text style={{ fontSize: 12, fontWeight: 'bold', color: colors.gray[700], marginBottom: 8 }}>Senha</Text>
        <TextInput
          style={{ 
            backgroundColor: 'white', borderRadius: 16, padding: 16, 
            borderWidth: 1, borderColor: colors.gray[200], marginBottom: 32 
          }}
          placeholder="••••••••"
          secureTextEntry
          value={password}
          onChangeText={setPassword}
        />

        {error ? <Text style={{ color: colors.error, textAlign: 'center', marginBottom: 16 }}>{error}</Text> : null}

        <TouchableOpacity 
          onPress={handleLogin}
          disabled={loading}
          style={{ 
            backgroundColor: colors.primary[600], borderRadius: 16, height: 56,
            justifyContent: 'center', alignItems: 'center', elevation: 4
          }}
        >
          {loading ? <ActivityIndicator color="white" /> : (
            <Text style={{ color: 'white', fontWeight: 'bold', fontSize: 16 }}>Entrar na Plataforma</Text>
          )}
        </TouchableOpacity>

      </View>
    </SafeAreaView>
  );
}
