# 從 SQL 帳密切換到 Azure Managed Identity（零密碼）

本文件說明如何把目前存在 Key Vault 的含帳密連線字串，切換為使用 Azure Managed Identity（MI）+ Entra ID 驗證。

## 現況
- Secret：`ConnectionStrings--DefaultConnection`
- 值：使用 `sqladmin_andy@canlove.org.tw + 密碼` 的傳統 SQL 連線字串
- 程式：`Program.cs` 已透過 Key Vault 載入，呼叫 `GetConnectionString("DefaultConnection")`

## 目標
- 使用 Web App 的 Managed Identity 連線 Azure SQL，不在任何地方保存密碼。

## 步驟
1. 啟用 Web App 系統指派身分：Web App → Identity → System assigned → On。
2. 授權 Web App 讀 Key Vault：
   - 存取原則（Secrets: Get、List），或在 IAM 指派 `Key Vault Secrets User`。
   - Web App 設定：`KeyVault:VaultUri=https://<vault>.vault.azure.net/`；建議 `WEBSITE_RUN_FROM_PACKAGE=1`、`ASPNETCORE_ENVIRONMENT=Production`。
3. 設定 SQL Server 的 Entra Admin：SQL server → Microsoft Entra ID → Set admin。
4. 在「目標資料庫」建立 AAD 使用者（以 Web App 服務主體顯示名稱）：
```
CREATE USER [<WebApp 服務主體顯示名稱>] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [<WebApp 服務主體顯示名稱>];
ALTER ROLE db_datawriter  ADD MEMBER [<WebApp 服務主體顯示名稱>];
-- 如需遷移/建表（臨時）
-- ALTER ROLE db_ddladmin  ADD MEMBER [<WebApp 服務主體顯示名稱>];
```
5. 將 Key Vault Secret 改為 MI 版本（名稱不變）：
```
Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<db>;
Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
Authentication=Active Directory Managed Identity;
```
6. 重啟 Web App，驗證 `/Case`，成功後刪除含密碼的舊 Secret 版本。

## 排查
- `principal not found`：請用企業應用程式「顯示名稱」，不要用 Object ID/Client ID；確認在目標資料庫執行，SQL 已設 Entra Admin。
- 仍 500：Web App 對 Key Vault 無 Secrets 讀權；Secret 名稱必須是 `ConnectionStrings--DefaultConnection`；SQL 網路/防火牆；資料表未建立（臨時授 `db_ddladmin`）。

## 備註
- 若使用使用者指派 MI（UAMI），於 Web App 另設 `ManagedIdentityClientId=<UAMI Client ID>`，連線字串同上。
