using System;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class UserAccountRepository : IUserAccountRepository
{
    #region Dependencies

    private readonly IDbContextFactory<HauloryDbContext> _factory;

    #endregion

    #region Constructor

    public UserAccountRepository(IDbContextFactory<HauloryDbContext> factory)
    {
        _factory = factory;
    }

    #endregion

    #region Existence

    public async Task<bool> AnyAsync()
    {
        using var db = _factory.CreateDbContext();
        return await db.UserAccounts.AnyAsync();
    }

    #endregion

    #region Commands

    public async Task AddAsync(UserAccount user)
    {
        using var db = _factory.CreateDbContext();

        // Normalize email for uniqueness
        var email = user.Email.Trim().ToLowerInvariant();

        // Prevent duplicate accounts by email
        var exists = await db.UserAccounts.AnyAsync(u => u.Email == email);
        if (exists)
            return;

        db.UserAccounts.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserAccount user)
    {
        using var db = _factory.CreateDbContext();

        // Prevent changing email to one that is already taken
        var conflict = await db.UserAccounts.AnyAsync(u =>
            u.Id != user.Id &&
            u.Email == user.Email);

        if (conflict)
            throw new InvalidOperationException("That email is already in use.");

        db.UserAccounts.Update(user);
        await db.SaveChangesAsync();
    }

    #endregion

    #region Queries

    public async Task<UserAccount?> GetByIdAsync(Guid id)
    {
        using var db = _factory.CreateDbContext();

        return await db.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserAccount?> GetByEmailAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalized = email.Trim().ToLowerInvariant();

        using var db = _factory.CreateDbContext();

        return await db.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized);
    }

    public async Task<IReadOnlyList<UserAccount>> GetSubUsersAsync(Guid mainUserId)
    {
        using var db = _factory.CreateDbContext();

        return await db.UserAccounts
            .AsNoTracking()
            .Where(u => u.ParentMainUserId == mainUserId)
            .ToListAsync();
    }

    #endregion
}
