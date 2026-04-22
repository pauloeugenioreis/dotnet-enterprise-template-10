import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import LoginScreen from './src/features/auth/LoginScreen';
import DashboardScreen from './src/features/dashboard/DashboardScreen';
import OrdersScreen from './src/features/orders/OrdersScreen';
import AuditScreen from './src/features/audit/AuditScreen';
import { colors } from './src/theme/colors';

const Stack = createNativeStackNavigator();
const Tab = createBottomTabNavigator();

function MainTabs() {
  return (
    <Tab.Navigator screenOptions={{
      tabBarActiveTintColor: colors.primary[600],
      tabBarInactiveTintColor: colors.gray[400],
      headerShown: false,
      tabBarStyle: { height: 60, paddingBottom: 10 }
    }}>
      <Tab.Screen name="Dashboard" component={DashboardScreen} />
      <Tab.Screen name="Pedidos" component={OrdersScreen} />
      <Tab.Screen name="Auditoria" component={AuditScreen} />
    </Tab.Navigator>
  );
}

export default function App() {
  return (
    <NavigationContainer>
      <Stack.Navigator initialRouteName="Login" screenOptions={{ headerShown: false }}>
        <Stack.Screen name="Login" component={LoginScreen} />
        <Stack.Screen name="MainTabs" component={MainTabs} />
      </Stack.Navigator>
    </NavigationContainer>
  );
}
