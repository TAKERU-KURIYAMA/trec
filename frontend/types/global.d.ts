declare global {
  interface Window {
    __authToken?: string | null
  }
}

export {}