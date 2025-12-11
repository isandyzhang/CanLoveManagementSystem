namespace CanLove_Backend.Domain.Case.Exceptions;

/// <summary>
/// 個案基本資料相關例外基類
/// </summary>
public class CaseBasicException : Exception
{
    public CaseBasicException(string message) : base(message)
    {
    }

    public CaseBasicException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// 個案不存在例外
/// </summary>
public class CaseBasicNotFoundException : CaseBasicException
{
    public CaseBasicNotFoundException(string caseId) 
        : base($"找不到指定的個案：{caseId}")
    {
        CaseId = caseId;
    }

    public string CaseId { get; }
}

/// <summary>
/// 個案基本資料儲存失敗例外
/// </summary>
public class CaseBasicSaveException : CaseBasicException
{
    public CaseBasicSaveException(string operation, Exception innerException) 
        : base($"執行個案操作 {operation} 時發生錯誤", innerException)
    {
        Operation = operation;
    }

    public string Operation { get; }
}

/// <summary>
/// 個案基本資料驗證失敗例外
/// </summary>
public class CaseBasicValidationException : CaseBasicException
{
    public CaseBasicValidationException(string message) 
        : base(message)
    {
    }

    public CaseBasicValidationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

/// <summary>
/// 個案狀態不允許操作例外
/// </summary>
public class CaseBasicInvalidStatusException : CaseBasicException
{
    public CaseBasicInvalidStatusException(string caseId, string currentStatus, string requiredStatus) 
        : base($"個案狀態不允許此操作。目前狀態：{currentStatus}，需要狀態：{requiredStatus}")
    {
        CaseId = caseId;
        CurrentStatus = currentStatus;
        RequiredStatus = requiredStatus;
    }

    public string CaseId { get; }
    public string CurrentStatus { get; }
    public string RequiredStatus { get; }
}

/// <summary>
/// 個案被鎖定例外
/// </summary>
public class CaseBasicLockedException : CaseBasicException
{
    public CaseBasicLockedException(string caseId, string lockedBy) 
        : base($"個案已被其他使用者鎖定，無法編輯。鎖定者：{lockedBy}")
    {
        CaseId = caseId;
        LockedBy = lockedBy;
    }

    public string CaseId { get; }
    public string LockedBy { get; }
}
