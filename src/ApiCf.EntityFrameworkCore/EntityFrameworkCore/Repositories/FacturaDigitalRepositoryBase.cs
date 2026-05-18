using Abp.Data;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace ApiCf.EntityFrameworkCore.Repositories
{
    /// <summary>
    /// Base class for custom repositories of the application.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
    public abstract class ApiCfRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepositoryBase<ApiCfDbContext, TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {

        private readonly IActiveTransactionProvider _activeTransactionProvider;
        private readonly IDbContextProvider<ApiCfDbContext> _dbContextProvider;

        protected ApiCfRepositoryBase(IDbContextProvider<ApiCfDbContext> dbContextProvider, IActiveTransactionProvider activeTransactionProvider = null)
            : base(dbContextProvider)
        {
            _activeTransactionProvider = activeTransactionProvider;
            _dbContextProvider = dbContextProvider;
        }

        // Add your common methods for all repositories

        public DbCommand CreateCommand(string commandText, CommandType commandType, params OracleParameter[] parameters)
        {
            var dbContext = _dbContextProvider.GetDbContext();

            var command = dbContext.Database.GetDbConnection().CreateCommand();

            command.CommandText = commandText;
            command.CommandType = commandType;
            command.Transaction = GetActiveTransaction();

            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            return command;
        }



        public async Task EnsureConnectionOpenAsync()
        {
            var dbContext = _dbContextProvider.GetDbContext();
            var connection = dbContext.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
        }

        public DbTransaction GetActiveTransaction()
        {
            return (DbTransaction)_activeTransactionProvider.GetActiveTransaction(new ActiveTransactionProviderArgs {
                   {"ContextType", typeof(ApiCfDbContext) }, {"MultiTenancySide", MultiTenancySide }});
        }
    }

    /// <summary>
    /// Base class for custom repositories of the application.
    /// This is a shortcut of <see cref="ApiCfRepositoryBase{TEntity,TPrimaryKey}"/> for <see cref="int"/> primary key.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public abstract class ApiCfRepositoryBase<TEntity> : ApiCfRepositoryBase<TEntity, int>, IRepository<TEntity>
        where TEntity : class, IEntity<int>
    {
        protected ApiCfRepositoryBase(IDbContextProvider<ApiCfDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        // Do not add any method here, add to the class above (since this inherits it)!!!
    }
}




