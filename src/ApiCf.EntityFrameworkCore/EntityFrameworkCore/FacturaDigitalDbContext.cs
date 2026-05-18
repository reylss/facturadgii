using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ApiCf.Authorization.Roles;
using ApiCf.Authorization.Users;
using ApiCf.Entidades.ComprobanteFiscalSecNs;
using ApiCf.Entidades.FacturaDGIINs;
using ApiCf.Entidades.IntermediarioNs;
using ApiCf.Entidades.ListaValorNs;
using ApiCf.Entidades.UsuarioNs;
using ApiCf.MultiTenancy;

namespace ApiCf.EntityFrameworkCore
{
    public class ApiCfDbContext : AbpZeroDbContext<Tenant, Role, User, ApiCfDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public DbSet<ListaValor> ListaValor { get; set; }
        public DbSet<Intermediario> Intermediario { get; set; }
        public DbSet<FacturaDGII> FacturaDGII { get; set; }
        public DbSet<ComprobanteFiscalSec> ComprobanteFiscalSec { get; set; }
        public DbSet<Usuario> Usuario { get; set; }

        public ApiCfDbContext(DbContextOptions<ApiCfDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}




