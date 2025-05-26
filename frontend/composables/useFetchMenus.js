export async function useFetchMenus() {
  const config = useRuntimeConfig()

  try {
    const res = await fetch(`${config.public.apiBaseUrl}/training/menu`)
    const data = await res.json()

    console.log('API Response:', data)

    const menus = data.respons_menus

    if (data.code !== '00000' || !Array.isArray(menus)) {
      throw new Error('レスポンス形式が不正です')
    }

    return menus.map(m => ({
      jpName: m.jpName,
      description: m.description,
      tags: m.tags,
      menuId: m.menuId
    }))
  } catch (e) {
    console.error('メニュー取得に失敗しました', e)
    return []
  }
}
