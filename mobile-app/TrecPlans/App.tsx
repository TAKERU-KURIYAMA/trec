import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import Icon from 'react-native-vector-icons/MaterialIcons';

import HomeScreen from './src/screens/HomeScreen';
import HistoryScreen from './src/screens/HistoryScreen';
import DashboardScreen from './src/screens/DashboardScreen';
import { AuthProvider } from './src/contexts/AuthContext';

const Tab = createBottomTabNavigator();

function App(): JSX.Element {
  return (
    <SafeAreaProvider>
      <AuthProvider>
        <NavigationContainer>
          <Tab.Navigator
            screenOptions={({ route }) => ({
              tabBarIcon: ({ focused, color, size }) => {
                let iconName;

                if (route.name === 'Home') {
                  iconName = 'fitness-center';
                } else if (route.name === 'History') {
                  iconName = 'history';
                } else if (route.name === 'Dashboard') {
                  iconName = 'dashboard';
                }

                return <Icon name={iconName!} size={size} color={color} />;
              },
              tabBarActiveTintColor: '#007AFF',
              tabBarInactiveTintColor: 'gray',
            })}
          >
            <Tab.Screen 
              name="Home" 
              component={HomeScreen} 
              options={{ title: 'トレーニング' }}
            />
            <Tab.Screen 
              name="History" 
              component={HistoryScreen} 
              options={{ title: '履歴' }}
            />
            <Tab.Screen 
              name="Dashboard" 
              component={DashboardScreen} 
              options={{ title: 'ダッシュボード' }}
            />
          </Tab.Navigator>
        </NavigationContainer>
      </AuthProvider>
    </SafeAreaProvider>
  );
}

export default App;