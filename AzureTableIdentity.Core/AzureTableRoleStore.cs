using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTableIdentity
{
    public class AzureTableRoleStore<TRole> : IRoleStore<TRole>
        where TRole : AzureTableRole, new()
    {
        public CloudTable AuthCloudTable { get; }
        public AzureTableRoleStore(CloudTable authCloudTable)
        {
            this.AuthCloudTable = authCloudTable;
        }

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            cancellationToken.ThrowIfCancellationRequested();

            TableOperation executeOperation = TableOperation.Insert(role);

            try
            {
                await AuthCloudTable.ExecuteAsync(executeOperation, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            cancellationToken.ThrowIfCancellationRequested();

            TableOperation executeOperation = TableOperation.Delete(role);
            try
            {
                TableResult result = await AuthCloudTable.ExecuteAsync(executeOperation, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed deleting user {role.RowKey}", ex);
            }

            return IdentityResult.Success;

        }

        public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return this.FindByNameAsync(roleId, cancellationToken);
        }

        public async Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TRole fetchedRole = null;
            if (string.IsNullOrEmpty(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                IQueryable<TRole> query = AuthCloudTable.CreateQuery<TRole>()
                            .Where(x => x.PartitionKey == "Users" && x.RowKey == normalizedRoleName);
                fetchedRole = await query.FirstAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return fetchedRole;
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            return GetNormalizedRoleNameAsync(role, cancellationToken);
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (normalizedName == null) throw new ArgumentNullException(nameof(normalizedName));

            role.NormalizedName = normalizedName;
            return Task.FromResult<object>(null);
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));

            role.Name = roleName;
            return Task.FromResult<object>(null);
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null) { throw new ArgumentNullException(nameof(role)); }

            cancellationToken.ThrowIfCancellationRequested();

            TableOperation executeOperation = TableOperation.Replace(role);

            try
            {
                await AuthCloudTable.ExecuteAsync(executeOperation, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return IdentityResult.Success;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AzureTableRoleStore()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
