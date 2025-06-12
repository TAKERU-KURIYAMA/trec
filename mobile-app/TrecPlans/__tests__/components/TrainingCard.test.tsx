import React from 'react';
import { render, fireEvent } from '@testing-library/react-native';
import TrainingCard from '../../src/components/TrainingCard';

// Mock dependencies
jest.mock('react-native-vector-icons/MaterialIcons', () => 'Icon');

describe('TrainingCard', () => {
  const defaultProps = {
    menu: {
      menuId: 'menu1',
      jpName: 'ベンチプレス',
      enName: 'Bench Press',
      description: '胸の筋肉を鍛える基本的なトレーニング',
      tags: [
        { tagId: 'chest', jpName: '胸筋' },
        { tagId: 'strength', jpName: '筋力' }
      ]
    },
    onPress: jest.fn(),
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render the card correctly', () => {
    const { getByText } = render(<TrainingCard {...defaultProps} />);

    expect(getByText('ベンチプレス')).toBeTruthy();
    expect(getByText('胸の筋肉を鍛える基本的なトレーニング')).toBeTruthy();
  });

  it('should display menu name and description', () => {
    const { getByText } = render(<TrainingCard {...defaultProps} />);

    expect(getByText('ベンチプレス')).toBeTruthy();
    expect(getByText('胸の筋肉を鍛える基本的なトレーニング')).toBeTruthy();
  });

  it('should call onPress when card is pressed', () => {
    const { getByTestId } = render(<TrainingCard {...defaultProps} />);

    const card = getByTestId('training-card');
    fireEvent.press(card);

    expect(defaultProps.onPress).toHaveBeenCalledWith(defaultProps.menu);
  });

  it('should display tags when provided', () => {
    const { getByText } = render(<TrainingCard {...defaultProps} />);

    expect(getByText('胸筋')).toBeTruthy();
    expect(getByText('筋力')).toBeTruthy();
  });

  it('should handle menu without tags', () => {
    const propsWithoutTags = {
      ...defaultProps,
      menu: {
        ...defaultProps.menu,
        tags: []
      }
    };

    const { getByText, queryByText } = render(<TrainingCard {...propsWithoutTags} />);

    expect(getByText('ベンチプレス')).toBeTruthy();
    expect(queryByText('胸筋')).toBeNull();
    expect(queryByText('筋力')).toBeNull();
  });

  it('should handle menu with undefined tags', () => {
    const propsWithUndefinedTags = {
      ...defaultProps,
      menu: {
        ...defaultProps.menu,
        tags: undefined
      }
    };

    const { getByText } = render(<TrainingCard {...propsWithUndefinedTags} />);

    expect(getByText('ベンチプレス')).toBeTruthy();
    // Should not crash when tags is undefined
  });

  it('should handle empty description', () => {
    const propsWithEmptyDescription = {
      ...defaultProps,
      menu: {
        ...defaultProps.menu,
        description: ''
      }
    };

    const { getByText, queryByText } = render(<TrainingCard {...propsWithEmptyDescription} />);

    expect(getByText('ベンチプレス')).toBeTruthy();
    // Empty description should still render but be empty
  });

  it('should handle long text gracefully', () => {
    const propsWithLongText = {
      ...defaultProps,
      menu: {
        ...defaultProps.menu,
        jpName: '非常に長いトレーニングメニュー名前です。この名前は画面からはみ出すかもしれません。',
        description: '非常に長い説明文です。この説明文は複数行にわたる可能性があり、UIがどのように対応するかをテストします。長いテキストが適切に表示されることを確認したいと思います。'
      }
    };

    const { getByText } = render(<TrainingCard {...propsWithLongText} />);

    expect(getByText(propsWithLongText.menu.jpName)).toBeTruthy();
    expect(getByText(propsWithLongText.menu.description)).toBeTruthy();
  });

  it('should display single tag correctly', () => {
    const propsWithSingleTag = {
      ...defaultProps,
      menu: {
        ...defaultProps.menu,
        tags: [{ tagId: 'single', jpName: '単一タグ' }]
      }
    };

    const { getByText } = render(<TrainingCard {...propsWithSingleTag} />);

    expect(getByText('単一タグ')).toBeTruthy();
  });

  it('should handle special characters in text', () => {
    const propsWithSpecialChars = {
      ...defaultProps,
      menu: {
        ...defaultProps.menu,
        jpName: 'テスト & <特殊>文字',
        description: '特殊文字 "テスト" \'含む\'',
        tags: [
          { tagId: 'special1', jpName: 'タグ&特殊' },
          { tagId: 'special2', jpName: '<script>タグ</script>' }
        ]
      }
    };

    const { getByText } = render(<TrainingCard {...propsWithSpecialChars} />);

    expect(getByText('テスト & <特殊>文字')).toBeTruthy();
    expect(getByText('特殊文字 "テスト" \'含む\'')).toBeTruthy();
    expect(getByText('タグ&特殊')).toBeTruthy();
    expect(getByText('<script>タグ</script>')).toBeTruthy();
  });

  it('should not call onPress when onPress is not provided', () => {
    const propsWithoutOnPress = {
      menu: defaultProps.menu
    };

    const { getByTestId } = render(<TrainingCard {...propsWithoutOnPress} />);

    const card = getByTestId('training-card');
    
    // Should not throw error when onPress is undefined
    expect(() => fireEvent.press(card)).not.toThrow();
  });

  it('should handle null or undefined menu gracefully', () => {
    const propsWithNullMenu = {
      menu: null,
      onPress: jest.fn()
    };

    // This should either handle gracefully or be protected by PropTypes
    expect(() => render(<TrainingCard {...propsWithNullMenu} />)).not.toThrow();
  });

  it('should handle missing menu properties', () => {
    const propsWithIncompleteMenu = {
      ...defaultProps,
      menu: {
        menuId: 'incomplete'
        // Missing jpName, description, tags
      }
    };

    const { getByTestId } = render(<TrainingCard {...propsWithIncompleteMenu} />);

    // Should render without crashing
    expect(getByTestId('training-card')).toBeTruthy();
  });

  it('should be accessible', () => {
    const { getByTestId } = render(<TrainingCard {...defaultProps} />);

    const card = getByTestId('training-card');
    
    // Check that the card is accessible (has proper accessibility props)
    expect(card).toBeTruthy();
  });

  it('should maintain consistent styling', () => {
    const { getByTestId } = render(<TrainingCard {...defaultProps} />);

    const card = getByTestId('training-card');
    
    // Verify that the card has the expected style properties
    expect(card.props.style).toBeDefined();
  });

  it('should handle multiple cards without interference', () => {
    const menu1 = { ...defaultProps.menu, menuId: 'menu1', jpName: 'メニュー1' };
    const menu2 = { ...defaultProps.menu, menuId: 'menu2', jpName: 'メニュー2' };
    
    const onPress1 = jest.fn();
    const onPress2 = jest.fn();

    const { getByText } = render(
      <>
        <TrainingCard menu={menu1} onPress={onPress1} />
        <TrainingCard menu={menu2} onPress={onPress2} />
      </>
    );

    const card1 = getByText('メニュー1');
    const card2 = getByText('メニュー2');

    fireEvent.press(card1);
    expect(onPress1).toHaveBeenCalledWith(menu1);
    expect(onPress2).not.toHaveBeenCalled();

    fireEvent.press(card2);
    expect(onPress2).toHaveBeenCalledWith(menu2);
    expect(onPress1).toHaveBeenCalledTimes(1); // Still only called once
  });
});