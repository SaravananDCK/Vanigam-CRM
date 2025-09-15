using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;

namespace Vanigam.CRM.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public abstract class BaseApiController<TEntity, TKey> : ControllerBase
    where TEntity : class
    where TKey : IEquatable<TKey>
{
    protected readonly ApplicationDbContext Context;
    protected readonly ILogger Logger;

    protected BaseApiController(ApplicationDbContext context, ILogger logger)
    {
        Context = context;
        Logger = logger;
    }

    protected abstract DbSet<TEntity> DbSet { get; }
    protected abstract Expression<Func<TEntity, TKey>> KeySelector { get; }

    protected virtual TKey GetKeyValue(TEntity entity)
    {
        return KeySelector.Compile()(entity);
    }
    protected abstract string EntityName { get; }

    protected virtual IQueryable<TEntity> GetQueryWithIncludes()
    {
        return DbSet.AsQueryable();
    }

    protected virtual async Task<IQueryable<TEntity>> GetQueryWithIncludesAsync()
    {
        return await Task.FromResult(GetQueryWithIncludes());
    }

    protected virtual async Task OnBeforeCreateAsync(TEntity entity) { await Task.CompletedTask; }
    protected virtual async Task OnAfterCreateAsync(TEntity entity) { await Task.CompletedTask; }
    protected virtual async Task OnBeforeUpdateAsync(TEntity entity, TEntity update) { await Task.CompletedTask; }
    protected virtual async Task OnAfterUpdateAsync(TEntity entity) { await Task.CompletedTask; }
    protected virtual async Task OnBeforeDeleteAsync(TEntity entity) { await Task.CompletedTask; }
    protected virtual async Task OnAfterDeleteAsync(TEntity entity) { await Task.CompletedTask; }

    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public virtual ActionResult<IQueryable<TEntity>> Get()
    {
        try
        {
            var query = GetQueryWithIncludes();
            return Ok(query);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving {EntityName} entities", EntityName);
            return BadRequest("An error occurred while retrieving data");
        }
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<TEntity>> GetAsync(TKey key)
    {
        try
        {
            var keyExpression = KeySelector;
            var parameter = keyExpression.Parameters[0];
            var property = keyExpression.Body;
            var equalExpression = Expression.Equal(property, Expression.Constant(key, typeof(TKey)));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);

            var query = GetQueryWithIncludes();
            var entity = await query.FirstOrDefaultAsync(lambda);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving {EntityName} with key {Key}", EntityName, key);
            return BadRequest("An error occurred while retrieving the entity");
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public virtual async Task<ActionResult<TEntity>> PostAsync(TEntity entity)
    {
        try
        {
            var keyValue = KeySelector.Compile()(entity);
            var existing = await DbSet.FindAsync(keyValue);
            if (existing != null)
            {
                return Conflict();
            }

            await OnBeforeCreateAsync(entity);
            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync();
            await OnAfterCreateAsync(entity);

            return Created($"/{EntityName.ToLower()}/{keyValue}", entity);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating {EntityName}", EntityName);
            return BadRequest("An error occurred while creating the entity");
        }
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<TEntity>> PutAsync(TKey key, TEntity update)
    {
        try
        {
            var existing = await DbSet.FindAsync(key);
            if (existing == null)
            {
                return NotFound();
            }

            await OnBeforeUpdateAsync(existing, update);
            Context.Entry(existing).CurrentValues.SetValues(update);
            await Context.SaveChangesAsync();
            await OnAfterUpdateAsync(existing);

            return Ok(existing);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating {EntityName} with key {Key}", EntityName, key);
            return BadRequest("An error occurred while updating the entity");
        }
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<TEntity>> PatchAsync(TKey key, Delta<TEntity> delta)
    {
        try
        {
            var entity = await DbSet.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            delta.Patch(entity);
            await Context.SaveChangesAsync();

            return Ok(entity);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error patching {EntityName} with key {Key}", EntityName, key);
            return BadRequest("An error occurred while updating the entity");
        }
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> DeleteAsync(TKey key)
    {
        try
        {
            var entity = await DbSet.FindAsync(key);
            if (entity != null)
            {
                await OnBeforeDeleteAsync(entity);
                DbSet.Remove(entity);
                await Context.SaveChangesAsync();
                await OnAfterDeleteAsync(entity);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting {EntityName} with key {Key}", EntityName, key);
            return BadRequest("An error occurred while deleting the entity");
        }
    }
}