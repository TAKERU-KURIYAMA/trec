import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { authService, trainingService } from '../../src/services/api';

// Mock dependencies
jest.mock('axios');
jest.mock('@react-native-async-storage/async-storage');

const mockedAxios = axios as jest.Mocked<typeof axios>;
const mockedAsyncStorage = AsyncStorage as jest.Mocked<typeof AsyncStorage>;

// Mock axios.create
const mockAxiosInstance = {
  get: jest.fn(),
  post: jest.fn(),
  delete: jest.fn(),
  interceptors: {
    request: {
      use: jest.fn(),
    },
    response: {
      use: jest.fn(),
    },
  },
};

mockedAxios.create = jest.fn(() => mockAxiosInstance);

describe('API Service', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('axios instance configuration', () => {
    it('should create axios instance with correct base URL and timeout', () => {
      expect(mockedAxios.create).toHaveBeenCalledWith({
        baseURL: 'http://local-trecplans:8080/api',
        timeout: 10000,
      });
    });

    it('should set up request interceptor', () => {
      expect(mockAxiosInstance.interceptors.request.use).toHaveBeenCalled();
    });

    it('should set up response interceptor', () => {
      expect(mockAxiosInstance.interceptors.response.use).toHaveBeenCalled();
    });
  });

  describe('authService', () => {
    describe('login', () => {
      it('should call POST /Account/GetToken with email and password', () => {
        const email = 'test@example.com';
        const password = 'password123';

        authService.login(email, password);

        expect(mockAxiosInstance.post).toHaveBeenCalledWith('/Account/GetToken', {
          email,
          password,
        });
      });
    });

    describe('register', () => {
      it('should call POST /Account/PostUser with user data', () => {
        const userData = {
          email: 'test@example.com',
          password: 'password123',
          displayName: 'Test User',
        };

        authService.register(userData);

        expect(mockAxiosInstance.post).toHaveBeenCalledWith('/Account/PostUser', userData);
      });
    });
  });

  describe('trainingService', () => {
    describe('getMenus', () => {
      it('should call GET /Training/GetMenu', () => {
        trainingService.getMenus();

        expect(mockAxiosInstance.get).toHaveBeenCalledWith('/Training/GetMenu');
      });
    });

    describe('getHistory', () => {
      it('should call GET /Training/GetRecord without params when no params provided', () => {
        trainingService.getHistory();

        expect(mockAxiosInstance.get).toHaveBeenCalledWith('/Training/GetRecord', {
          params: undefined,
        });
      });

      it('should call GET /Training/GetRecord with params when params provided', () => {
        const params = { from: '2023-01-01', to: '2023-12-31' };

        trainingService.getHistory(params);

        expect(mockAxiosInstance.get).toHaveBeenCalledWith('/Training/GetRecord', {
          params,
        });
      });
    });

    describe('getDailyRecord', () => {
      it('should call GET /Training/GetDaily with date param', () => {
        const date = '2023-06-15';

        trainingService.getDailyRecord(date);

        expect(mockAxiosInstance.get).toHaveBeenCalledWith('/Training/GetDaily', {
          params: { date },
        });
      });
    });

    describe('saveRecord', () => {
      it('should call POST /Training/PostRecord with record data', () => {
        const record = {
          menuId: 'menu1',
          weight: 100,
          reps: 10,
          date: '2023-06-15',
        };

        trainingService.saveRecord(record);

        expect(mockAxiosInstance.post).toHaveBeenCalledWith('/Training/PostRecord', record);
      });
    });

    describe('deleteRecord', () => {
      it('should call DELETE /Training/DeleteRecord with record ID', () => {
        const recordId = 'record123';

        trainingService.deleteRecord(recordId);

        expect(mockAxiosInstance.delete).toHaveBeenCalledWith(
          `/Training/DeleteRecord/${recordId}`
        );
      });
    });
  });

  describe('request interceptor', () => {
    it('should add authorization header when token exists', async () => {
      const mockToken = 'mock-jwt-token';
      mockedAsyncStorage.getItem.mockResolvedValue(mockToken);

      // Get the request interceptor function
      const requestInterceptor = mockAxiosInstance.interceptors.request.use.mock.calls[0][0];
      const config = { headers: {} };

      const result = await requestInterceptor(config);

      expect(mockedAsyncStorage.getItem).toHaveBeenCalledWith('@TrecPlans:token');
      expect(result.headers.Authorization).toBe(`Bearer ${mockToken}`);
    });

    it('should not add authorization header when token does not exist', async () => {
      mockedAsyncStorage.getItem.mockResolvedValue(null);

      const requestInterceptor = mockAxiosInstance.interceptors.request.use.mock.calls[0][0];
      const config = { headers: {} };

      const result = await requestInterceptor(config);

      expect(result.headers.Authorization).toBeUndefined();
    });

    it('should reject promise when error occurs', async () => {
      const requestInterceptor = mockAxiosInstance.interceptors.request.use.mock.calls[0][1];
      const error = new Error('Request error');

      await expect(requestInterceptor(error)).rejects.toBe(error);
    });
  });

  describe('response interceptor', () => {
    it('should return response when successful', () => {
      const response = { data: { success: true } };
      const responseInterceptor = mockAxiosInstance.interceptors.response.use.mock.calls[0][0];

      const result = responseInterceptor(response);

      expect(result).toBe(response);
    });

    it('should clear storage and reject when response status is 401', async () => {
      const error = {
        response: { status: 401 },
      };
      const errorInterceptor = mockAxiosInstance.interceptors.response.use.mock.calls[0][1];

      await expect(errorInterceptor(error)).rejects.toBe(error);
      expect(mockedAsyncStorage.multiRemove).toHaveBeenCalledWith([
        '@TrecPlans:token',
        '@TrecPlans:user',
      ]);
    });

    it('should reject without clearing storage for non-401 errors', async () => {
      const error = {
        response: { status: 500 },
      };
      const errorInterceptor = mockAxiosInstance.interceptors.response.use.mock.calls[0][1];

      await expect(errorInterceptor(error)).rejects.toBe(error);
      expect(mockedAsyncStorage.multiRemove).not.toHaveBeenCalled();
    });

    it('should reject without clearing storage when no response object', async () => {
      const error = new Error('Network error');
      const errorInterceptor = mockAxiosInstance.interceptors.response.use.mock.calls[0][1];

      await expect(errorInterceptor(error)).rejects.toBe(error);
      expect(mockedAsyncStorage.multiRemove).not.toHaveBeenCalled();
    });
  });
});