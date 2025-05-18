# 📘 API I/O 仕様書

このドキュメントは、現在実装済みの API におけるリクエストおよびレスポンスの構造を記載しています。

---

## 🔐 POST `/api/auth/register` - ユーザー登録

### Request Body

```json
{
  "email": "user@example.com",
  "password": "YourPassword123!",
  "displayName": "表示名"
}
```

### Response

- `204 No Content`: 登録成功
- `409 Conflict`: メールアドレス重複

---

## 🔐 POST `/api/auth/token` - トークン発行（ログイン）

### Request Body

```json
{
  "email": "user@example.com",
  "password": "YourPassword123!"
}
```

### Response Body

```json
{
  "accessToken": "JWT_TOKEN"
}
```

### Response

- `200 OK`: 認証成功
- `401 Unauthorized`: メールまたはパスワード不一致

---

## 🏋️‍♂️ POST `/api/training/record` - トレーニング記録登録

### Header

```
Authorization: Bearer {accessToken}
```

### Request Body

```json
{
  "trainingDate": "2025-05-01",
  "menuId": 1,
  "sets": [
    { "setNumber": 1, "reps": 10, "weight": 60.0 },
    { "setNumber": 2, "reps": 8, "weight": 65.0 }
  ]
}
```

### Response

- `204 No Content`: 登録成功
- `409 Conflict`: 同じセットが既に登録されている
- `401 Unauthorized`: トークン不正

---

## 📊 GET `/api/training/history` - トレーニング履歴取得

### Header

```
Authorization: Bearer {accessToken}
```

### Query Parameters（任意）

- `startDate`: yyyy-MM-dd
- `endDate`: yyyy-MM-dd

### Response Body

```json
[
  {
    "trainingDate": "2025-04-30",
    "menuId": 1,
    "sets": [
      { "setNumber": 1, "reps": 10, "weight": 60.0 },
      { "setNumber": 2, "reps": 8, "weight": 65.0 }
    ]
  }
]
```

### Response

- `200 OK`: データ取得成功
- `401 Unauthorized`: トークン不正

---
