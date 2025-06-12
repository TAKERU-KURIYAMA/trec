import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';

const API_BASE_URL = 'http://local-trecplans:8080/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
});

api.interceptors.request.use(
  async (config) => {
    const token = await AsyncStorage.getItem('@TrecPlans:token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      await AsyncStorage.multiRemove(['@TrecPlans:token', '@TrecPlans:user']);
    }
    return Promise.reject(error);
  }
);

export const authService = {
  login: (email: string, password: string) => 
    api.post('/Account/GetToken', { email, password }),
  
  register: (userData: any) => 
    api.post('/Account/PostUser', userData),
};

export const trainingService = {
  getMenus: () => 
    api.get('/Training/GetMenu'),
  
  getHistory: (params?: { from?: string; to?: string }) => 
    api.get('/Training/GetRecord', { params }),
  
  getDailyRecord: (date: string) => 
    api.get('/Training/GetDaily', { params: { date } }),
  
  saveRecord: (record: any) => 
    api.post('/Training/PostRecord', record),
  
  deleteRecord: (recordId: string) => 
    api.delete(`/Training/DeleteRecord/${recordId}`),
};

export default api;