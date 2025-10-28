# 登入系統實作計劃

## 概述
本計劃包含前端登入畫面設計和後端 Azure AD 整合，建立完整的身份驗證系統。

## 系統名稱建議
**「CanLove 肯愛協會管理系統」** (CanLove Association Management System)

### 系統功能模組
- 👥 **個案管理** - 個案資料建立、追蹤、評估
- ⏰ **考勤管理** - 員工打卡、請假申請、出勤統計
- 📊 **報表系統** - 考勤報表、會計報表、個案統計
- 📦 **物資管理** - 物資進出、庫存管理、採購記錄
- 👤 **人事管理** - 員工資料、角色權限、組織架構
- 📱 **Line 整合** - 打卡功能、通知推播、快速查詢

---

## 前端計劃

### 1. 登入畫面設計

#### 1.1 視覺設計
- **色彩主題**：淺綠色系
  - 主色調：`#E8F5E8` (淺綠背景)
  - 強調色：`#4CAF50` (綠色按鈕)
  - 文字色：`#2E7D32` (深綠文字)
  - 邊框色：`#C8E6C9` (淺綠邊框)

#### 1.2 頁面元素
- **標題區域**
  - 系統名稱：「CanLove 協會管理系統」
  - 副標題：「登入以存取協會管理功能」
  
- **登入卡片**
  - 圓角設計 (border-radius: 12px)
  - 陰影效果 (box-shadow)
  - 白色背景，淺綠邊框
  
- **登入按鈕**
  - Azure AD 登入按鈕
  - 綠色背景，白色文字
  - 懸停效果 (hover)
  - Microsoft 圖示

#### 1.3 響應式設計
- 桌面版：居中卡片設計
- 平板版：適當間距調整
- 手機版：全寬度設計

### 2. CSS 檔案結構

```
wwwroot/css/
├── login.css          # 登入頁面專用樣式
├── login-responsive.css # 響應式樣式
└── login-animations.css # 動畫效果
```

### 3. 頁面檔案

```
Views/Account/
├── Login.cshtml       # 登入頁面
└── _LoginLayout.cshtml # 登入頁面專用 Layout
```

---

## 後端計劃

### 1. 資料庫設計

#### 1.1 使用者資料表 (Users)

```sql
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AzureObjectId NVARCHAR(450) NOT NULL UNIQUE,  -- Azure AD Object ID
    Email NVARCHAR(256) NOT NULL,                 -- 電子郵件
    DisplayName NVARCHAR(256) NOT NULL,           -- 顯示名稱
    GivenName NVARCHAR(100),                      -- 名字
    Surname NVARCHAR(100),                        -- 姓氏
    JobTitle NVARCHAR(100),                       -- 職稱
    Department NVARCHAR(100),                     -- 部門
    OfficeLocation NVARCHAR(100),                 -- 辦公室位置
    UserPrincipalName NVARCHAR(256),              -- UPN
    PreferredLanguage NVARCHAR(10),               -- 偏好語言
    IsActive BIT NOT NULL DEFAULT 1,              -- 是否啟用
    FirstLoginAt DATETIME2,                       -- 首次登入時間
    LastLoginAt DATETIME2,                        -- 最後登入時間
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedBy NVARCHAR(256)
);
```

#### 1.2 使用者角色關聯表 (UserRoles)

```sql
CREATE TABLE UserRoles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RoleName NVARCHAR(50) NOT NULL,               -- admin, socialworker, assistant, viewer
    AssignedBy NVARCHAR(256),                     -- 指派者
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

#### 1.3 登入記錄表 (UserLoginLogs)

```sql
CREATE TABLE UserLoginLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    LoginAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LoginIP NVARCHAR(45),                         -- IP 位址
    UserAgent NVARCHAR(500),                      -- 瀏覽器資訊
    LoginMethod NVARCHAR(50) NOT NULL,            -- Azure, Line, etc.
    IsSuccessful BIT NOT NULL DEFAULT 1,
    FailureReason NVARCHAR(500),                  -- 失敗原因
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

#### 1.4 使用者 Line 綁定表 (UserLineBindings)

```sql
CREATE TABLE UserLineBindings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    LineUserId NVARCHAR(100) NOT NULL UNIQUE,     -- Line 使用者 ID
    LineDisplayName NVARCHAR(256),                -- Line 顯示名稱
    BoundAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    LastUsedAt DATETIME2,                         -- 最後使用時間
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

#### 1.5 打卡記錄表 (AttendanceRecords)

```sql
CREATE TABLE AttendanceRecords (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ClockInAt DATETIME2 NOT NULL,                 -- 打卡時間
    ClockOutAt DATETIME2,                         -- 下班打卡時間
    ClockInLocation NVARCHAR(200),                -- 打卡地點
    ClockOutLocation NVARCHAR(200),               -- 下班打卡地點
    ClockInMethod NVARCHAR(50) NOT NULL,          -- 打卡方式 (Web, Line)
    ClockOutMethod NVARCHAR(50),                  -- 下班打卡方式
    WorkHours DECIMAL(4,2),                       -- 工作時數
    IsLate BIT DEFAULT 0,                         -- 是否遲到
    IsEarlyLeave BIT DEFAULT 0,                   -- 是否早退
    Remarks NVARCHAR(500),                        -- 備註
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

### 2. 資料模型

#### 2.1 User 模型

```csharp
public class User
{
    public int Id { get; set; }
    public string AzureObjectId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? GivenName { get; set; }
    public string? Surname { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? OfficeLocation { get; set; }
    public string? UserPrincipalName { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? FirstLoginAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // 導航屬性
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserLoginLog> LoginLogs { get; set; } = new List<UserLoginLog>();
}
```

#### 2.2 UserRole 模型

```csharp
public class UserRole
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // 導航屬性
    public virtual User User { get; set; } = null!;
}
```

### 3. 服務層設計

#### 3.1 UserService

```csharp
public interface IUserService
{
    Task<User?> GetUserByAzureObjectIdAsync(string azureObjectId);
    Task<User> CreateUserFromAzureClaimsAsync(ClaimsPrincipal principal);
    Task<User> UpdateUserFromAzureClaimsAsync(User user, ClaimsPrincipal principal);
    Task<bool> AssignRoleToUserAsync(int userId, string roleName, string assignedBy);
    Task<List<string>> GetUserRolesAsync(int userId);
    Task LogUserLoginAsync(int userId, string loginMethod, string? ipAddress, string? userAgent);
}
```

#### 3.2 認證中介軟體

```csharp
public class UserSyncMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IUserService _userService;

    public UserSyncMiddleware(RequestDelegate next, IUserService userService)
    {
        _next = next;
        _userService = userService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var azureObjectId = context.User.GetObjectId();
            if (!string.IsNullOrEmpty(azureObjectId))
            {
                var user = await _userService.GetUserByAzureObjectIdAsync(azureObjectId);
                if (user == null)
                {
                    // 建立新使用者
                    user = await _userService.CreateUserFromAzureClaimsAsync(context.User);
                }
                else
                {
                    // 更新使用者資料
                    user = await _userService.UpdateUserFromAzureClaimsAsync(user, context.User);
                }
                
                // 記錄登入
                await _userService.LogUserLoginAsync(
                    user.Id, 
                    "Azure", 
                    context.Connection.RemoteIpAddress?.ToString(),
                    context.Request.Headers.UserAgent.ToString()
                );
            }
        }
        
        await _next(context);
    }
}
```

### 4. 控制器更新

#### 4.1 AccountController

```csharp
public class AccountController : Controller
{
    private readonly IUserService _userService;

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SignIn()
    {
        // 重導向到 Azure AD 登入
        return Challenge(new AuthenticationProperties 
        { 
            RedirectUri = Url.Action("SignInCallback", "Account") 
        });
    }

    [HttpGet]
    public async Task<IActionResult> SignInCallback()
    {
        // 登入成功後的重導向處理
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }
}
```

---

## 實作步驟

### 階段一：資料庫準備
1. [ ] 建立 Users 資料表
2. [ ] 建立 UserRoles 資料表  
3. [ ] 建立 UserLoginLogs 資料表
4. [ ] 建立 Entity Framework 模型
5. [ ] 建立並執行資料庫遷移

### 階段二：後端服務
1. [ ] 實作 UserService
2. [ ] 建立 UserSyncMiddleware
3. [ ] 更新 AccountController
4. [ ] 啟用認證保護 (取消註解 [Authorize] 屬性)
5. [ ] 測試 Azure AD 登入流程

### 階段三：前端設計
1. [ ] 建立登入頁面 HTML
2. [ ] 設計淺綠色系 CSS
3. [ ] 實作響應式設計
4. [ ] 加入動畫效果
5. [ ] 測試各種裝置顯示

### 階段四：整合測試
1. [ ] 測試完整登入流程
2. [ ] 測試使用者資料同步
3. [ ] 測試角色權限控制
4. [ ] 測試登出功能
5. [ ] 效能測試

---

## 未來擴展

### Line 打卡功能整合
- **Line Bot 開發**：建立 Line 機器人處理打卡指令
- **使用者綁定**：透過 Azure ObjectId 綁定 Line 使用者 ID
- **打卡記錄**：Line 打卡資料同步到系統資料庫
- **地理位置驗證**：Line 打卡時驗證位置資訊
- **快速查詢**：透過 Line 查詢個人考勤記錄

### 考勤管理功能
- **打卡系統**：支援 Line 打卡和網頁打卡
- **請假管理**：請假申請、審核、統計
- **出勤報表**：個人/部門考勤統計
- **異常處理**：遲到、早退、缺勤記錄

---

## 技術規格

### 前端技術
- HTML5 + CSS3
- Bootstrap 5 (響應式框架)
- Font Awesome (圖示)
- 自定義淺綠色主題

### 後端技術
- ASP.NET Core 9.0
- Entity Framework Core
- Microsoft Identity Web
- Azure AD B2C
- Line Messaging API (未來)

### 資料庫
- SQL Server
- 索引優化
- 資料備份策略

---

## 注意事項

1. **安全性**
   - 所有敏感資料加密儲存
   - 定期更新認證金鑰
   - 實作登入失敗鎖定機制

2. **效能**
   - 使用者資料快取
   - 資料庫查詢優化
   - 非同步處理

3. **維護性**
   - 完整的錯誤處理
   - 詳細的日誌記錄
   - 單元測試覆蓋

---

*最後更新：2024年12月*
