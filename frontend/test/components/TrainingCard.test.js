import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import TrainingCard from '../../components/TrainingCard.vue'

describe('TrainingCard', () => {
  const defaultProps = {
    menu: {
      jpName: 'ベンチプレス',
      description: '胸の筋肉を鍛える基本的なトレーニング',
      tags: [
        { tagId: 'chest' },
        { tagId: 'strength' }
      ]
    }
  }

  it('should render the component correctly', () => {
    // Act
    const wrapper = mount(TrainingCard, {
      props: defaultProps
    })

    // Assert
    expect(wrapper.exists()).toBe(true)
    expect(wrapper.find('.training-card').exists()).toBe(true)
  })

  it('should display the menu name', () => {
    // Act
    const wrapper = mount(TrainingCard, {
      props: defaultProps
    })

    // Assert
    expect(wrapper.find('h2').text()).toBe('ベンチプレス')
  })

  it('should display the menu description', () => {
    // Act
    const wrapper = mount(TrainingCard, {
      props: defaultProps
    })

    // Assert
    expect(wrapper.find('p').text()).toBe('胸の筋肉を鍛える基本的なトレーニング')
  })

  it('should display all tags', () => {
    // Act
    const wrapper = mount(TrainingCard, {
      props: defaultProps
    })

    // Assert
    const tags = wrapper.findAll('.tag')
    expect(tags).toHaveLength(2)
    expect(tags[0].text()).toBe('chest')
    expect(tags[1].text()).toBe('strength')
  })

  it('should handle menu with no tags', () => {
    // Arrange
    const props = {
      menu: {
        jpName: 'プルアップ',
        description: '背中の筋肉を鍛える',
        tags: []
      }
    }

    // Act
    const wrapper = mount(TrainingCard, {
      props
    })

    // Assert
    expect(wrapper.find('h2').text()).toBe('プルアップ')
    expect(wrapper.find('p').text()).toBe('背中の筋肉を鍛える')
    expect(wrapper.findAll('.tag')).toHaveLength(0)
  })

  it('should handle menu with undefined tags', () => {
    // Arrange
    const props = {
      menu: {
        jpName: 'スクワット',
        description: '下半身を鍛える',
        tags: undefined
      }
    }

    // Act
    const wrapper = mount(TrainingCard, {
      props
    })

    // Assert
    expect(wrapper.find('h2').text()).toBe('スクワット')
    expect(wrapper.find('p').text()).toBe('下半身を鍛える')
    expect(wrapper.findAll('.tag')).toHaveLength(0)
  })

  it('should handle menu with single tag', () => {
    // Arrange
    const props = {
      menu: {
        jpName: 'ランニング',
        description: '有酸素運動',
        tags: [{ tagId: 'cardio' }]
      }
    }

    // Act
    const wrapper = mount(TrainingCard, {
      props
    })

    // Assert
    const tags = wrapper.findAll('.tag')
    expect(tags).toHaveLength(1)
    expect(tags[0].text()).toBe('cardio')
  })

  it('should handle empty strings in menu properties', () => {
    // Arrange
    const props = {
      menu: {
        jpName: '',
        description: '',
        tags: []
      }
    }

    // Act
    const wrapper = mount(TrainingCard, {
      props
    })

    // Assert
    expect(wrapper.find('h2').text()).toBe('')
    expect(wrapper.find('p').text()).toBe('')
    expect(wrapper.findAll('.tag')).toHaveLength(0)
  })

  it('should have correct CSS classes', () => {
    // Act
    const wrapper = mount(TrainingCard, {
      props: defaultProps
    })

    // Assert
    expect(wrapper.find('.training-card').exists()).toBe(true)
    expect(wrapper.find('.tags').exists()).toBe(true)
    expect(wrapper.find('.tag').exists()).toBe(true)
  })

  it('should render tag with correct key attribute', () => {
    // Act
    const wrapper = mount(TrainingCard, {
      props: defaultProps
    })

    // Assert
    const tags = wrapper.findAll('.tag')
    // Vue Test Utils doesn't expose keys directly, but we can verify structure
    expect(tags).toHaveLength(2)
    expect(tags[0].text()).toBe('chest')
    expect(tags[1].text()).toBe('strength')
  })

  it('should handle special characters in menu properties', () => {
    // Arrange
    const props = {
      menu: {
        jpName: 'テスト & <特殊>文字',
        description: '特殊文字 "テスト" \'含む\'',
        tags: [
          { tagId: 'test&special' },
          { tagId: '<script>alert("xss")</script>' }
        ]
      }
    }

    // Act
    const wrapper = mount(TrainingCard, {
      props
    })

    // Assert
    expect(wrapper.find('h2').text()).toBe('テスト & <特殊>文字')
    expect(wrapper.find('p').text()).toBe('特殊文字 "テスト" \'含む\'')
    const tags = wrapper.findAll('.tag')
    expect(tags[0].text()).toBe('test&special')
    expect(tags[1].text()).toBe('<script>alert("xss")</script>')
  })

  it('should handle large number of tags', () => {
    // Arrange
    const manyTags = Array.from({ length: 10 }, (_, i) => ({ tagId: `tag${i}` }))
    const props = {
      menu: {
        jpName: 'マルチタグメニュー',
        description: 'たくさんのタグを持つメニュー',
        tags: manyTags
      }
    }

    // Act
    const wrapper = mount(TrainingCard, {
      props
    })

    // Assert
    const tags = wrapper.findAll('.tag')
    expect(tags).toHaveLength(10)
    tags.forEach((tag, index) => {
      expect(tag.text()).toBe(`tag${index}`)
    })
  })
})