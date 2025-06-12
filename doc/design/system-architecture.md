# システムアーキテクチャ設計図

## 概要
トレーニング管理システムの全体アーキテクチャと各コンポーネントの関係を示します。

## 1. システム全体構成図

```
┌─────────────────────────────────────────────────────────────────┐
│                         外部ユーザー                              │
└─────────────────────────────────────────────────────────────────┘
                                    │
                            ┌───────┴───────┐
                            │               │
                    ┌───────▼─────┐  ┌──────▼──────┐
                    │             │  │             │
                    │ Webブラウザ  │  │ モバイルアプリ │
                    │ (Nuxt.js)   │  │(React Native)│
                    │             │  │             │
                    └───────┬─────┘  └──────┬──────┘
                            │               │
                            └───────┬───────┘
                                   │ HTTPS/HTTP
┌─────────────────────────────────────────────────────────────────┐
│                        ロードバランサー                          │
│                       (nginx reverse proxy)                    │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                          APIレイヤー                            │
│  ┌─────────────────┐        ┌─────────────────┐                 │
│  │    Api2         │        │      api        │                 │
│  │ Azure Functions │        │ ASP.NET Core    │                 │
│  │ (.NET 8)        │        │ Web API (.NET 8)│                 │
│  │                 │        │                 │                 │
│  │ ・GetVersion    │        │ ・Training API  │                 │
│  │                 │        │ ・Authentication│                 │
│  └─────────────────┘        │ ・User Management│                │
│                              └─────────────────┘                 │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                        データベースレイヤー                      │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                SQL Server Database                      │   │
│  │                                                         │   │
│  │  ┌────────────┐ ┌────────────┐ ┌────────────┐          │   │
│  │  │   Users    │ │ TrainingMenu│ │TrainingRecord│         │   │
│  │  │            │ │            │ │              │         │   │
│  │  │・UserId    │ │・MenuId    │ │・RecordId    │         │   │
│  │  │・LoginId   │ │・JPName    │ │・MenuId      │         │   │
│  │  │・Password  │ │・ENName    │ │・Weight      │         │   │
│  │  │・DisplayName│ │・Description│ │・Reps        │         │   │
│  │  └────────────┘ └────────────┘ │・Date        │         │   │
│  │                                │・UserId      │         │   │
│  │  ┌────────────┐ ┌────────────┐ └────────────┘         │   │
│  │  │ TagMaster  │ │TrainingTag │                        │   │
│  │  │            │ │            │                        │   │
│  │  │・TagId     │ │・TagId     │                        │   │
│  │  │・JPName    │ │・MenuId    │                        │   │
│  │  │・ENName    │ │            │                        │   │
│  │  └────────────┘ └────────────┘                        │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## 2. API層の詳細構成

```
┌─────────────────────────────────────────────────────────────────┐
│                          API Gateway                           │
└─────────────────────────────────────────────────────────────────┘
                                   │
        ┌─────────────────────────────────────────────┐
        │                                             │
┌───────▼─────────┐                        ┌─────────▼───────┐
│     Api2        │                        │       api       │
│ Azure Functions │                        │ ASP.NET Core    │
│                 │                        │                 │
│ Controllers:    │                        │ Controllers:    │
│ ・GetVersion    │                        │ ・ApiController │
│                 │                        │ ・TrainingCtrl  │
│ Common:         │                        │                 │
│ ・Logger        │                        │ Services:       │
│ ・ApiResponse   │                        │ ・AuthService   │
│ ・AppException  │                        │ ・IJwtService   │
│ ・FoundationCode│                        │ ・JwtService    │
└─────────────────┘                        │                 │
                                           │ Models:         │
                                           │ ・User          │
                                           │ ・TrainingMenu  │
                                           │ ・TrainingRecord│
                                           │ ・MessageRDBCtx │
                                           │                 │
                                           │ Common:         │
                                           │ ・Logger        │
                                           │ ・ApiResponse   │
                                           │ ・TrainingUtil  │
                                           │ ・ValidateUtil  │
                                           └─────────────────┘
```

## 3. フロントエンド層の詳細構成

### 3.1 Web Frontend (Nuxt.js)
```
┌─────────────────────────────────────────────────────────────────┐
│                        Nuxt.js Application                     │
│                                                                 │
│  Pages:                  Components:              Composables:  │
│  ┌────────────────┐     ┌────────────────┐      ┌─────────────┐ │
│  │ ・index.vue    │     │ ・TrainingCard │      │ ・useFetch  │ │
│  │ ・dashboard.vue│     │ ・Enhanced     │      │   Menus     │ │
│  │ ・training/    │     │   TrainingCard │      │ ・useTraining│ │
│  │   history.vue  │     │ ・Dashboard/   │      │   History   │ │
│  └────────────────┘     │   Components   │      └─────────────┘ │
│                         └────────────────┘                      │
│  Assets:                 Public:                                │
│  ┌────────────────┐     ┌────────────────┐                     │
│  │ ・CSS/SCSS     │     │ ・favicon.ico  │                     │
│  │ ・Images       │     │ ・robots.txt   │                     │
│  └────────────────┘     └────────────────┘                     │
└─────────────────────────────────────────────────────────────────┘
```

### 3.2 Mobile App (React Native)
```
┌─────────────────────────────────────────────────────────────────┐
│                    React Native Application                     │
│                                                                 │
│  Screens:               Components:              Services:      │
│  ┌────────────────┐     ┌────────────────┐      ┌─────────────┐ │
│  │ ・HomeScreen   │     │ ・TrainingCard │      │ ・api.ts    │ │
│  │ ・DashboardScr │     │                │      │   auth      │ │
│  │ ・HistoryScreen│     │                │      │   training  │ │
│  └────────────────┘     └────────────────┘      └─────────────┘ │
│                                                                 │
│  Contexts:              Types:                  Utils:          │
│  ┌────────────────┐     ┌────────────────┐      ┌─────────────┐ │
│  │ ・AuthContext  │     │ ・index.ts     │      │ ・helpers   │ │
│  │                │     │   interfaces   │      │ ・constants │ │
│  └────────────────┘     └────────────────┘      └─────────────┘ │
│                                                                 │
│  Navigation:                                                    │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ ・BottomTabNavigator                                    │   │
│  │ ・StackNavigator                                        │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## 4. データフロー詳細

### 4.1 認証フロー
```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Client    │    │  API Layer  │    │  Service    │    │  Database   │
│             │    │             │    │             │    │             │
│ 1.Login     │───►│ 2.POST      │───►│ 3.Validate  │───►│ 4.Query     │
│   Request   │    │  /GetToken  │    │   User      │    │   Users     │
│             │    │             │    │             │    │             │
│ 8.Store     │◄───│ 7.Return    │◄───│ 6.Generate  │◄───│ 5.Return    │
│   Token     │    │   JWT       │    │   JWT       │    │   User      │
│             │    │             │    │             │    │             │
│ 9.API       │───►│10.Validate  │───►│11.Verify    │    │             │
│   Requests  │    │   Token     │    │   JWT       │    │             │
│   with JWT  │    │             │    │             │    │             │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
```

### 4.2 トレーニングデータフロー
```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Client    │    │  API Layer  │    │  Service    │    │  Database   │
│             │    │             │    │             │    │             │
│ 1.Get       │───►│ 2.GET       │───►│ 3.Fetch     │───►│ 4.Query     │
│   Menus     │    │  /menu      │    │   Menus     │    │ TrainingMenu│
│             │    │             │    │             │    │             │
│ 7.Display   │◄───│ 6.Return    │◄───│ 5.Format    │◄───│             │
│   Menu List │    │   JSON      │    │   Response  │    │             │
│             │    │             │    │             │    │             │
│ 8.Select    │───►│ 9.POST      │───►│10.Save      │───►│11.Insert    │
│   Menu &    │    │  /record    │    │   Record    │    │ TrainingRec │
│   Input Data│    │             │    │             │    │             │
│             │    │             │    │             │    │             │
│12.Confirm   │◄───│13.Success   │◄───│             │◄───│             │
│   Saved     │    │   Response  │    │             │    │             │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
```

## 5. セキュリティアーキテクチャ

### 5.1 認証・認可フロー
```
┌─────────────────────────────────────────────────────────────────┐
│                        Security Layer                          │
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐          │
│  │   HTTPS     │    │    JWT      │    │ SQL Injection│          │
│  │ Encryption  │    │ Authentication│ │  Protection  │          │
│  │             │    │             │    │             │          │
│  │ ・TLS 1.2+  │    │ ・Token     │    │ ・Entity    │          │
│  │ ・Certificate│    │   Validation│    │   Framework │          │
│  │   Validation│    │ ・Claims    │    │ ・Parameterized│        │
│  │             │    │   Based Auth│    │   Queries   │          │
│  └─────────────┘    └─────────────┘    └─────────────┘          │
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐          │
│  │   CORS      │    │ Input       │    │ Error       │          │
│  │ Configuration│   │ Validation  │    │ Handling    │          │
│  │             │    │             │    │             │          │
│  │ ・Origin    │    │ ・Data      │    │ ・Secure    │          │
│  │   Whitelist │    │   Validation│    │   Error     │          │
│  │ ・Method    │    │ ・Length    │    │   Messages  │          │
│  │   Restriction│   │   Limits    │    │ ・Logging   │          │
│  └─────────────┘    └─────────────┘    └─────────────┘          │
└─────────────────────────────────────────────────────────────────┘
```

## 6. デプロイメントアーキテクチャ

### 6.1 本番環境構成
```
┌─────────────────────────────────────────────────────────────────┐
│                        Production Environment                   │
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐          │
│  │   CDN       │    │ Load        │    │ Web         │          │
│  │ (Static     │    │ Balancer    │    │ Servers     │          │
│  │ Assets)     │    │             │    │             │          │
│  │             │    │ ・nginx     │    │ ・nginx     │          │
│  │ ・JS/CSS    │    │ ・SSL       │    │ ・Docker    │          │
│  │ ・Images    │    │   Termination│   │   Containers│          │
│  │ ・Fonts     │    │ ・Health    │    │ ・Auto      │          │
│  └─────────────┘    │   Check     │    │   Scaling   │          │
│                     └─────────────┘    └─────────────┘          │
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐          │
│  │ Application │    │ Database    │    │ Monitoring  │          │
│  │ Servers     │    │ Cluster     │    │ & Logging   │          │
│  │             │    │             │    │             │          │
│  │ ・API       │    │ ・Primary   │    │ ・Application│         │
│  │   Instances │    │   Database  │    │   Insights  │          │
│  │ ・Auto      │    │ ・Read      │    │ ・Log       │          │
│  │   Scaling   │    │   Replicas  │    │   Analytics │          │
│  │ ・Health    │    │ ・Backup    │    │ ・Alerts    │          │
│  │   Monitoring│    │   Strategy  │    │             │          │
│  └─────────────┘    └─────────────┘    └─────────────┘          │
└─────────────────────────────────────────────────────────────────┘
```

### 6.2 開発環境構成
```
┌─────────────────────────────────────────────────────────────────┐
│                      Development Environment                    │
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐          │
│  │ Local       │    │ Docker      │    │ Database    │          │
│  │ Development │    │ Compose     │    │ (Local)     │          │
│  │             │    │             │    │             │          │
│  │ ・Frontend  │    │ ・API       │    │ ・SQL Server│          │
│  │   Dev Server│    │   Container │    │   LocalDB   │          │
│  │ ・Mobile    │    │ ・Database  │    │ ・Test Data │          │
│  │   Simulator │    │   Container │    │ ・Migrations│          │
│  │ ・Hot Reload│    │ ・nginx     │    │             │          │
│  └─────────────┘    │   Container │    └─────────────┘          │
│                     └─────────────┘                             │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                     CI/CD Pipeline                      │   │
│  │                                                         │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │   │
│  │  │ Source   │  │ Build    │  │ Test     │  │ Deploy   │ │   │
│  │  │ Control  │  │          │  │          │  │          │ │   │
│  │  │          │  │ ・Compile│  │ ・Unit   │  │ ・Staging│ │   │
│  │  │ ・Git    │─►│ ・Package│─►│   Tests  │─►│ ・Production│ │
│  │  │ ・Branch │  │ ・Docker │  │ ・Integration│ │ ・Rollback│ │   │
│  │  │   Strategy│ │   Build  │  │   Tests  │  │   Strategy│ │   │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

このアーキテクチャ設計は、スケーラビリティ、セキュリティ、保守性を考慮して設計されており、
システムの成長に応じて各コンポーネントを独立してスケールできるように構成されています。