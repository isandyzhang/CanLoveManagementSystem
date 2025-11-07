using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Review;

namespace CanLove_Backend.Services.Shared;

public interface IReviewHandler
{
    Task HandleApproveAsync(string caseId, string targetId);
    Task HandleRejectAsync(string caseId, string targetId);
}

public class CaseBasicReviewHandler : IReviewHandler
{
    private readonly CanLoveDbContext _context;

    public CaseBasicReviewHandler(CanLoveDbContext context)
    {
        _context = context;
    }

    public async Task HandleApproveAsync(string caseId, string targetId)
    {
        var entity = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseId);
        if (entity == null) return;
        entity.Status = "Approved";
        entity.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task HandleRejectAsync(string caseId, string targetId)
    {
        var entity = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseId);
        if (entity == null) return;
        entity.Status = "Rejected";
        entity.SubmittedAt = null;
        await _context.SaveChangesAsync();
    }
}

public class ReviewService
{
    private readonly CanLoveDbContext _context;
    private readonly CaseBasicReviewHandler _caseBasicHandler;
    private readonly CaseOpeningReviewHandler _caseOpeningHandler;

    public ReviewService(CanLoveDbContext context, CaseBasicReviewHandler caseBasicHandler, CaseOpeningReviewHandler caseOpeningHandler)
    {
        _context = context;
        _caseBasicHandler = caseBasicHandler;
        _caseOpeningHandler = caseOpeningHandler;
    }

    public async Task<bool> DecideAsync(int reviewId, bool approved, string? reviewer, string? comment)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        var item = await _context.Set<CaseReviewItem>().FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        if (item == null) return false;

        item.ReviewedBy = reviewer;
        item.ReviewedAt = DateTime.UtcNow;
        item.ReviewComment = comment;
        item.Status = approved ? "Approved" : "Rejected";
        item.UpdatedAt = DateTime.UtcNow;
        _context.Update(item);
        await _context.SaveChangesAsync();

        // 依 Type 呼叫對應 handler（目前支援 CaseBasic, CaseOpening）
        if (string.Equals(item.Type, "CaseBasic", StringComparison.OrdinalIgnoreCase))
        {
            if (approved)
            {
                await _caseBasicHandler.HandleApproveAsync(item.CaseId, item.TargetId);
            }
            else
            {
                await _caseBasicHandler.HandleRejectAsync(item.CaseId, item.TargetId);
            }
        }
        else if (string.Equals(item.Type, "CaseOpening", StringComparison.OrdinalIgnoreCase))
        {
            if (approved)
            {
                await _caseOpeningHandler.HandleApproveAsync(item.CaseId, item.TargetId);
            }
            else
            {
                await _caseOpeningHandler.HandleRejectAsync(item.CaseId, item.TargetId);
            }
        }

        await tx.CommitAsync();
        return true;
    }
}

public class CaseOpeningReviewHandler : IReviewHandler
{
    private readonly CanLoveDbContext _context;

    public CaseOpeningReviewHandler(CanLoveDbContext context)
    {
        _context = context;
    }

    public async Task HandleApproveAsync(string caseId, string targetId)
    {
        // targetId 可對應 opening_id 或 caseId，先以 caseId 尋找
        var entity = await _context.CaseOpenings.FirstOrDefaultAsync(o => o.CaseId == caseId);
        if (entity == null) return;
        entity.Status = "Approved";
        entity.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task HandleRejectAsync(string caseId, string targetId)
    {
        var entity = await _context.CaseOpenings.FirstOrDefaultAsync(o => o.CaseId == caseId);
        if (entity == null) return;
        entity.Status = "Rejected";
        entity.SubmittedAt = null;
        await _context.SaveChangesAsync();
    }
}


