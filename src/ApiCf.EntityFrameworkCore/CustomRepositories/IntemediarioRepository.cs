using Abp.Data;
using Abp.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using ApiCf.Entidades.IntermediarioNs;
using ApiCf.EntityFrameworkCore;
using ApiCf.EntityFrameworkCore.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ApiCf.CustomRepositories
{
    public class IntermediarioRepository : ApiCfRepositoryBase<Intermediario, int>, IIntermediarioRepository
    {
        private readonly IDbContextProvider<ApiCfDbContext> _dbContextProvider;
        public IntermediarioRepository(IDbContextProvider<ApiCfDbContext> dbContextProvider, IActiveTransactionProvider transactionProvider)
          : base(dbContextProvider, transactionProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<Intermediario>> ObtenerListaIntermediarios(string codigo = null, string descripcion = null)
        {
            await EnsureConnectionOpenAsync();

            if (codigo == null)
            {
                codigo = "null";
            }
            else
            {
                codigo = "'" + codigo + "'";
            }

            if (descripcion == null)
            {
                descripcion = "null";
            }
            else
            {
                descripcion = "'" + descripcion + "'";
            }

            var pCodigo = new OracleParameter("codigo", OracleDbType.Varchar2, ParameterDirection.Input)
            {
                Value = codigo
            };

            var pDescripcion = new OracleParameter("descripcion", OracleDbType.Varchar2, ParameterDirection.Input)
            {
                Value = descripcion
            };

            string sql = @$"SELECT I.CODINTER AS CODIGO, PR.NOMBRE_TERCERO (I.TIPOID,I.SERIEID,I.NUMID, I.DVID) AS NOMBRE
                            FROM INTERMEDIARIO I
                            WHERE STSINTER  = 'ACT'
                              AND ((I.CODINTER LIKE '%{codigo.Replace("'", "")}%' OR {codigo} IS NULL) OR (LOWER(PR.NOMBRE_TERCERO(I.TIPOID,I.SERIEID,I.NUMID, I.DVID)) LIKE '%{descripcion.ToLower().Replace("'", "")}%' OR {descripcion} IS NULL))";

            using var command = CreateCommand(sql, CommandType.Text);
            using var dataReader = await command.ExecuteReaderAsync();
            var result = new List<Intermediario>();

            while (dataReader.Read())
            {
                var item = new Intermediario()
                {
                    Codigo = dataReader["CODIGO"].ToString(),
                    Descripcion = dataReader["NOMBRE"].ToString(),
                };
                result.Add(item);
            }
            return result;
        }
    }
}




