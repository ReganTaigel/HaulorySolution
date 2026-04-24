using System;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public class UserAccountRepository : IUserAccountRepository
{
    #region Dependencies

    private readonly HaulitCoreDbContext _db;

    #endregion

    #region Constructor

    public UserAccountRepository(HaulitCoreDbContext db)
    {
        _db = db;
    }


    #endregion

    #region Existence

    public async Task<bool> AnyAsync()
    {
        return await _db.UserAccounts.AnyAsync();
    }


    #endregion

    #region Commands

    public async Task AddAsync(UserAccount user)
    {
        var email = user.Email.Trim().ToLowerInvariant();

        var exists = await _db.UserAccounts.AnyAsync(u => u.Email == email);
        if (exists)
            return;

        _db.UserAccounts.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserAccount user)
    {
        var conflict = await _db.UserAccounts.AnyAsync(u =>
            u.Id != user.Id &&
            u.Email == user.Email);

        if (conflict)
            throw new InvalidOperationException("That email is already in use.");

        _db.UserAccounts.Update(user);
        await _db.SaveChangesAsync();
    }
    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
    #endregion

    #region Queries

    public async Task<UserAccount?> GetByIdAsync(Guid id)
    {
        return await _db.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserAccount?> GetByEmailAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalized = email.Trim().ToLowerInvariant();

        return await _db.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized);
    }

    public async Task<IReadOnlyList<UserAccount>> GetSubUsersAsync(Guid mainUserId)
    {
        return await _db.UserAccounts
            .AsNoTracking()
            .Where(u => u.ParentMainUserId == mainUserId)
            .ToListAsync();
    }

    #endregion
}
