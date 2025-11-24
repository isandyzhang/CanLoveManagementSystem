namespace CanLove_Backend.Extensions
{
    /// <summary>
    /// 權限常數類別
    /// 用於定義系統中的各種權限角色和權限名稱
    /// 未來實作權限系統時，可在 Controller 或 Action 上使用 [Authorize(Roles = PermissionConstants.Role.Admin)]
    /// </summary>
    public static class PermissionConstants
    {
        /// <summary>
        /// 角色常數
        /// </summary>
        public static class Role
        {
            // 預留未來角色定義
            // 範例：
            // public const string Admin = "Admin";
            // public const string Manager = "Manager";
            // public const string Staff = "Staff";
            // public const string Viewer = "Viewer";
        }

        /// <summary>
        /// 權限名稱常數
        /// </summary>
        public static class Permission
        {
            // 預留未來權限定義
            // 範例：
            // public const string CaseCreate = "Case.Create";
            // public const string CaseEdit = "Case.Edit";
            // public const string CaseDelete = "Case.Delete";
            // public const string CaseView = "Case.View";
        }

        /// <summary>
        /// 政策名稱常數
        /// </summary>
        public static class Policy
        {
            // 預留未來政策定義
            // 範例：
            // public const string CaseManagement = "CaseManagement";
            // public const string StaffManagement = "StaffManagement";
        }
    }
}

