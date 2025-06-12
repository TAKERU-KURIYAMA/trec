import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { useFetchMenus } from '../../composables/useFetchMenus.js'

describe('useFetchMenus', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('should return menu data when API call is successful', async () => {
    // Arrange
    const mockMenuData = {
      code: '00000',
      response_menus: [
        {
          MenuId: 'menu1',
          JPName: 'ベンチプレス',
          ENName: 'Bench Press',
          Description: '胸の筋肉を鍛える',
          Tags: [{ TagId: 'tag1' }]
        },
        {
          MenuId: 'menu2',
          JPName: 'スクワット',
          ENName: 'Squat',
          Description: '下半身を鍛える',
          Tags: [{ TagId: 'tag2' }]
        }
      ]
    }

    global.fetch = vi.fn().mockResolvedValue({
      json: () => Promise.resolve(mockMenuData)
    })

    // Act
    const result = await useFetchMenus()

    // Assert
    expect(result).toHaveLength(2)
    expect(result[0]).toEqual({
      jpName: 'ベンチプレス',
      description: '胸の筋肉を鍛える',
      tags: [{ TagId: 'tag1' }],
      menuId: 'menu1'
    })
    expect(result[1]).toEqual({
      jpName: 'スクワット',
      description: '下半身を鍛える',
      tags: [{ TagId: 'tag2' }],
      menuId: 'menu2'
    })
  })

  it('should call the correct API endpoint', async () => {
    // Arrange
    const mockMenuData = {
      code: '00000',
      response_menus: []
    }

    global.fetch = vi.fn().mockResolvedValue({
      json: () => Promise.resolve(mockMenuData)
    })

    // Act
    await useFetchMenus()

    // Assert
    expect(global.fetch).toHaveBeenCalledWith('http://localhost:8080/api/training/menu')
  })

  it('should return empty array when API returns error code', async () => {
    // Arrange
    const mockErrorData = {
      code: '99999',
      response_menus: [
        {
          MenuId: 'menu1',
          JPName: 'ベンチプレス',
          ENName: 'Bench Press',
          Description: '胸の筋肉を鍛える',
          Tags: []
        }
      ]
    }

    global.fetch = vi.fn().mockResolvedValue({
      json: () => Promise.resolve(mockErrorData)
    })

    // Act
    const result = await useFetchMenus()

    // Assert
    expect(result).toEqual([])
  })

  it('should return empty array when response_menus is not an array', async () => {
    // Arrange
    const mockInvalidData = {
      code: '00000',
      response_menus: null
    }

    global.fetch = vi.fn().mockResolvedValue({
      json: () => Promise.resolve(mockInvalidData)
    })

    // Act
    const result = await useFetchMenus()

    // Assert
    expect(result).toEqual([])
  })

  it('should return empty array when fetch throws an error', async () => {
    // Arrange
    global.fetch = vi.fn().mockRejectedValue(new Error('Network error'))

    // Act
    const result = await useFetchMenus()

    // Assert
    expect(result).toEqual([])
  })

  it('should return empty array when JSON parsing fails', async () => {
    // Arrange
    global.fetch = vi.fn().mockResolvedValue({
      json: () => Promise.reject(new Error('Invalid JSON'))
    })

    // Act
    const result = await useFetchMenus()

    // Assert
    expect(result).toEqual([])
  })

  it('should handle empty response_menus array', async () => {
    // Arrange
    const mockEmptyData = {
      code: '00000',
      response_menus: []
    }

    global.fetch = vi.fn().mockResolvedValue({
      json: () => Promise.resolve(mockEmptyData)
    })

    // Act
    const result = await useFetchMenus()

    // Assert
    expect(result).toEqual([])
  })

  it('should map menu properties correctly', async () => {
    // Arrange
    const mockMenuData = {
      code: '00000',
      response_menus: [
        {
          MenuId: 'test-menu-id',
          JPName: 'テストメニュー',
          ENName: 'Test Menu',
          Description: 'テスト用の説明',
          Tags: [{ TagId: 'test-tag' }]
        }
      ]
    }

    global.fetch = vi.fn().mockResolvedValue({
      json: () => Promise.resolve(mockMenuData)
    })

    // Act
    const result = await useFetchMenus()

    // Assert
    expect(result[0]).toHaveProperty('jpName', 'テストメニュー')
    expect(result[0]).toHaveProperty('description', 'テスト用の説明')
    expect(result[0]).toHaveProperty('tags', [{ TagId: 'test-tag' }])
    expect(result[0]).toHaveProperty('menuId', 'test-menu-id')
  })

  it('should handle missing properties gracefully', async () => {
    // Arrange
    const mockIncompleteData = {
      code: '00000',
      response_menus: [
        {
          MenuId: 'incomplete-menu'
          // Missing JPName, ENName, Description, Tags
        }
      ]
    }

    global.fetch = vi.fn().mockResolvedValue({
      json: () => Promise.resolve(mockIncompleteData)
    })

    // Act
    const result = await useFetchMenus()

    // Assert
    expect(result).toHaveLength(1)
    expect(result[0]).toHaveProperty('menuId', 'incomplete-menu')
    expect(result[0]).toHaveProperty('jpName', undefined)
    expect(result[0]).toHaveProperty('description', undefined)
    expect(result[0]).toHaveProperty('tags', undefined)
  })
})