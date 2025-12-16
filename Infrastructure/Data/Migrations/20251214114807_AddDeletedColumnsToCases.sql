-- Migration: AddDeletedColumnsToCases
-- 新增 deleted、deleted_at、deleted_by 欄位到 Cases 表
-- 執行日期: 2025-12-14

BEGIN TRANSACTION;

-- 新增 deleted 欄位
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cases]') AND name = 'deleted')
BEGIN
    ALTER TABLE [dbo].[Cases]
    ADD [deleted] bit NULL DEFAULT 0;
    
    PRINT '已新增 deleted 欄位到 Cases 表';
END
ELSE
BEGIN
    PRINT 'deleted 欄位已存在，跳過';
END

-- 新增 deleted_at 欄位
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cases]') AND name = 'deleted_at')
BEGIN
    ALTER TABLE [dbo].[Cases]
    ADD [deleted_at] datetime2 NULL;
    
    PRINT '已新增 deleted_at 欄位到 Cases 表';
END
ELSE
BEGIN
    PRINT 'deleted_at 欄位已存在，跳過';
END

-- 新增 deleted_by 欄位
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cases]') AND name = 'deleted_by')
BEGIN
    ALTER TABLE [dbo].[Cases]
    ADD [deleted_by] nvarchar(30) NULL;
    
    PRINT '已新增 deleted_by 欄位到 Cases 表';
END
ELSE
BEGIN
    PRINT 'deleted_by 欄位已存在，跳過';
END

-- 更新現有記錄的 deleted 欄位為 false（如果為 NULL）
UPDATE [dbo].[Cases]
SET [deleted] = 0
WHERE [deleted] IS NULL;

COMMIT TRANSACTION;

PRINT 'Migration 執行完成！';
