using Abp.Data;
using Abp.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using ApiCf.Entidades.ComprobanteFiscalSecNs;
using ApiCf.Entidades.FacturaDGIINs;
using ApiCf.Entidades.GenericNs;
using ApiCf.EntityFrameworkCore;
using ApiCf.EntityFrameworkCore.CustomRepositories;
using ApiCf.EntityFrameworkCore.Repositories;
using ApiCf.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ApiCf.CustomRepositories
{
    public class ComprobanteFiscalSecRepository : ApiCfRepositoryBase<ComprobanteFiscalSec, int>, IComprobanteFiscalSecRepository
    {
        private readonly IDbContextProvider<ApiCfDbContext> _dbContextProvider;
        private readonly IOptions<RutaCertificado> _configuracionRutasArchivos;
        private readonly IOptions<VoXelGroup> _configuracionVoXelGroup;

        public ComprobanteFiscalSecRepository(IDbContextProvider<ApiCfDbContext> dbContextProvider,
                  IActiveTransactionProvider transactionProvider,
                  IOptions<RutaCertificado> configuracionRutasArchivos,
                  IOptions<VoXelGroup> configuracionVoXelGroup
            )
          : base(dbContextProvider, transactionProvider)
        {
            _dbContextProvider = dbContextProvider;
            _configuracionRutasArchivos = configuracionRutasArchivos;
            _configuracionVoXelGroup = configuracionVoXelGroup;
        }

        /// <summary>
        /// Obtiene los datos COMPROBANTE_FISCAL_SEC que se encuentran en estado Enviado para ser consultados en VoXel
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComprobanteFiscalSec>> ObtenerDatoComprobanteFiscal()
        {
            await EnsureConnectionOpenAsync();

            string sql = @$"SELECT CFS.NUMRELCOM
                                 , CFS.NUMCOMPROBANTE
                                 , CFS.NUMCOM
                                 , CFS.STSCOM
                                 , CFS.FECSTS
                                 , CFS.IDENOTACRED
                                 , COALESCE( CFS.IDEFACT,CFS.IDEFACT_AFECT )  IDEFACT
                                 , CFS.FECREPORTE
                                 , CFS.FECENVIO
                                 , CFS.IDEFACT_AFECT
                                 , CFS.SLDOFACTMONEDA
                                 , CFS.SLDOFACTLOCAL
                                 , CFS.STSDGII
                                 , FD.ENCF AS NCF 
                             FROM COMPROBANTE_FISCAL_SEC CFS ,FACTURADGII FD , FACTURA_QR R
                           WHERE CFS.NUMCOM        = FD.NUMCOM
                             AND CFS.NUMRELCOM     = FD.NUMRELCOM 
                             AND CFS.NUMCOMPROBANTE= FD.NUMCOMPROBANTE
                             AND FD.STSFACDGII='ENV' 
                             AND FD.ENCF =R.ENCF 
                            AND EXTRACT(MINUTE FROM CAST(FECREGISTRO AS TIMESTAMP))>=5
                              ORDER BY FD.ID ASC" ;

            var result = new List<ComprobanteFiscalSec>();
            using var command = CreateCommand(sql, CommandType.Text);
            try
            {
                using var dataReader = await command.ExecuteReaderAsync();


                while (dataReader.Read())
                {
                    var item = new ComprobanteFiscalSec()
                    {

                        NUMRELCOM = dataReader["NUMRELCOM"].ToString(),
                        NUMCOMPROBANTE = dataReader["NUMCOMPROBANTE"].ToString(),
                        NUMCOM = dataReader["NUMCOM"].ToString(),
                        STSCOM = dataReader["STSCOM"].ToString(),
                        FECSTS = dataReader["FECSTS"].ToString(),
                        IDENOTACRED = dataReader["IDENOTACRED"].ToString(),
                        IDEFACT = dataReader["IDEFACT"].ToString(),
                        FECREPORTE = dataReader["FECREPORTE"].ToString(),
                        FECENVIO = dataReader["FECENVIO"].ToString(),
                        IDEFACT_AFECT = dataReader["IDEFACT_AFECT"].ToString(),
                        SLDOFACTMONEDA = dataReader["SLDOFACTMONEDA"].ToString(),
                        SLDOFACTLOCAL = dataReader["SLDOFACTLOCAL"].ToString(),
                        STSDGII = dataReader["STSDGII"].ToString(),
                        NCF = dataReader["NCF"].ToString()
                    };
                    result.Add(item);
                }

                // ------------------------------
                // Conusmo API VoXelGroup
                // ------------------------------
                DGII dGII = new DGII();
                var usernameVoXelGroup = _configuracionVoXelGroup.Value.Username;
                var passwordVoXelGroup = _configuracionVoXelGroup.Value.Password;
                var serviceUrlVoXelGroup = _configuracionVoXelGroup.Value.serviceUrl + "Inbox/";

                foreach (var item in result)
                {
                    var aceptado = serviceUrlVoXelGroup + "Aceptado_" + item.NCF + ".json";
                    var rechazado = serviceUrlVoXelGroup + "Rechazado_" + item.NCF + ".json";
                    var aceptadoCondicional = serviceUrlVoXelGroup + "AceptadoCondicional_" + item.NCF + ".json";
                    string sqlResultDGII = "";
                    var respuestaA = dGII.VoXelGroupGetInBox(usernameVoXelGroup, passwordVoXelGroup, aceptado);
                    if ((respuestaA.StatusCode == HttpStatusCode.OK))
                    {
                        // ------------------------------
                        // Actualizacion de comprobante_fiscal_sec
                        // ------------------------------
                        actualizarCampoFacturaQR(item.IDEFACT, item.NUMCOM, item.NCF, string.Empty, string.Empty, string.Empty, respuestaA.Content.ReadAsStringAsync().Result.Replace("'", ""), "ACP", "", "", "ACP", "U");
                        // ------------------------------
                        // Inserta Resultado 
                        // -----------------------------\
                        actualizarResultadoPeticion(item.IDEFACT, item.NUMCOM, item.NCF, respuestaA.Content.ReadAsStringAsync().Result.Replace("'", ""));

                    }
                    else
                    {
                        var respuestaAC = dGII.VoXelGroupGetInBox(usernameVoXelGroup, passwordVoXelGroup, aceptadoCondicional);

                        if ((respuestaAC.StatusCode == HttpStatusCode.OK))
                        {
                            // ------------------------------
                            // Actualizacion de comprobante_fiscal_sec
                            // ------------------------------
                            actualizarCampoFacturaQR(item.IDEFACT, item.NUMCOM, item.NCF, string.Empty, string.Empty, string.Empty, respuestaAC.Content.ReadAsStringAsync().Result.Replace("'", ""), "ACC", "", "", "ACC", "U");
                            // ------------------------------
                            // Inserta Resultado 
                            // ------------------------------
                            actualizarResultadoPeticion(item.IDEFACT, item.NUMCOM, item.NCF, respuestaAC.Content.ReadAsStringAsync().Result.Replace("'", ""));

                        }
                        else
                        {
                            var respuestaR = dGII.VoXelGroupGetInBox(usernameVoXelGroup, passwordVoXelGroup, rechazado);
                            if ((respuestaR.StatusCode == HttpStatusCode.OK))
                            {
                                // ------------------------------
                                // Actualizacion de comprobante_fiscal_sec
                                // ------------------------------
                                actualizarCampoFacturaQR(item.IDEFACT, item.NUMCOM, item.NCF, string.Empty, string.Empty, string.Empty, respuestaR.Content.ReadAsStringAsync().Result.Replace("'", ""), "RCO", "", "", "RCO", "U");
                                // ------------------------------
                                // Inserta Resultado 
                                // ------------------------------
                                actualizarResultadoPeticion(item.IDEFACT, item.NUMCOM, item.NCF, respuestaR.Content.ReadAsStringAsync().Result.Replace("'", ""));
                            }
                            else
                            {

                                string respuestaApi = respuestaA.Content.ReadAsStringAsync().Result.Replace("'", "");
                                respuestaApi += respuestaR.Content.ReadAsStringAsync().Result.Replace("'", "");
                                respuestaApi += respuestaAC.Content.ReadAsStringAsync().Result.Replace("'", "");
                                if ((respuestaAC.StatusCode != HttpStatusCode.OK) && (respuestaA.StatusCode != HttpStatusCode.OK) && (respuestaA.StatusCode != HttpStatusCode.OK))
                                {
                                    // ------------------------------
                                    // Inserta Resultado 
                                    // ------------------------------
                                    actualizarResultadoPeticion(item.IDEFACT, item.NUMCOM, item.NCF, respuestaApi);

                                }
                            }


                        }

                    }



                }

            }
            catch (Exception ex)
            {
                var _comp = new ComprobanteFiscalSec();
                _comp.error = ex.Message;
                result.Add(_comp);

            }
            return result;

        }
        public async void actualizarCampoFacturaQR(string pIdeFact, string pNumCom, string pEncf, string pRuta, string pQr, string pEcfxml, string pRespuesta, string pEstado, string pFechaFirma, string pCodigofirma,string pEstadoVoxel, string pAccion)
        {
            await EnsureConnectionOpenAsync();


            var parameters = Array.Empty<OracleParameter>();
            // Cotizar
            var sqlQR = "PR_ECF.FACTURA_ELECTRONICA_QR";
            parameters = new OracleParameter[]
              {
                  new OracleParameter("nIdefact", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pIdeFact
                    },
                   new OracleParameter("nNumcom", OracleDbType.Int32)
                    {
                        Direction = ParameterDirection.Input,
                        Value = int.Parse( pNumCom)
                    },
                    new OracleParameter("cEncf", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pEncf
                    },
                    new OracleParameter("ruta", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pRuta
                    },
                    new OracleParameter("qr", OracleDbType.Clob)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pQr
                    },
                    new OracleParameter("cEcfxml", OracleDbType.Clob)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pEcfxml
                    }
                    ,
                    new OracleParameter("pRespuesta", OracleDbType.Clob)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pRespuesta
                    } , new OracleParameter("cEstado", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pEstado
                    },
                     new OracleParameter("cFechaFirmaDigital", OracleDbType.Varchar2)

                     {
                        Direction = ParameterDirection.Input,
                        Value = pFechaFirma
                    },
                      new OracleParameter("cCodigoSeguridad", OracleDbType.Varchar2)
                      {
                        Direction = ParameterDirection.Input,
                        Value = pCodigofirma
                    },new OracleParameter("cResp_envio_archivo_voxel", OracleDbType.Varchar2)
                      {
                        Direction = ParameterDirection.Input,
                        Value = pEstadoVoxel
                    }
                    ,new OracleParameter("cCodSeguridadOld", OracleDbType.Varchar2)
                      {
                        Direction = ParameterDirection.Input,
                        Value = String.Empty
                    }
                    ,new OracleParameter("ACCION", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pAccion
                    }
                };
            using var command = CreateCommand(sqlQR, CommandType.StoredProcedure, parameters);
            await command.ExecuteNonQueryAsync();




        }
        public async void actualizarResultadoPeticion(string pIdeFact, string pNumCom, string pEncf, string mensaje)
        {
            await EnsureConnectionOpenAsync();
            string sqlResultDGII = @$" BEGIN  PR_ECF.RESPUESTA_VOXEL({pIdeFact},{pNumCom},'{pEncf}','{mensaje.Replace("'","")}' ) ; COMMIT; END;";
            using var commandResultDGII = CreateCommand(sqlResultDGII, CommandType.Text);
            using var dataReaderResultDGII = commandResultDGII.ExecuteNonQueryAsync();

        }
        /// <summary>
        /// Obtiene los datos COMPROBANTE_FISCAL_SEC que se encuentran en estado Enviado para ser consultados en VoXel
        /// </summary>
        /// <returns></returns>
        public async void ObtenerComprobanteFiscal()
        {
            await EnsureConnectionOpenAsync();

            try
            {

                // ------------------------------
                // Conusmo API VoXelGroup
                // ------------------------------
                DGII dGII = new DGII();
                var usernameVoXelGroup = _configuracionVoXelGroup.Value.Username;
                var passwordVoXelGroup = _configuracionVoXelGroup.Value.Password;
                var serviceUrlVoXelGroup = _configuracionVoXelGroup.Value.serviceUrl + "Inbox/";


                var aceptado = serviceUrlVoXelGroup;
                string sqlResultDGII = "";
                var respuestaA = dGII.VoXelGroupGetInBox(usernameVoXelGroup, passwordVoXelGroup, aceptado);
                if ((respuestaA.StatusCode == HttpStatusCode.OK))
                {

                    string responseBody = await respuestaA.Content.ReadAsStringAsync();
                    // Convertimos el resultado en un arreglo línea por línea
                    string[] lines = responseBody.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var match = new Regex(@"^(Aceptado|Rechazado|AceptadoCondicional)_");

                    // Recorremos el array
                    foreach (string line in lines)
                    {

                        try
                        {
                            var cEncf = match.Match(line);
                            if (cEncf.Success)
                            {
                                string estado = cEncf.Value.TrimEnd('_');
                                string encflimpio = LimpiarNombreComprobante(line, estado);
                                string estado1 = ObtenerCodigoEstado(estado);
                                var resultado = this.BuscarEncf(encflimpio);

                                if ((resultado != null) && (encflimpio == resultado.Result[0].ENCF))
                                {
                                    if (estado1 == "ACC" || estado1 == "ACP")
                                    {
                                        actualizarCampoFacturaQR(resultado.Result[0].IDEFACT,
                                                          resultado.Result[0].NUMCOM,
                                                          resultado.Result[0].ENCF,
                                                          string.Empty,
                                                          string.Empty,
                                                          string.Empty,
                                                          "",
                                                          estado1,
                                                          "",
                                                          "",
                                                          "OK",
                                                          "U");

                                    }
                                    else if (estado1 == "RCO")
                                    {
                                        var respuesta = dGII.VoXelGroupGetInBox(usernameVoXelGroup,
                                                                                passwordVoXelGroup,
                                                                                serviceUrlVoXelGroup + line);
                                        responseBody = await respuesta.Content.ReadAsStringAsync();

                                        actualizarCampoFacturaQR(resultado.Result[0].IDEFACT,
                                                                 resultado.Result[0].NUMCOM,
                                                                 resultado.Result[0].ENCF,
                                                                 string.Empty,
                                                                 string.Empty,
                                                                 string.Empty,
                                                                 responseBody,
                                                                 estado1,
                                                                 "",
                                                                "",
                                                                respuesta.ReasonPhrase,
                                                                 "U");

                                    }



                                    if (estado1 == "ACC" || estado1 == "ACP" || estado1 == "RCO")
                                    {
                                        try
                                        {
                                            var EliminarRegistro = dGII.VoXelGroupDeleteInBox(usernameVoXelGroup,
                                                                                          passwordVoXelGroup,
                                                                                          serviceUrlVoXelGroup + line);
                                        }
                                        catch (Exception ex)
                                        {
                                            LogearError(ex);

                                        }

                                    }
                                }


                            }
                            else
                            {
                                Console.WriteLine("No se encontró estado válido.");
                            }
                        }
                        catch (Exception ex)
                        {

                            LogearError(ex);
                        }
                    }

                }



            }
            catch (Exception ex)
            {
                var _comp = new ComprobanteFiscalSec();
                _comp.error = ex.Message;

            }
        }
        private string ObtenerCodigoEstado(string estado)
        {
            return estado switch
            {
                "Aceptado" => "ACP",
                "Rechazado" => "RCO",
                "AceptadoCondicional" => "ACC",
                _ => throw new ArgumentException($"Estado no válido: {estado}")
            };
        }
        private string LimpiarNombreComprobante(string line, string estado)
        {
            return Regex.Replace(
                line.Replace($"{estado}_", "").Replace(".json", ""),
                @"\s*\(\d+\)\s*",
                ""
            );
        }
        private async Task LogearError(Exception ex)
        {
            var comprobanteFiscal = new ComprobanteFiscalSec
            {
                error = ex.Message,
            };

            await EnsureConnectionOpenAsync();
            string sqlResultDGII = @$" begin  PR_AUDITORIA_ERROR.raise_error('DGIIAPI',
                                                'Respuesta'
                                               ,'{ex.Message.Replace("'", "")}'); COMMIT; END;";
            using var commandResultDGII = CreateCommand(sqlResultDGII, CommandType.Text);
            using var dataReaderResultDGII = commandResultDGII.ExecuteNonQueryAsync();



            // Aquí podrías guardar en base de datos o log
            Console.WriteLine($"Error en ObtenerComprobanteFiscal: {ex.Message}");
        }

        /******************************/
        public async Task<List<FacturaQr>> BuscarEncf(string pEncf)
        {
            await EnsureConnectionOpenAsync();
            string query = "SELECT NUMCOM, IDEFACT,ENCF  FROM FACTURA_QR WHERE ENCF=:pEncf";
            var parameters = Array.Empty<OracleParameter>();
            parameters = new OracleParameter[]
             {
                  new OracleParameter("pEncf", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pEncf
                    }
             };

            using var command = CreateCommand(query, CommandType.Text, parameters);
            await command.ExecuteNonQueryAsync();
            FacturaQr _qr;
            try
            {
                var result = new List<FacturaQr>();
                using (OracleDataReader reader = (OracleDataReader)command.ExecuteReader())
                {
                    _qr = new FacturaQr();
                    while (reader.Read())
                    {//201048
                        _qr.NUMCOM = reader["NUMCOM"].ToString();
                        _qr.ENCF = reader["ENCF"].ToString();
                        _qr.IDEFACT = reader["IDEFACT"].ToString();

                    }

                    result.Add(_qr);
                }
                return result;
            }
            catch (Exception)
            {

                _qr = new FacturaQr();
                _qr.NUMCOM = "";
                _qr.ENCF = "";
                _qr.IDEFACT = "";
                var result1 = new List<FacturaQr>();
                result1.Add(_qr);
                return result1;
            }





        }


    }
}




