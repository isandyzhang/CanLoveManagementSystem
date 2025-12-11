namespace CanLove_Backend.Domain.Case.Exceptions;

/// <summary>
/// 開案記錄相關例外基類
/// </summary>
public class CaseOpeningException : Exception
{
    public CaseOpeningException(string message) : base(message)
    {
    }

    public CaseOpeningException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// 開案記錄不存在例外
/// </summary>
public class CaseOpeningNotFoundException : CaseOpeningException
{
    public CaseOpeningNotFoundException(string caseId) 
        : base($"找不到指定的開案記錄：{caseId}")
    {
        CaseId = caseId;
    }

    public string CaseId { get; }
}

/// <summary>
/// 個案不存在例外
/// </summary>
public class CaseNotFoundException : CaseOpeningException
{
    public CaseNotFoundException(string caseId) 
        : base($"找不到指定的個案：{caseId}")
    {
        CaseId = caseId;
    }

    public string CaseId { get; }
}

/// <summary>
/// 開案記錄狀態不允許操作例外
/// </summary>
public class CaseOpeningInvalidStatusException : CaseOpeningException
{
    public CaseOpeningInvalidStatusException(string caseId, string currentStatus, string requiredStatus) 
        : base($"開案記錄狀態不允許此操作。目前狀態：{currentStatus}，需要狀態：{requiredStatus}")
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
/// 資料儲存失敗例外
/// </summary>
public class CaseOpeningSaveException : CaseOpeningException
{
    public CaseOpeningSaveException(string step, Exception innerException) 
        : base($"儲存開案記錄步驟 {step} 資料時發生錯誤", innerException)
    {
        Step = step;
    }

    public string Step { get; }
}
