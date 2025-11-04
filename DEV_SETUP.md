# 開發環境設置指南

## 即時重新載入設置

此專案已配置支援即時重新載入（Hot Reload），修改後端代碼或前端 Tailwind CSS 樣式時會自動反映變更。

## 啟動開發環境

### 方法 1：使用兩個終端視窗（推薦）

#### 終端 1：啟動 ASP.NET Core 後端（啟用 Hot Reload）
```bash
dotnet watch run
```

或者使用 Visual Studio / VS Code：
- 直接按 F5 或點擊「執行」按鈕
- .NET 會自動啟用 Hot Reload

#### 終端 2：啟動 Tailwind CSS 監聽模式
```bash
npm run dev:css
```

### 方法 2：使用單一終端（同時運行）

在專案根目錄執行：
```bash
# macOS / Linux
npm run dev:css & dotnet watch run

# 或者使用 concurrently（需要安裝：npm install -g concurrently）
concurrently "npm run dev:css" "dotnet watch run"
```

## 工作原理

### 後端 Hot Reload
- 修改 `.cs` 文件（Controllers、Services、Models 等）時會自動重新載入
- 修改 `.cshtml` 視圖文件時會自動重新載入
- 瀏覽器會自動刷新（如果支援）

### 前端 Tailwind CSS
- 修改 `wwwroot/css/tailwind.css` 或任何 `.cshtml` 視圖文件時
- Tailwind 會自動重新編譯 CSS
- 瀏覽器會看到即時的樣式變更

## 注意事項

1. **首次運行前**：確保已安裝 Node.js 依賴
   ```bash
   npm install
   ```

2. **瀏覽器刷新**：
   - 大部分情況下瀏覽器會自動刷新
   - 如果沒有，手動按 `F5` 或 `Cmd+R` / `Ctrl+R`

3. **某些變更需要重啟**：
   - 修改 `Program.cs`、`appsettings.json` 等配置檔案
   - 修改 `.csproj` 檔案
   - 添加新的 NuGet 套件
   - 這些情況下需要完全重啟應用程式

4. **Tailwind CSS 變更檢測**：
   - 確保 Tailwind 監聽模式正在運行
   - 檢查終端是否有編譯錯誤訊息

## 故障排除

### 後端 Hot Reload 不工作
- 確認 `CanLove_Backend.csproj` 中有 `<HotReloadEnabled>true</HotReloadEnabled>`
- 使用 `dotnet watch run` 而不是 `dotnet run`

### Tailwind CSS 不更新
- 確認 `npm run dev:css` 正在運行
- 檢查 `wwwroot/css/theme.css` 檔案是否有更新時間戳
- 確認瀏覽器有正確載入 `theme.css`（檢查網路面板）

### 瀏覽器不自動刷新
- 檢查瀏覽器開發者工具的控制台
- 某些瀏覽器擴充功能可能阻止自動刷新
- 手動刷新瀏覽器通常可以解決

