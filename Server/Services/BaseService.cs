using System.Text.Encodings.Web;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Enums;
using Vanigam.CRM.Objects.Exceptions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Vanigam.CRM.Objects.Services;
using Vanigam.CRM.Objects.Attributes;

namespace Vanigam.CRM.Server.Services
{
    public abstract class BaseService<T> : IDisposable where T : class, ITenant, IHasId
    {
        public abstract DbSet<T> GetDbSet();
        public VanigamAccountingDbContext Context { get; private set; }
        public int? TenantId { get; private set; }
        public Guid? UserId { get; private set; }
        public LoginUserType? UserType { get; }

        public ILogger<BaseService<T>> Logger { get; }
        private bool IsNonTenantObject { get; set; }

        protected BaseService(VanigamAccountingDbContext context, ILogger<BaseService<T>> logger)
        {
            this.Context = context;
            Logger = logger;
            var currentUser = context.GetService<ICurrentUserService>();
            TenantId = currentUser.TenantId;
            UserType = currentUser.UserType;
            if (!string.IsNullOrEmpty(currentUser?.UserId))
            {
                UserId = Guid.Parse(currentUser.UserId);
            }
            var attrs = typeof(T).GetCustomAttributes(typeof(NonTenantObjectAttribute), true);
            IsNonTenantObject = attrs.Any();
        }

        public IQueryable<T> ApplyUserRoleFilter(IQueryable<T> items)
        {
            if (!IsNonTenantObject)
            {
                items = items.Where(i => i.TenantId == TenantId);
            }

            return items;
        }


        protected IQueryable<T> ApplyHeaderFilter(IQueryable<T> items, HttpRequest request, bool singleItemRequest)
        {
           
            return items;
        }
       
        public async Task<IQueryable<T>> GetAsync(Guid key, HttpRequest request = null)
        {
            IQueryable<T> items = GetDbSet();
            items = items.Where(i => i.Oid == key);
            items = ApplyHeaderFilter(items, request, true);
            items = ApplyUserRoleFilter(items);
            items = await OnGetAsync(key, items);
            return items;
        }

        public async Task<IQueryable<T>> GetAllAsync(HttpRequest request = null)
        {
            IQueryable<T> items = GetDbSet();
            items = ApplyHeaderFilter(items, request, false);
            items = ApplyUserRoleFilter(items);
            items = await OnGetAllAsync(items);
            return items;
        }

        public async Task<bool> DeleteAsync(Guid key, HttpRequest request = null)
        {
            var items = await GetAsync(key, request);
            var item = items.FirstOrDefault();
            if (item != null)
            {
                await DeleteAsync(item);
            }
            return true;
        }
        public async Task<bool> DeleteAsync(T item)
        {
            await OnDeletedAsync(item);
            GetDbSet().Remove(item);
            await Context.SaveChangesAsync();
            await OnAfterDeletedAsync(item);
            return true;
        }
        public async Task<IQueryable<T>> UpdateAsync(T item, bool skipAuditLog = false)
        {
            //ConvertDateTimesToUniversalTime(item);
            var isUnique = await IsUnique(item);
            if (isUnique != null && isUnique.Value)
            {
                await OnUpdatedAsync(item);
                GetDbSet().Update(item);
                await SaveContext(skipAuditLog);
                var itemToReturn = GetItemToReturn(item);
                await OnAfterUpdatedAsync(item);
                return itemToReturn;
            }
            else
            {
                throw new NotUniqueException(typeof(T), $"{typeof(T).Name} is not unique");
            }
        }
        public async Task<IQueryable<T>> CreateAsync(T item, bool skipAuditLog = false)
        {
            var isUnique = await IsUnique(item);
            if (isUnique != null && isUnique.Value)
            {
                if (!IsNonTenantObject && TenantId != null)
                {
                    item.TenantId = TenantId;
                }
                ConvertDateTimesToUniversalTime(item);
                await OnCreatedAsync(item);
                GetDbSet().Add(item);
                await SaveContext(skipAuditLog);
                var itemToReturn = GetItemToReturn(item);
                await OnAfterCreatedAsync(item);
                return itemToReturn;
            }
            else
            {
                throw new NotUniqueException(typeof(T), $"{typeof(T).Name} is not unique");
            }
        }

        public virtual IQueryable<T> GetItemToReturn(T item)
        {
            return GetDbSet().Where(i => i.Oid == item.Oid);
        }
        protected async Task SaveContext(bool skipAuditLog)
        {
            await Context.SaveChangesAsync();
            //if (skipAuditLog)
            //{
            //    await Context.SaveChangesAsyncWithoutAudit();
            //}
            //else
            //{
            //    await Context.SaveChangesAsync();
            //}
        }

        public virtual async Task<bool?> IsUnique(T item)
        {
            return true;
        }
        private void ConvertDateTimesToUniversalTime(T item)
        {
            var properties = item.GetType().GetProperties();
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(DateTimeOffset))
                {
                    prop.SetValue(item, ((DateTimeOffset)prop.GetValue(item)).ToUniversalTime());
                }
                else if (prop.PropertyType == typeof(DateTimeOffset?) && prop.GetValue(item) != null)
                {
                    prop.SetValue(item, ((DateTimeOffset?)prop.GetValue(item)).Value.ToUniversalTime());
                }
            }
        }
        public async Task<T> CreateAndGetAsync(T item)
        {
            var itemToReturn = await CreateAsync(item);
            return itemToReturn.FirstOrDefault();
        }
        public async Task<T> UpdateAndGetAsync(T item)
        {
            var itemToReturn = await UpdateAsync(item);
            return itemToReturn.FirstOrDefault();
        }

        protected virtual async Task OnReadAsync(IQueryable<T> items)
        {
        }

        protected virtual async Task<IQueryable<T>> OnGetAsync(Guid key, IQueryable<T> item)
        {
            return item;
        }
        protected virtual async Task<IQueryable<T>> OnGetAllAsync(IQueryable<T> item)
        {
            return item;
        }

        protected virtual async Task OnDeletedAsync(T item)
        {
        }

        protected virtual async Task OnAfterDeletedAsync(T item)
        {
        }

        protected virtual async Task OnCreatedAsync(T item)
        {
        }

        public virtual async Task OnAfterCreatedAsync(T item, bool save = true)
        {
        }

        protected virtual async Task OnUpdatedAsync(T item)
        {
        }

        protected virtual async Task OnAfterUpdatedAsync(T item)
        {
        }

        public IQueryable<T> GetFilteredQueryable(HttpRequest request)
        {
            var items = ApplyUserRoleFilter(GetDbSet());
            items = ApplyHeaderFilter(items, request, false);
            return items;
        }
        public void Dispose()
        {
            Context.Dispose();
        }
    }

}

