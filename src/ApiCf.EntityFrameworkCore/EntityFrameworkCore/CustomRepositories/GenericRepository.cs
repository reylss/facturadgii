using Abp.Data;
using Abp.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using ApiCf.Entidades.ListaValorNs;
using ApiCf.Entidades.TransaccionesNs;
using ApiCf.EntityFrameworkCore;
using ApiCf.EntityFrameworkCore.Repositories;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Sura.GestionUsuario.CustomRepositories
{
    public class GenericRepository : ApiCfRepositoryBase<Generic, int>, IGenericRepository
    {
        public GenericRepository(IDbContextProvider<ApiCfDbContext> dbContextProvider, IActiveTransactionProvider transactionProvider)
          : base(dbContextProvider, transactionProvider)
        {
        }

        public async Task<DateTime> ObtenerFechaFinalApiCf()
        {
            var result = new DateTime();
            await EnsureConnectionOpenAsync();
            var dFecha = new OracleParameter("dFecha", OracleDbType.Date, ParameterDirection.Input)
            {
                Value = DateTime.Now.Date
            };

            string sql = $@"SELECT PR_INSPECCION.RETORNA_FECHA_INSPECCION(:dFecha) AS FECHA FROM DUAL";

            using var command = CreateCommand(sql, CommandType.Text, dFecha);
            using var dataReader = await command.ExecuteReaderAsync();
            while (dataReader.Read())
            {
                var fecha = Convert.ToDateTime(dataReader["FECHA"]);
                result = fecha;
            }
            return result;
        }

        public async Task<bool> UsuarioTieneOperacionAcceso(string codigoOperacion, string codigoUsuario)
        {
            await EnsureConnectionOpenAsync();
            var pCodigoUsuario = new OracleParameter("codigoUsuario", OracleDbType.Varchar2, ParameterDirection.Input)
            {
                Value = codigoUsuario
            };

            var pCodigoOperacion = new OracleParameter("codigoOperacion", OracleDbType.Varchar2, ParameterDirection.Input)
            {
                Value = codigoOperacion
            };


            string sql = $@"SELECT COUNT(*) FROM OPER_USUARIO WHERE CODUSR = :codigoUsuario AND CODOPER = :codigoOperacion";
            using var command = CreateCommand(sql, CommandType.Text, pCodigoUsuario, pCodigoOperacion);
            using var dataReader = await command.ExecuteReaderAsync();
            int count = Convert.ToInt32(command.ExecuteScalar().ToString());
            return count > 0;
        }
    }
}



