using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace ApiCf.EntityFrameworkCore
{
    public static class ApiCfDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<ApiCfDbContext> builder, string connectionString)
        {
            builder.UseOracle(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<ApiCfDbContext> builder, DbConnection connection)
        {
            builder.UseOracle(connection);
        }
    }
}




