using Abp.Data;
using Abp.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using ApiCf.Entidades.ListaValorNs;
using ApiCf.EntityFrameworkCore;
using ApiCf.EntityFrameworkCore.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ApiCf.CustomRepositories
{
    public class ListaValorRepository : ApiCfRepositoryBase<ListaValor, int>, IListaValorRepository
    {
        private readonly IDbContextProvider<ApiCfDbContext> _dbContextProvider;
        public ListaValorRepository(IDbContextProvider<ApiCfDbContext> dbContextProvider, IActiveTransactionProvider transactionProvider)
          : base(dbContextProvider, transactionProvider)
        {
            _dbContextProvider = dbContextProvider;
        }


        public async Task<List<ListaValor>> ObtenerListaValor(string identificador)
        {
            await EnsureConnectionOpenAsync();

            var pIdentificador = new OracleParameter("identificador", OracleDbType.Varchar2, ParameterDirection.Input)
            {
                Value = identificador
            };

            string sql = @$"SELECT TIPOLVAL, CODLVAL, DESCRIP
                            FROM LVAL 
                            WHERE TIPOLVAL = :identificador";

            using var command = CreateCommand(sql, CommandType.Text, pIdentificador);
            using var dataReader = await command.ExecuteReaderAsync();
            var result = new List<ListaValor>();

            while (dataReader.Read())
            {
                var listaValor = new ListaValor()
                {
                    Codigo = dataReader["CODLVAL"].ToString(),
                    Descripcion = dataReader["DESCRIP"].ToString(),
                    Tipo = dataReader["TIPOLVAL"].ToString()
                };
                result.Add(listaValor);
            }
            return result;
        }
    }
}




