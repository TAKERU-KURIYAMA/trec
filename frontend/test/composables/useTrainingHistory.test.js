import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import axios from 'axios'
import useTrainingHistory from '../../composables/useTrainingHistory.js'

// Mock axios
vi.mock('axios')
const mockedAxios = vi.mocked(axios)

// Mock alert
global.alert = vi.fn()

describe('useTrainingHistory', () => {
  let trainingHistory

  beforeEach(() => {
    vi.clearAllMocks()
    trainingHistory = useTrainingHistory()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('should initialize with empty records and menu name', () => {
    // Assert
    expect(trainingHistory.records.value).toEqual([])
    expect(trainingHistory.menuName.value).toBe('')
  })

  it('should fetch training history successfully', async () => {
    // Arrange
    const mockResponse = {
      data: {
        code: '0000',
        menuName: 'ベンチプレス',
        records: [
          { id: 1, weight: 100, reps: 10, date: '2023-01-01' },
          { id: 2, weight: 105, reps: 8, date: '2023-01-02' }
        ]
      }
    }

    mockedAxios.get.mockResolvedValue(mockResponse)

    // Act
    await trainingHistory.fetchHistory('menu1')

    // Assert
    expect(trainingHistory.menuName.value).toBe('ベンチプレス')
    expect(trainingHistory.records.value).toHaveLength(2)
    expect(trainingHistory.records.value[0]).toEqual({
      id: 1,
      weight: 100,
      reps: 10,
      date: '2023-01-01'
    })
  })

  it('should call API with correct parameters', async () => {
    // Arrange
    const mockResponse = {
      data: {
        code: '0000',
        menuName: 'テストメニュー',
        records: []
      }
    }

    mockedAxios.get.mockResolvedValue(mockResponse)

    // Act
    await trainingHistory.fetchHistory('test-menu-id')

    // Assert
    expect(mockedAxios.get).toHaveBeenCalledWith(
      'http://localhost:8080/api/training/daily',
      {
        params: { menuId: 'test-menu-id' }
      }
    )
  })

  it('should show alert when menuId is not provided', async () => {
    // Act
    await trainingHistory.fetchHistory()

    // Assert
    expect(global.alert).toHaveBeenCalledWith('メニューIDが指定されていません')
    expect(mockedAxios.get).not.toHaveBeenCalled()
  })

  it('should show alert when menuId is empty string', async () => {
    // Act
    await trainingHistory.fetchHistory('')

    // Assert
    expect(global.alert).toHaveBeenCalledWith('メニューIDが指定されていません')
    expect(mockedAxios.get).not.toHaveBeenCalled()
  })

  it('should show alert when API returns error code', async () => {
    // Arrange
    const mockResponse = {
      data: {
        code: '9999',
        message: 'Error occurred'
      }
    }

    mockedAxios.get.mockResolvedValue(mockResponse)

    // Act
    await trainingHistory.fetchHistory('menu1')

    // Assert
    expect(global.alert).toHaveBeenCalledWith('履歴取得に失敗しました')
    expect(trainingHistory.records.value).toEqual([])
    expect(trainingHistory.menuName.value).toBe('')
  })

  it('should handle API request failure', async () => {
    // Arrange
    mockedAxios.get.mockRejectedValue(new Error('Network error'))

    // Act
    await trainingHistory.fetchHistory('menu1')

    // Assert
    expect(global.alert).toHaveBeenCalledWith('トレーニング履歴の取得に失敗しました')
    expect(trainingHistory.records.value).toEqual([])
    expect(trainingHistory.menuName.value).toBe('')
  })

  it('should handle missing menuName in response', async () => {
    // Arrange
    const mockResponse = {
      data: {
        code: '0000',
        records: [{ id: 1, weight: 100, reps: 10 }]
      }
    }

    mockedAxios.get.mockResolvedValue(mockResponse)

    // Act
    await trainingHistory.fetchHistory('menu1')

    // Assert
    expect(trainingHistory.menuName.value).toBe('メニュー名不明')
    expect(trainingHistory.records.value).toHaveLength(1)
  })

  it('should handle missing records in response', async () => {
    // Arrange
    const mockResponse = {
      data: {
        code: '0000',
        menuName: 'テストメニュー'
      }
    }

    mockedAxios.get.mockResolvedValue(mockResponse)

    // Act
    await trainingHistory.fetchHistory('menu1')

    // Assert
    expect(trainingHistory.menuName.value).toBe('テストメニュー')
    expect(trainingHistory.records.value).toEqual([])
  })

  it('should handle null menuName and records in response', async () => {
    // Arrange
    const mockResponse = {
      data: {
        code: '0000',
        menuName: null,
        records: null
      }
    }

    mockedAxios.get.mockResolvedValue(mockResponse)

    // Act
    await trainingHistory.fetchHistory('menu1')

    // Assert
    expect(trainingHistory.menuName.value).toBe('メニュー名不明')
    expect(trainingHistory.records.value).toEqual([])
  })

  it('should maintain reactive state across multiple calls', async () => {
    // Arrange
    const mockResponse1 = {
      data: {
        code: '0000',
        menuName: 'メニュー1',
        records: [{ id: 1 }]
      }
    }

    const mockResponse2 = {
      data: {
        code: '0000',
        menuName: 'メニュー2',
        records: [{ id: 2 }, { id: 3 }]
      }
    }

    mockedAxios.get
      .mockResolvedValueOnce(mockResponse1)
      .mockResolvedValueOnce(mockResponse2)

    // Act
    await trainingHistory.fetchHistory('menu1')
    expect(trainingHistory.records.value).toHaveLength(1)
    expect(trainingHistory.menuName.value).toBe('メニュー1')

    await trainingHistory.fetchHistory('menu2')

    // Assert
    expect(trainingHistory.records.value).toHaveLength(2)
    expect(trainingHistory.menuName.value).toBe('メニュー2')
  })
})