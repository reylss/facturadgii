using Abp.Data;
using Abp.EntityFrameworkCore;
using Abp.Extensions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
//using Castle.Core.Logging;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; 
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using ApiCf.Entidades.FacturaDGIINs;
using ApiCf.Entidades.FacturaDGIINs.Dto;
using ApiCf.Entidades.GenericNs;
using ApiCf.Entidades.IntermediarioNs;
using ApiCf.Entidades.ListaValorNs;
using ApiCf.Entidades.UsuarioNs;
using ApiCf.EntityFrameworkCore;
using ApiCf.EntityFrameworkCore.CustomRepositories;
using ApiCf.EntityFrameworkCore.Repositories;
using ApiCf.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization; 

namespace ApiCf.CustomRepositories
{
    public class FacturaDGIIRepository : ApiCfRepositoryBase<FacturaDGII, int>, IFacturaDGIIRepository
    {
        private readonly IDbContextProvider<ApiCfDbContext> _dbContextProvider;
        private readonly IOptions<RutaCertificado> _configuracionRutasArchivos;
        private readonly IOptions<VoXelGroup> _configuracionVoXelGroup;
        private readonly HttpClient _httpClient;
        X509Certificate2 cert = null;
       
        public Castle.Core.Logging.ILogger Logger { get; set; }
        public FacturaDGIIRepository(IDbContextProvider<ApiCfDbContext> dbContextProvider,
                  IActiveTransactionProvider transactionProvider,
                  IOptions<RutaCertificado> configuracionRutasArchivos,
                  IOptions<VoXelGroup> configuracionVoXelGroup
            )
          : base(dbContextProvider, transactionProvider)
        {

            _dbContextProvider = dbContextProvider;
            _configuracionRutasArchivos = configuracionRutasArchivos;
            _configuracionVoXelGroup = configuracionVoXelGroup;
            _httpClient = new HttpClient();
            Logger = Castle.Core.Logging.NullLogger.Instance; ;

        }

        /// <summary>
        /// Obtiene datos del metodo de factura
        /// </summary>
        /// <param name="IDEOP"></param>
        /// <returns></returns>
        public async Task<string> ObtenerDatosFacturaJson(FacturaDGII facturaDGII )
        {
            await EnsureConnectionOpenAsync();
            // var configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            // var nombreCertificado = configurations["RutaCertificado:CertificadoPFX"];
            // string certificadoDireccion = Path.Combine("wwwroot", "Certificate", nombreCertificado);
            /////////////////////////////////////////////*
            
            string certificadoDireccion = _configuracionRutasArchivos.Value.CertificadoPFX;
            var configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var nombreCertificado = configurations["RutaCertificado:CertificadoPFX"];
            if (cert == null)
            {
                string passw = _configuracionRutasArchivos.Value.Password;
                cert = new X509Certificate2(certificadoDireccion, passw, X509KeyStorageFlags.PersistKeySet);
                X509Store store = new X509Store(StoreName.My);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
            }
          
            string password = _configuracionRutasArchivos.Value.Password;
            string urlQR = _configuracionRutasArchivos.Value.UrlQR;
            string UrlMenorQR = _configuracionRutasArchivos.Value.UrlMenorQR;

            

          
            try
            {
              
                //  using var dataReader = await command.ExecuteReaderAsync();

                

                    //  var facturaDGII = item;// result.FirstOrDefault();
                    try
                    {
                        if ((facturaDGII == null) || (facturaDGII.TIPOECF == ""))
                        {

                            return "No existe datos";
                        }
                        else
                        {
                         /*string sqlPoliticasDGII = @$" SELECT UPPER(Campo) AS Campo, TipoComprobante, Indicador
                                                           FROM acsel.POLITICAS_DGII
                                                          WHERE  TIPOCOMPROBANTE ='" + facturaDGII.TIPOECF + "'  ";


                            using var commandPoliticasDGII = CreateCommand(sqlPoliticasDGII, CommandType.Text);
                            using var dataReaderPoliticasDGII = await commandPoliticasDGII.ExecuteReaderAsync();

                            var resultPoliticasDGII = new List<SignedDGII.Comprobantes>();
                            while (dataReaderPoliticasDGII.Read())
                            {
                                var item = new SignedDGII.Comprobantes
                                {
                                    Campo = dataReaderPoliticasDGII["Campo"].ToString(),
                                    Indicador = dataReaderPoliticasDGII["Indicador"].ToString()
                                };
                                resultPoliticasDGII.Add(item);
                            } 
                            */
                            SignedDGII factura = new SignedDGII();
                            var result = new List<FacturaDGII>();
                                 result.Add(facturaDGII);
                           var resultPoliticasDGII = new  List<SignedDGII.Comprobantes>();
                            var xmlFactura = factura.VoucherFormatECF(result , resultPoliticasDGII  );

                            XmlDocument signXmlWithP12 = SignXmlWithP12(xmlFactura, cert);
                            XmlSerializer serializer = new XmlSerializer(typeof(SignedDGII.ECF));
                            var resultDeserializar = new SignedDGII.ECF();
                            using (TextReader reader = new StringReader(xmlFactura.InnerXml))
                            {
                                resultDeserializar = (SignedDGII.ECF)serializer.Deserialize(reader);
                            }

                            // ------------------------------
                            // Validacion de Campos con error
                            // ------------------------------
                            if (resultDeserializar.Error.Count > 0)
                            {
                                string errores = string.Empty;
                                foreach (var dato in resultDeserializar.Error)
                                {
                                    errores += dato.Error + ", ";
                                }
                                // ------------------------------
                                // Inserta Resultado 
                                // ------------------------------
                                actualizarResultadoPeticion(facturaDGII.IDEFACT, facturaDGII.NUMCOM, facturaDGII.ENCF, "ERRORFACTSURA :" + errores);
                            }
                            else
                            {
                                // ------------------------------
                                // Conusmo API VoXelGroup
                                // ------------------------------
                                DGII dGII = new DGII();
                               // var usernameVoXelGroup = _configuracionVoXelGroup.Value.Username;
                                //var passwordVoXelGroup = _configuracionVoXelGroup.Value.Password;
                                try
                                {
                                 //   var serviceUrlVoXelGroup = _configuracionVoXelGroup.Value.serviceUrl + "Outbox/" + facturaDGII.NUMCOM + ".xml";
                                 //var respuesta = dGII.VoXelGroupPutOutBox(usernameVoXelGroup, passwordVoXelGroup, serviceUrlVoXelGroup, xmlFactura);
                                    //if (respuesta.StatusCode == HttpStatusCode.OK)
                                    //{
                                    var codigoStringQR = (xmlFactura.GetElementsByTagName("Signature")[0].ChildNodes[1].InnerText).ToString().Substring(0, 6);
                                    facturaDGII.QRPassword = codigoStringQR;
                                    var codigoStringQRNoModifiaco = codigoStringQR;
                                    var _codseguridad = ReeplazarCaracterEstecial(codigoStringQR);

                                    codigoStringQR = _codseguridad.Result;
                                    string fechaFirma = factura.getFechaFirma(); // DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss").Replace(" ", "%20");
                                    fechaFirma = fechaFirma.Replace(" ", "%20");
                                    string keyQr = "";
                                    string sqlResultQR = "";
                                    if ((Convert.ToDecimal(facturaDGII.MONTOTOTAL) < 250000) && (facturaDGII.TIPOECF == "32"))
                                    {
                                        keyQr = UrlMenorQR + $@"RNCEmisor={facturaDGII.RNCEMISOR}&ENCF={facturaDGII.ENCF}&FechaEmision={facturaDGII.FECHAEMISION}&MontoTotal={facturaDGII.MONTOTOTAL}&CodigoSeguridad={codigoStringQR}";
                                        sqlResultQR = GenerarQR.GenerateQRCode(keyQr);
                                    }
                                    else
                                    {
                                        if (String.IsNullOrEmpty(facturaDGII.RNCCOMPRADOR) && String.IsNullOrEmpty(facturaDGII.IDENTIFICADOREXTRANJERO))
                                        {
                                            keyQr = urlQR + $@"RNCEmisor={facturaDGII.RNCEMISOR}&ENCF={facturaDGII.ENCF}&FechaEmision={facturaDGII.FECHAEMISION}&MontoTotal={facturaDGII.MONTOTOTAL}&FechaFirma={fechaFirma}&CodigoSeguridad={codigoStringQR}";
                                        }
                                        else if (!String.IsNullOrEmpty(facturaDGII.IDENTIFICADOREXTRANJERO))
                                        {
                                            keyQr = urlQR + $@"RNCEmisor={facturaDGII.RNCEMISOR}&ENCF={facturaDGII.ENCF}&FechaEmision={facturaDGII.FECHAEMISION}&MontoTotal={facturaDGII.MONTOTOTAL}&FechaFirma={fechaFirma}&CodigoSeguridad={codigoStringQR}&identificadorExtranjero={facturaDGII.IDENTIFICADOREXTRANJERO}";

                                        }
                                        else
                                        {
                                            keyQr = urlQR + $@"RNCEmisor={facturaDGII.RNCEMISOR}&RncComprador={facturaDGII.RNCCOMPRADOR}&ENCF={facturaDGII.ENCF}&FechaEmision={facturaDGII.FECHAEMISION}&MontoTotal={facturaDGII.MONTOTOTAL}&FechaFirma={fechaFirma}&CodigoSeguridad={codigoStringQR}";
                                        }
                                        sqlResultQR = GenerarQR.GenerateQRCode(keyQr);
                                    }
                                    actualizarCampoFacturaQR(facturaDGII.IDEFACT, facturaDGII.NUMCOM, facturaDGII.ENCF, keyQr, sqlResultQR, xmlFactura.InnerXml.ToString(), string.Empty, "PRO", fechaFirma, codigoStringQRNoModifiaco, "", codigoStringQR, "I");


                                    //}
                                    //else
                                    //{
                                    //    // ------------------------------
                                    //    // Inserta Resultado 
                                    //    // ------------------------------
                                    //    actualizarResultadoPeticion(facturaDGII.IDEFACT, facturaDGII.NUMCOM, facturaDGII.ENCF, respuesta.Content.ReadAsStringAsync().Result.Replace("'", ""));

                                    //}
                                }
                                catch (Exception err)
                                {
                                    Logger.Info($"ObtenerDatosFactura 468 {err.ToString()}");

                                    Auditoria_error(err.Message);

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Info($"ObtenerDatosFactura 478 {ex.ToString()}");
                        Auditoria_error("* " + ex.ToString());
                        actualizarResultadoPeticion(facturaDGII.IDEFACT, facturaDGII.NUMCOM, facturaDGII.ENCF, ex.Message);
                    return ex.ToString();

                }
                return "OK";

            }
            catch (Exception ex)
            {
                Logger.Info($"ObtenerDatosFactura 486 {ex.ToString()}");
                Auditoria_error("+ " + ex.Message);
               
                return ex.Message;
            }  
             
        }
        public async Task<List<FacturaDGII>> ObtenerDatosFactura(String cId, String cToken, String cProcedencia)
        {
            await EnsureConnectionOpenAsync();
            // var configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            // var nombreCertificado = configurations["RutaCertificado:CertificadoPFX"];
            // string certificadoDireccion = Path.Combine("wwwroot", "Certificate", nombreCertificado);
            /////////////////////////////////////////////*
            var configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            string certificadoDireccion = _configuracionRutasArchivos.Value.CertificadoPFX;
            var nombreCertificado = configurations["RutaCertificado:CertificadoPFX"];
            var intervalo = int.Parse(configurations["Jobs:Job3:IntervalValue"]);
            var intervaloUpdate = int.Parse(configurations["Jobs:Job3:IntervalValueUpdate"]);

            if (cert == null)
            {
                string passw = _configuracionRutasArchivos.Value.Password;
                cert = new X509Certificate2(certificadoDireccion, passw, X509KeyStorageFlags.PersistKeySet);
                X509Store store = new X509Store(StoreName.My);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
            }

            string password = _configuracionRutasArchivos.Value.Password;
            string urlQR = _configuracionRutasArchivos.Value.UrlQR;
            string UrlMenorQR = _configuracionRutasArchivos.Value.UrlMenorQR;

            string sql = @$"SELECT TIPOECF
                                , ENCF 
                                --, TIPOINGRESO
                                , TIPOPAGO
                                , FECHALIMITEPAGO
                                , RNCEMISOR
                                , RAZONSOCIALEMISOR
                                , NOMBRECOMERCIAL
                                , DIRECCIONEMISOR
                                , NUMEROFACTURAINTERNA
                                , FECHAEMISION
                                , POLIZA
                                --, COMPROBANTE
                                , FECHACOMPROBANTE 
                                , RNCCOMPRADOR
                                , RAZONSOCIALCOMPRADOR
                                , DIRECCIONCOMPRADOR
                                , MONTOGRAVADOTOTAL
                                , MONTOGRAVADOI1
                                , ITBIS1
                                , TOTALITBIS
                                , TOTALITBIS1
                                , FORMAPAGO
                                , MONTOTOTAL
                                , MONTOGRAVADOTOTALOTRAMONEDA
                                , MONTOGRAVADO1OTRAMONEDA
                                , TOTALITBISOTRAMONEDA
                                , TOTALITBIS1OTRAMONEDA
                                , MONTOTOTALOTRAMONEDA
                                , INDICADORFACTURACION
                                , Nvl(TIPOMONEDA,'RD') TIPOMONEDA
                                , TIPOCAMBIO
                                , CODIGO
                                , NOMBRESERVICIO
                                , INDICADORTIPOSERVICIO
                                , RAMOS
                                , TELEFONO
                                , EMAIL
                                , SUMAASEG
                                , INTERMEDIARIO
                                , VIGENCIA
                                , MONEDA
                                , CONCEPTO
                                , EFECTIVIDAD
                                , IDEPOL
                                , IDEOP
                                , IDEFACT
                                , NOMBREITEM               
                                , INDICADORBIENOSERVICIO   
                                , CANTIDADITEM             
                                , PRECIOUNITARIOITEM       
                                , MONTOITEM                
                                , NUMEROLINEA              
                                , FECHAVENCIMIENTOSECUENCIA
                                , TIPOINGRESOS             
                                , TOTALPAGINAS             
                                , TOTALES 
                                , INDICADORNOTACREDITO                             
                                , INDICADORENVIODIFERIDO                           
                                , INDICADORMONTOGRAVADO                            
                                , MONTOPAGO                                        
                                , IDENTIFICADOREXTRANJERO     
                                , MONTOGRAVADOI1          
                                , MONTOGRAVADOI2                                   
                                , MONTOGRAVADOI3                                   
                                , MONTOEXENTO                                      
                                , ITBIS2                                           
                                , ITBIS3                                           
                                , TOTALITBIS2                                      
                                , TOTALITBIS3                                      
                                , MONTOIMPUESTOADICIONAL                           
                                , IMPUESTOSADICIONALES                             
                                , TIPOIMPUESTO                                     
                                , TASAIMPUESTOADICIONAL                            
                                , MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICO          
                                , MONTOIMPUESTOSELECTIVOCONSUMOADVALOREM           
                                , OTROSIMPUESTOSADICIONALES                        
                                , MONTONOFACTURABLE                                
                                , TOTALITBISRETENIDO                               
                                , TOTALISRRETENCION                                
                                , TOTALITBISPERCEPCION                             
                                , TOTALISRPERCEPCION      
                                , MONTOEXENTOOTRAMONEDA
                                , MONTOIMPUESTOADICIONALOTRAMONEDA                 
                                , IMPUESTOSADICIONALESOTRAMONEDA                   
                                , TIPOIMPUESTOOTRAMONEDA                           
                                , TASAIMPUESTOADICIONALOTRAMONEDA                  
                                , MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICOOTRAMONEDA
                                , MONTOIMPUESTOSELECTIVOCONSUMOADVALOREMOTRAMONEDA 
                                , OTROSIMPUESTOSADICIONALESOTRAMONEDA              
                                , MONTOGRAVADO2OTRAMONEDA                          
                                , MONTOGRAVADO3OTRAMONEDA                          
                                , TOTALITBIS2OTRAMONEDA                            
                                , TOTALITBIS3OTRAMONEDA                            
                                , TIPOSUBDESCUENTO                                 
                                , SUBDESCUENTOPORCENTAJE                           
                                , MONTOSUBDESCUENTO                                
                                , PRECIOOTRAMONEDA                                 
                                , DESCUENTOOTRAMONEDA                              
                                , MONTOITEMOTRAMONEDA                              
                                , UNIDADREFERENCIA                                 
                                , RECARGOMONTO                                     
                                , CANTIDADREFERENCIA  
                                , NCFMODIFICADO
                                , FECHANCFMODIFICADO 
                                , CODIGOMODIFICACION 
                                , NUMCOM 
                                , INDICADORAGENTERETENCIONOPERCEPCION
                                , MONTOISRRETENIDO
                                , MONTOITBISRETENIDO
                             FROM  FacturaDGII
                            WHERE STSFACDGII='PEN'
                              AND NUMCOMPROBANTE LIKE 'E%' ";
            if (cProcedencia == "DBDGII")
            {
                sql = sql + @$" AND (Id={cId}  )  AND (TOKEN ='{cToken}'  ) ";
            }
            if (cProcedencia == "JOB")
            {
                sql = sql + @$" AND (SYSDATE - FECREGISTRO) * 86400000  >={intervalo}";
            }
            sql = sql + " ORDER BY ID ASC";

            var result = new List<FacturaDGII>();
            using var command = CreateCommand(sql, CommandType.Text);
            try
            {

                using var dataReader = await command.ExecuteReaderAsync();


                while (dataReader.Read())
                {
                    result = new List<FacturaDGII>();
                    var _item = new FacturaDGII()
                    {
                        TIPOECF = dataReader["TIPOECF"].ToString(),
                        ENCF = dataReader["ENCF"].ToString(),
                        //TIPOINGRESO = dataReader["TIPOINGRESO"].ToString(),
                        TIPOPAGO = dataReader["TIPOPAGO"].ToString(),
                        FECHALIMITEPAGO = (dataReader["FECHALIMITEPAGO"].ToString() != "" ? Convert.ToDateTime(dataReader["FECHALIMITEPAGO"].ToString()).ToString("dd-MM-yyyy") : ""),
                        RNCEMISOR = dataReader["RNCEMISOR"].ToString(),
                        RAZONSOCIALEMISOR = dataReader["RAZONSOCIALEMISOR"].ToString(),
                        NOMBRECOMERCIAL = dataReader["NOMBRECOMERCIAL"].ToString(),
                        DIRECCIONEMISOR = dataReader["DIRECCIONEMISOR"].ToString(),
                        NUMEROFACTURAINTERNA = dataReader["NUMEROFACTURAINTERNA"].ToString(),
                        FECHAEMISION = (dataReader["FECHAEMISION"].ToString() != "" ? Convert.ToDateTime(dataReader["FECHAEMISION"].ToString()).ToString("dd-MM-yyyy") : ""),
                        POLIZA = dataReader["POLIZA"].ToString(),
                        //COMPROBANTE = dataReader["COMPROBANTE"].ToString(),
                        FECHACOMPROBANTE = (dataReader["FECHACOMPROBANTE"].ToString() != "" ? Convert.ToDateTime(dataReader["FECHACOMPROBANTE"].ToString()).ToString("dd-MM-yyyy") : ""),
                        RNCCOMPRADOR = dataReader["RNCCOMPRADOR"].ToString(),
                        RAZONSOCIALCOMPRADOR = dataReader["RAZONSOCIALCOMPRADOR"].ToString(),
                        DIRECCIONCOMPRADOR = dataReader["DIRECCIONCOMPRADOR"].ToString(),
                        MONTOGRAVADOTOTAL = dataReader["MONTOGRAVADOTOTAL"].ToString(),
                        //MONTOGRAVADOI1 = dataReader["MONTOGRAVADOI1"].ToString(),
                        ITBIS1 = dataReader["ITBIS1"].ToString(),
                        TOTALITBIS = dataReader["TOTALITBIS"].ToString(),
                        TOTALITBIS1 = dataReader["TOTALITBIS1"].ToString(),
                        FORMAPAGO = dataReader["FORMAPAGO"].ToString(),
                        MONTOTOTAL = dataReader["MONTOTOTAL"].ToString(),
                        MONTOGRAVADOTOTALOTRAMONEDA = dataReader["MONTOGRAVADOTOTALOTRAMONEDA"].ToString(),
                        MONTOGRAVADO1OTRAMONEDA = dataReader["MONTOGRAVADO1OTRAMONEDA"].ToString(),
                        TOTALITBISOTRAMONEDA = dataReader["TOTALITBISOTRAMONEDA"].ToString(),
                        TOTALITBIS1OTRAMONEDA = dataReader["TOTALITBIS1OTRAMONEDA"].ToString(),
                        MONTOTOTALOTRAMONEDA = dataReader["MONTOTOTALOTRAMONEDA"].ToString(),
                        INDICADORFACTURACION = dataReader["INDICADORFACTURACION"].ToString(),
                        TIPOMONEDA = dataReader["TIPOMONEDA"].ToString(),
                        TIPOCAMBIO = dataReader["TIPOCAMBIO"].ToString(),
                        CODIGO = dataReader["CODIGO"].ToString(),
                        NOMBRESERVICIO = dataReader["NOMBRESERVICIO"].ToString(),
                        INDICADORTIPOSERVICIO = dataReader["INDICADORTIPOSERVICIO"].ToString(),
                        RAMOS = dataReader["RAMOS"].ToString(),
                        TELEFONO = dataReader["TELEFONO"].ToString(),
                        EMAIL = dataReader["EMAIL"].ToString(),
                        SUMAASEG = dataReader["SUMAASEG"].ToString(),
                        INTERMEDIARIO = dataReader["INTERMEDIARIO"].ToString(),
                        VIGENCIA = dataReader["VIGENCIA"].ToString(),
                        MONEDA = dataReader["MONEDA"].ToString(),
                        CONCEPTO = dataReader["CONCEPTO"].ToString(),
                        EFECTIVIDAD = dataReader["EFECTIVIDAD"].ToString(),
                        IDEPOL = dataReader["IDEPOL"].ToString(),
                        IDEOP = dataReader["IDEOP"].ToString(),
                        IDEFACT = dataReader["IDEFACT"].ToString(),


                        NOMBREITEM = dataReader["NOMBREITEM"].ToString(),
                        INDICADORBIENOSERVICIO = dataReader["INDICADORBIENOSERVICIO"].ToString(),
                        CANTIDADITEM = dataReader["CANTIDADITEM"].ToString(),
                        PRECIOUNITARIOITEM = dataReader["PRECIOUNITARIOITEM"].ToString(),
                        MONTOITEM = dataReader["MONTOITEM"].ToString(),
                        NUMEROLINEA = dataReader["NUMEROLINEA"].ToString(),
                        FECHAVENCIMIENTOSECUENCIA = (dataReader["FECHAVENCIMIENTOSECUENCIA"].ToString() != "" ? Convert.ToDateTime(dataReader["FECHAVENCIMIENTOSECUENCIA"].ToString()).ToString("dd-MM-yyyy") : ""),
                        TIPOINGRESOS = dataReader["TIPOINGRESOS"].ToString(),
                        TOTALPAGINAS = dataReader["TOTALPAGINAS"].ToString(),
                        //TOTALES = dataReader["TOTALES"].ToString(),

                        INDICADORNOTACREDITO = dataReader["INDICADORNOTACREDITO"].ToString(),
                        INDICADORENVIODIFERIDO = dataReader["INDICADORENVIODIFERIDO"].ToString(),
                        INDICADORMONTOGRAVADO = dataReader["INDICADORMONTOGRAVADO"].ToString(),
                        MONTOPAGO = dataReader["MONTOPAGO"].ToString(),
                        IDENTIFICADOREXTRANJERO = dataReader["IDENTIFICADOREXTRANJERO"].ToString(),
                        MONTOGRAVADOI1 = dataReader["MONTOGRAVADOI1"].ToString(),
                        MONTOGRAVADOI2 = dataReader["MONTOGRAVADOI2"].ToString(),
                        MONTOGRAVADOI3 = dataReader["MONTOGRAVADOI3"].ToString(),
                        MONTOEXENTO = dataReader["MONTOEXENTO"].ToString(),
                        ITBIS2 = dataReader["ITBIS2"].ToString(),
                        ITBIS3 = dataReader["ITBIS3"].ToString(),
                        TOTALITBIS2 = dataReader["TOTALITBIS2"].ToString(),
                        TOTALITBIS3 = dataReader["TOTALITBIS3"].ToString(),
                        MONTOIMPUESTOADICIONAL = dataReader["MONTOIMPUESTOADICIONAL"].ToString(),
                        IMPUESTOSADICIONALES = dataReader["IMPUESTOSADICIONALES"].ToString(),
                        TIPOIMPUESTO = dataReader["TIPOIMPUESTO"].ToString(),
                        TASAIMPUESTOADICIONAL = dataReader["TASAIMPUESTOADICIONAL"].ToString(),
                        MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICO = dataReader["MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICO"].ToString(),
                        MONTOIMPUESTOSELECTIVOCONSUMOADVALOREM = dataReader["MONTOIMPUESTOSELECTIVOCONSUMOADVALOREM"].ToString(),
                        OTROSIMPUESTOSADICIONALES = dataReader["OTROSIMPUESTOSADICIONALES"].ToString(),
                        MONTONOFACTURABLE = dataReader["MONTONOFACTURABLE"].ToString(),
                        TOTALITBISRETENIDO = dataReader["TOTALITBISRETENIDO"].ToString(),
                        TOTALISRRETENCION = dataReader["TOTALISRRETENCION"].ToString(),
                        TOTALITBISPERCEPCION = dataReader["TOTALITBISPERCEPCION"].ToString(),
                        TOTALISRPERCEPCION = dataReader["TOTALISRPERCEPCION"].ToString(),
                        MONTOEXENTOOTRAMONEDA = dataReader["MONTOEXENTOOTRAMONEDA"].ToString(),
                        MONTOIMPUESTOADICIONALOTRAMONEDA = dataReader["MONTOIMPUESTOADICIONALOTRAMONEDA"].ToString(),
                        IMPUESTOSADICIONALESOTRAMONEDA = dataReader["IMPUESTOSADICIONALESOTRAMONEDA"].ToString(),
                        TIPOIMPUESTOOTRAMONEDA = dataReader["TIPOIMPUESTOOTRAMONEDA"].ToString(),
                        TASAIMPUESTOADICIONALOTRAMONEDA = dataReader["TASAIMPUESTOADICIONALOTRAMONEDA"].ToString(),
                        MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICOOTRAMONEDA = dataReader["MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICOOTRAMONEDA"].ToString(),
                        MONTOIMPUESTOSELECTIVOCONSUMOADVALOREMOTRAMONEDA = dataReader["MONTOIMPUESTOSELECTIVOCONSUMOADVALOREMOTRAMONEDA"].ToString(),
                        OTROSIMPUESTOSADICIONALESOTRAMONEDA = dataReader["OTROSIMPUESTOSADICIONALESOTRAMONEDA"].ToString(),
                        MONTOGRAVADO2OTRAMONEDA = dataReader["MONTOGRAVADO2OTRAMONEDA"].ToString(),
                        MONTOGRAVADO3OTRAMONEDA = dataReader["MONTOGRAVADO3OTRAMONEDA"].ToString(),
                        TOTALITBIS2OTRAMONEDA = dataReader["TOTALITBIS2OTRAMONEDA"].ToString(),
                        TOTALITBIS3OTRAMONEDA = dataReader["TOTALITBIS3OTRAMONEDA"].ToString(),
                        TIPOSUBDESCUENTO = dataReader["TIPOSUBDESCUENTO"].ToString(),
                        SUBDESCUENTOPORCENTAJE = dataReader["SUBDESCUENTOPORCENTAJE"].ToString(),
                        MONTOSUBDESCUENTO = dataReader["MONTOSUBDESCUENTO"].ToString(),
                        PRECIOOTRAMONEDA = dataReader["PRECIOOTRAMONEDA"].ToString(),
                        DESCUENTOOTRAMONEDA = dataReader["DESCUENTOOTRAMONEDA"].ToString(),
                        MONTOITEMOTRAMONEDA = dataReader["MONTOITEMOTRAMONEDA"].ToString(),
                        UNIDADREFERENCIA = dataReader["UNIDADREFERENCIA"].ToString(),
                        RECARGOMONTO = dataReader["RECARGOMONTO"].ToString(),
                        CANTIDADREFERENCIA = dataReader["CANTIDADREFERENCIA"].ToString(),

                        NCFMODIFICADO = dataReader["NCFMODIFICADO"].ToString(),
                        FECHANCFMODIFICADO = (dataReader["FECHANCFMODIFICADO"].ToString() != "" ? Convert.ToDateTime(dataReader["FECHANCFMODIFICADO"].ToString()).ToString("dd-MM-yyyy") : ""),
                        CODIGOMODIFICACION = dataReader["CODIGOMODIFICACION"].ToString(),

                        NUMCOM = dataReader["NUMCOM"].ToString(),
                        INDICADORAGENTERETENCIONOPERCEPCION = dataReader["INDICADORAGENTERETENCIONOPERCEPCION"].ToString(),
                        MONTOISRRETENIDO = dataReader["MONTOISRRETENIDO"].ToString(),
                        MONTOITBISRETENIDO = dataReader["MONTOITBISRETENIDO"].ToString()

                    };
                    result.Add(_item);

                    var facturaDGII = result.FirstOrDefault();
                    ActualizaFechaEjecucion(_item.NUMCOM, _item.ENCF, intervaloUpdate);
                    //  var facturaDGII = item;// result.FirstOrDefault();
                    try
                    {
                        if ((facturaDGII == null) || (facturaDGII.TIPOECF == ""))
                        {

                            return result;
                        }
                        else
                        {
                            /*
                            string sqlPoliticasDGII = @$" SELECT UPPER(Campo) AS Campo, TipoComprobante, Indicador
                                                           FROM acsel.POLITICAS_DGII
                                                          WHERE  TIPOCOMPROBANTE ='" + facturaDGII.TIPOECF + "'  ";


                            using var commandPoliticasDGII = CreateCommand(sqlPoliticasDGII, CommandType.Text);
                            using var dataReaderPoliticasDGII = await commandPoliticasDGII.ExecuteReaderAsync();

                            var resultPoliticasDGII = new List<SignedDGII.Comprobantes>();
                            while (dataReaderPoliticasDGII.Read())
                            {
                                var item = new SignedDGII.Comprobantes
                                {
                                    Campo = dataReaderPoliticasDGII["Campo"].ToString(),
                                    Indicador = dataReaderPoliticasDGII["Indicador"].ToString()
                                };
                                resultPoliticasDGII.Add(item);
                            }
                            */
                            SignedDGII factura = new SignedDGII();
                            var resultPoliticasDGII = new List<SignedDGII.Comprobantes>();
                            var xmlFactura = factura.VoucherFormatECF(result, resultPoliticasDGII);

                            XmlDocument signXmlWithP12 = SignXmlWithP12(xmlFactura, cert);
                            XmlSerializer serializer = new XmlSerializer(typeof(SignedDGII.ECF));
                            var resultDeserializar = new SignedDGII.ECF();
                            using (TextReader reader = new StringReader(xmlFactura.InnerXml))
                            {
                                resultDeserializar = (SignedDGII.ECF)serializer.Deserialize(reader);
                            }

                            // ------------------------------
                            // Validacion de Campos con error
                            // ------------------------------
                            if (resultDeserializar.Error.Count > 0)
                            {
                                string errores = string.Empty;
                                foreach (var dato in resultDeserializar.Error)
                                {
                                    errores += dato.Error + ", ";
                                }
                                // ------------------------------
                                // Inserta Resultado 
                                // ------------------------------
                                actualizarResultadoPeticion(facturaDGII.IDEFACT, facturaDGII.NUMCOM, facturaDGII.ENCF, "ERRORFACTSURA :" + errores);
                            }
                            else
                            {
                                // ------------------------------
                                // Conusmo API VoXelGroup
                                // ------------------------------
                                DGII dGII = new DGII();
                                //var usernameVoXelGroup = _configuracionVoXelGroup.Value.Username;
                                //var passwordVoXelGroup = _configuracionVoXelGroup.Value.Password;
                                try
                                {
                                    // serviceUrlVoXelGroup = _configuracionVoXelGroup.Value.serviceUrl + "Outbox/" + facturaDGII.NUMCOM + ".xml";
                                    //var respuesta = dGII.VoXelGroupPutOutBox(usernameVoXelGroup, passwordVoXelGroup, serviceUrlVoXelGroup, xmlFactura);
                                    //if (respuesta.StatusCode == HttpStatusCode.OK)
                                    //{
                                    var codigoStringQR = (xmlFactura.GetElementsByTagName("Signature")[0].ChildNodes[1].InnerText).ToString().Substring(0, 6);
                                    result[0].QRPassword = codigoStringQR;
                                    var codigoStringQRNoModifiaco = codigoStringQR;
                                    var _codseguridad = ReeplazarCaracterEstecial(codigoStringQR);

                                    codigoStringQR = _codseguridad.Result;
                                    string fechaFirma = factura.getFechaFirma(); // DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss").Replace(" ", "%20");
                                    fechaFirma = fechaFirma.Replace(" ", "%20");
                                    string keyQr = "";
                                    string sqlResultQR = "";
                                    if ((Convert.ToDecimal(facturaDGII.MONTOTOTAL) < 250000) && (facturaDGII.TIPOECF == "32"))
                                    {
                                        keyQr = UrlMenorQR + $@"RNCEmisor={facturaDGII.RNCEMISOR}&ENCF={facturaDGII.ENCF}&FechaEmision={facturaDGII.FECHAEMISION}&MontoTotal={facturaDGII.MONTOTOTAL}&CodigoSeguridad={codigoStringQR}";
                                        sqlResultQR = GenerarQR.GenerateQRCode(keyQr);
                                    }
                                    else
                                    {
                                        if (String.IsNullOrEmpty(facturaDGII.RNCCOMPRADOR) && String.IsNullOrEmpty(facturaDGII.IDENTIFICADOREXTRANJERO))
                                        {
                                            keyQr = urlQR + $@"RNCEmisor={facturaDGII.RNCEMISOR}&ENCF={facturaDGII.ENCF}&FechaEmision={facturaDGII.FECHAEMISION}&MontoTotal={facturaDGII.MONTOTOTAL}&FechaFirma={fechaFirma}&CodigoSeguridad={codigoStringQR}";
                                        }
                                        else if (!String.IsNullOrEmpty(facturaDGII.IDENTIFICADOREXTRANJERO))
                                        {
                                            keyQr = urlQR + $@"RNCEmisor={facturaDGII.RNCEMISOR}&ENCF={facturaDGII.ENCF}&FechaEmision={facturaDGII.FECHAEMISION}&MontoTotal={facturaDGII.MONTOTOTAL}&FechaFirma={fechaFirma}&CodigoSeguridad={codigoStringQR}&identificadorExtranjero={facturaDGII.IDENTIFICADOREXTRANJERO}";

                                        }
                                        else
                                        {
                                            keyQr = urlQR + $@"RNCEmisor={facturaDGII.RNCEMISOR}&RncComprador={facturaDGII.RNCCOMPRADOR}&ENCF={facturaDGII.ENCF}&FechaEmision={facturaDGII.FECHAEMISION}&MontoTotal={facturaDGII.MONTOTOTAL}&FechaFirma={fechaFirma}&CodigoSeguridad={codigoStringQR}";
                                        }
                                        sqlResultQR = GenerarQR.GenerateQRCode(keyQr);
                                    }
                                    actualizarCampoFacturaQR(facturaDGII.IDEFACT, facturaDGII.NUMCOM, facturaDGII.ENCF, keyQr, sqlResultQR, xmlFactura.InnerXml.ToString(), string.Empty, "PRO", fechaFirma, codigoStringQRNoModifiaco, "", codigoStringQR, "I");


                                    //}
                                    //else
                                    //{
                                    //    // ------------------------------
                                    //    // Inserta Resultado 
                                    //    // ------------------------------
                                    //    actualizarResultadoPeticion(facturaDGII.IDEFACT, facturaDGII.NUMCOM, facturaDGII.ENCF, respuesta.Content.ReadAsStringAsync().Result.Replace("'", ""));

                                    //}
                                }
                                catch (Exception err)
                                {
                                    Logger.Info($"ObtenerDatosFactura 468 {err.ToString()}");

                                    Auditoria_error(err.Message);

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Info($"ObtenerDatosFactura 478 {ex.ToString()}");
                        Auditoria_error("* " + ex.ToString());
                        actualizarResultadoPeticion(facturaDGII.IDEFACT, facturaDGII.NUMCOM, facturaDGII.ENCF, ex.Message);

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info($"ObtenerDatosFactura 486 {ex.ToString()}");
                Auditoria_error("+ " + ex.Message);
                var fact = new FacturaDGII();
                fact.error = ex.Message;
                result.Add(fact);
            }

            return result;
        }

        public async Task<string> getGenerateQRCode(string cEcf, string cUrl) 
        {
            try
            {
                if ((!cUrl.IsNullOrEmpty()) && (!cUrl.IsNullOrEmpty())) { 
                await EnsureConnectionOpenAsync();
                

                    var sqlQR = "PR_ECF.ECF_QR_API";
                    var parameters = Array.Empty<OracleParameter>();
                    parameters = new OracleParameter[]
                      {
                    new OracleParameter("cEncf", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = cEcf
                    },
                    new OracleParameter("ruta", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = cUrl
                    },
                    new OracleParameter("cqr", OracleDbType.Clob)
                    {
                        Direction = ParameterDirection.Input,
                        Value = GenerarQR.GenerateQRCode(cUrl)
                    }  
                      };
                    using var command = CreateCommand(sqlQR, CommandType.StoredProcedure, parameters);
                    await command.ExecuteNonQueryAsync();

                    return "OK";
                }
                { 
                return "Debe enviar información para ser procesada";
                }
            }
            catch (Exception ex)
            {
                Logger.Info($"ObtenerDatosFactura 1313 {ex.ToString()}");

                return "Error en la información";
            }

             
        }
        public async void actualizarCampoFacturaQR(string pIdeFact, string pNumCom, string pEncf, string pRuta, string pQr, string pEcfxml, string pRespuesta, string pEstado, string pFechaFirma, string pCodigofirma, string pEstadoVoxel, string pCodSeguridadOld  ,string pAccion)
        {
            await EnsureConnectionOpenAsync();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(pEcfxml);
                string formattedXml;
                using (StringWriter stringWriter = new StringWriter())
                {
                    using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
                    {
                        xmlTextWriter.Formatting = System.Xml.Formatting.Indented;
                        xmlDoc.WriteTo(xmlTextWriter);
                        formattedXml = stringWriter.ToString();
                    }
                }


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
                    new OracleParameter("cqr", OracleDbType.Clob)
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
                    }, new OracleParameter("cEstado", OracleDbType.Varchar2)
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
                    }
                    , new OracleParameter("cResp_envio_archivo_voxel", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pEstadoVoxel
                    }
                    , new OracleParameter("cCodSeguridadOld", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =  pCodSeguridadOld
                    }, 
                    new OracleParameter("ACCION", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pAccion
                    }
                    };
                using var command = CreateCommand(sqlQR, CommandType.StoredProcedure, parameters);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {

                actualizarResultadoPeticion(pIdeFact, pNumCom, pEncf, ex.Message);

            }

        }
        public async void actualizarResultadoPeticion(string pIdeFact, string pNumCom, string pEncf, string mensaje)
        {
            await EnsureConnectionOpenAsync();
            string factura = pIdeFact.ToString() != String.Empty ? pIdeFact.ToString() : "null";
            string sqlResultDGII = @$" BEGIN  PR_ECF.RESPUESTA_VOXEL({factura},{pNumCom},'{pEncf}','{mensaje.Replace("'","")}' ) ; COMMIT; END;";
            using var commandResultDGII = CreateCommand(sqlResultDGII, CommandType.Text);
            using var dataReaderResultDGII = commandResultDGII.ExecuteNonQueryAsync();

        }
        public async Task<string> ReeplazarCaracterEstecial(string pCodigoSpecial)
        {
            await EnsureConnectionOpenAsync();
            string sqleQuivalenciaCaractDGII = @$" SELECT CODLVAL, DESCRIP
                                                    FROM acsel.lval
                                                    WHERE  TIPOLVAL ='DGIISEG'  ";


            using var commandCaracterEspecialDGII = CreateCommand(sqleQuivalenciaCaractDGII, CommandType.Text);
            using var dataReaderCaracterEspecialDGII = await commandCaracterEspecialDGII.ExecuteReaderAsync();

            while (dataReaderCaracterEspecialDGII.Read())
            {
                pCodigoSpecial = pCodigoSpecial.Replace(dataReaderCaracterEspecialDGII["CODLVAL"].ToString(), dataReaderCaracterEspecialDGII["DESCRIP"].ToString());

            }
            return pCodigoSpecial;
        }
        public XmlDocument SignXmlWithP12(XmlDocument xmlDoc, X509Certificate2 cert)
        {

            //Obtenemos la llave privada del certificado
            var llavePrivada = cert.PrivateKey;

            //Creamos un SignedXml object que contendrá el documento.
            SignedXml xmlFirmado = new SignedXml(xmlDoc);
            //Atachamos la llave privada al SignedXml document.
            xmlFirmado.SigningKey = llavePrivada;
            //Creamos una referencia del documento a ser firmado.
            Reference reference = new Reference();
            reference.Uri = "";
            //Todo el documento
            //Agregamos un enveloped transformation a la referencia.
            XmlDsigEnvelopedSignatureTransform env = new
            XmlDsigEnvelopedSignatureTransform(true);
            reference.AddTransform(env);
            //Agregamos la referencia al SignedXml object.
            xmlFirmado.AddReference(reference);
            //Creamos un KeyInfo object.
            KeyInfo keyInfo = new KeyInfo();
            //Cargamos el certificado dentro de un KeyInfoX509Data object
            //y luego lo añadimos al KeyInfo object.
            KeyInfoX509Data clause = new KeyInfoX509Data();
            //Opcionalmente se podría agregar el Subject del certificado
            clause.AddSubjectName(cert.Subject);
            clause.AddCertificate(cert);
            keyInfo.AddClause(clause);
            //Asignamos el KeyInfo object al SignedXml object.
            xmlFirmado.KeyInfo = keyInfo;
            //Procedemos a computar la firma.
            xmlFirmado.ComputeSignature();
            //Obtenemos la representación del XML firmado y la guardamos en un XmlElement object
            XmlElement xmlFirmaDigital = xmlFirmado.GetXml();
            //Adicionamos el elemento de la firma al documento XML.

            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlFirmaDigital, true));
            
            return xmlDoc;
        }
      
        public async void Auditoria_error(string message)
        {
            try
            {
                await EnsureConnectionOpenAsync();
                string sqlResultDGII = @$" begin  PR_AUDITORIA_ERROR.raise_error('DGIIAPI',
                                                'archivo_voxel'
                                               ,'{message.Replace("'","")}'); COMMIT; END;";
                using var commandResultDGII = CreateCommand(sqlResultDGII, CommandType.Text);
                using var dataReaderResultDGII = commandResultDGII.ExecuteNonQueryAsync();

            }
            catch (Exception ex)
            {
                Logger.Info($"ObtenerDatosFactura 1313 {ex.ToString()}");


            }



        }
        public async Task<String> ValidarId(string cId = null, string cToken = null)
        {
            string result = "NOK";
            await EnsureConnectionOpenAsync();

            if (cId == null)
            {
                cId =String.Empty;
            }
            

            if (cToken == null)
            {
                cToken = "null";
            }
            

            var pId = new OracleParameter("pId", OracleDbType.Int32, ParameterDirection.Input)
            {
                Value = cId
            };

            var pToken = new OracleParameter("pToken", OracleDbType.Varchar2, ParameterDirection.Input)
            {
                Value = cToken
            };

            string sql = @$"SELECT F.Id 
                              FROM FacturaDgii F
                             WHERE F.STSFACDGII IN ( 'PEN')
                                AND F.Id= :pId AND  Token =:pToken";
             

            using var command = CreateCommand(sql, CommandType.Text, pId, pToken); 
            using var dataReader = await command.ExecuteReaderAsync(); 
            
            while (dataReader.Read())
            {
                result = dataReader["Id"].ToString();
                
            }
            return result;
        }
    
        /// <summary>
        /// Cambios Para Mejorar el flujo 28/06/2028
        /// </summary> 
        /*Generar el firma digital*/
        public async Task<String> GenerarSignXmlWithP12(String cEncf)
        {
            await EnsureConnectionOpenAsync();
            // var configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            // var nombreCertificado = configurations["RutaCertificado:CertificadoPFX"];
            // string certificadoDireccion = Path.Combine("wwwroot", "Certificate", nombreCertificado);
            /////////////////////////////////////////////*

            string certificadoDireccion = _configuracionRutasArchivos.Value.CertificadoPFX;
            var configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var nombreCertificado = configurations["RutaCertificado:CertificadoPFX"];
            if (cert == null)
            {
                string passw = _configuracionRutasArchivos.Value.Password;
                cert = new X509Certificate2(certificadoDireccion, passw, X509KeyStorageFlags.PersistKeySet);
                X509Store store = new X509Store(StoreName.My);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
            }

            string password = _configuracionRutasArchivos.Value.Password;
            string urlQR = _configuracionRutasArchivos.Value.UrlQR;
            string UrlMenorQR = _configuracionRutasArchivos.Value.UrlMenorQR;
            var pEncf = new OracleParameter("pEncf", OracleDbType.Varchar2, ParameterDirection.Input)
            {
                Value = cEncf
            };

            string sql = @$"SELECT ID, IDEFACT, NUMCOM, ENCF,RUTAQR,ECFXML FROM FACTURA_QR WHERE ESTADO='PEN'  AND ( ENCF = :pEncf OR  FECHA +  (5 / 86400) < SYSDATE AND DBMS_LOB.SUBSTR(QR,4000) IS NULL)";
            String cId = "";
            String cNumCom = "";
            String cCodigo = "";
            String cRuta = "";
            String xmlContent = "";
            try
            {
                using var command = CreateCommand(sql, CommandType.Text, pEncf);
                using var dataReader = await command.ExecuteReaderAsync();


                while (dataReader.Read())
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlContent = dataReader["ECFXML"].ToString();
                    cRuta = dataReader["RUTAQR"].ToString();
                    cNumCom = dataReader["NUMCOM"].ToString();
                    cId = dataReader["ID"].ToString();
                    try
                    {
                        if ((cId == null) || (cEncf == ""))
                        {

                            return "no existe cENcf";
                        }
                        else
                        {


                            SignedDGII factura = new SignedDGII();
                            XmlDocument xmlFactura = new XmlDocument();
                            xmlFactura.LoadXml(xmlContent);


                            XmlDocument signXmlWithP12 = SignXmlWithP12(xmlFactura, cert);
                            XmlSerializer serializer = new XmlSerializer(typeof(SignedDGII.ECF));
                            var resultDeserializar = new SignedDGII.ECF();
                            using (TextReader reader = new StringReader(xmlFactura.InnerXml))
                            {
                                resultDeserializar = (SignedDGII.ECF)serializer.Deserialize(reader);
                            }

                            DGII dGII = new DGII();
                            try
                            {
                                var codigoStringQR = (xmlFactura.GetElementsByTagName("Signature")[0].ChildNodes[1].InnerText).ToString().Substring(0, 6);
                                //result[0].QRPassword = codigoStringQR;
                                var codigoStringQRNoModifiaco = codigoStringQR;
                                var _codseguridad = ReeplazarCaracterEstecial(codigoStringQR);
                                codigoStringQR = _codseguridad.Result;

                                //codigoStringQR

                                cRuta = cRuta.Replace("<CODSEGURIDAD>", codigoStringQR);
                                cNumCom = dataReader["NUMCOM"].ToString();
                                cId = dataReader["ID"].ToString();
                                String sqlResultQR = GenerarQR.GenerateQRCode(cRuta);
                                ActualizarQr(cId, cNumCom, cEncf, xmlFactura.InnerXml.ToString(), cRuta, sqlResultQR, codigoStringQRNoModifiaco, codigoStringQR);


                                //actualizarCampoFacturaQR(IDEFACT, NUMCOM, ENCF, keyQr, sqlResultQR, xmlFactura.InnerXml.ToString(), string.Empty, "PRO", fechaFirma, codigoStringQRNoModifiaco, "", codigoStringQR, "I");

                            }
                            catch (Exception err)
                            {
                                Logger.Info($"ObtenerDatosFactura 468 {err.ToString()}");

                                Auditoria_error(err.Message);

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Info($"ObtenerDatosFactura 478 {ex.ToString()}");
                        Auditoria_error("* " + ex.ToString());


                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info($"ObtenerDatosFactura 486 {ex.ToString()}");
                Auditoria_error("+ " + ex.Message);
                 return "Error";
            }

            return "Error";

        }
        public async Task<String> GenerarQr(String cRutaQr)
        {
            await EnsureConnectionOpenAsync();
            String sqlResultQR = "";

            try
            {
                   
                    if (!cRutaQr.IsNullOrEmpty()) { 
                     sqlResultQR = GenerarQR.GenerateQRCode(cRutaQr);
                    }
                    return sqlResultQR;
               
            }
            catch (Exception ex)
            {
                Logger.Info($"ObtenerDatosFactura 486 {ex.ToString()}");
                Auditoria_error("+ " + ex.Message);
                return "Error";
            }

            return "Error";

        }

        /// <summary>
        /// Cambios Para Mejorar el flujo 28/06/2028
        /// </summary> 

        public async void GenerarSSL()
        {
            await EnsureConnectionOpenAsync();
           try
            {

                string certificadoDireccion = _configuracionRutasArchivos.Value.CertificadoPFX;
            var configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var nombreCertificado = configurations["RutaCertificado:CertificadoPFX"];
            if (cert == null)
            {
                string passw = _configuracionRutasArchivos.Value.Password;
                cert = new X509Certificate2(certificadoDireccion, passw, X509KeyStorageFlags.PersistKeySet);
                X509Store store = new X509Store(StoreName.My);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
            }

                string certificadoPEM = ConvertToPEM(cert);
              this.ActualizaSsl(certificadoPEM, _configuracionRutasArchivos.Value.Password);
             
        }
        catch (Exception ex)
        {
            var errorResponse = new
            {
                Status = "Error",
                ErrorMessage = ex.Message,
                ErrorType = ex.GetType().Name,
                GeneratedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            };

                Logger.Info($"ObtenerDat ssl {errorResponse.ToString()}");

            }


        }
        private string ConvertToPEM(X509Certificate2 cert)
        {
            try
            {
                string base64 = Convert.ToBase64String(cert.RawData);

                // Formatear en líneas de 64 caracteres
                var lines = new List<string>();
                for (int i = 0; i < base64.Length; i += 64)
                {
                    int length = Math.Min(64, base64.Length - i);
                    lines.Add(base64.Substring(i, length));
                }

                return "-----BEGIN CERTIFICATE-----\n" +
                       string.Join("\n", lines) +
                       "\n-----END CERTIFICATE-----";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error convirtiendo certificado a PEM: {ex.Message}");
            }
        }
        private string ConvertToBase64(X509Certificate2 cert)
        {
            try
            {
                return Convert.ToBase64String(cert.RawData);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error convirtiendo certificado a Base64: {ex.Message}");
            }
        }
        /* Actualizar QR 27-06-2025*/
        public async void ActualizarQr(string pId, string pNumCom, string pEncf, String pXml, String pRuta, String pQR, String pcodigoQRNoMod, String pcodigoQROld)
        {
            try
            {

                await EnsureConnectionOpenAsync();
                string query = $@"UPDATE acsel.FACTURA_QR
                                    SET  ECFXML  =:pXml,
                                         RUTAQR  =:pRuta,
                                         QR      =:pQR,
                                         CODIGOSEGURIDAD =:pcodigoQRNoMod,
                                         CODSEGURIDADOLD=:pcodigoQROld,
                                         ESTADO ='PRO'
                                   WHERE ID     = :pId
                                     AND NUMCOM = :pNumCom
                                     AND ENCF   = :pEncf";

                var parameters = Array.Empty<OracleParameter>();

                parameters = new OracleParameter[]
                 {
                   new OracleParameter("pXml", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =   pXml
                    },
                  new OracleParameter("pRuta", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =   pRuta
                    },
                     new OracleParameter("pQR", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =   pQR
                    },
                  new OracleParameter("pcodigoQRNoMod", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =   pcodigoQRNoMod
                    },
                  new OracleParameter("pcodigoQROld", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =   pcodigoQROld
                    },
                  new OracleParameter("pId", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =   pId
                    },
                  new OracleParameter("pNumCom", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =   pNumCom
                    },
                    new OracleParameter("pEncf", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pEncf
                    }
                 };
                var command = CreateCommand(query, CommandType.Text, parameters);

                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Logger.Info($"ObtenerDatosFactura 1313 {ex.ToString()}");
                Auditoria_error(ex.Message);
            }


        }
        public async void EnviarEncfXML()
        {
            var parameters = Array.Empty<OracleParameter>();
            DGII dGII = new DGII();
            //string username = "";// "segurossurapro";
            //string password = "";// "cfrR6T@8rYJ2";
            //Inicializaciones
            //  string authorization = DGII.Base64Encode(username + ":" + password);


            string xmlContent = "";
            string sql = @$"SELECT FQ.ENCF  
                                , F.ID IdFactura
                                , FQ.ID 
                                , FQ.NUMCOM  
                                , FQ.ECFXML
                                , F.Idefact
                            FROM  FacturaDGII F,FACTURA_QR FQ
                            WHERE FQ.ESTADO= 'PRO'
                              AND FQ.NUMCOM   = F.NUMCOM
                              AND FQ.ENCF     = F.ENCF 
                              AND F.NUMCOMPROBANTE LIKE 'E%'
                              AND  F.FECEJECUCION + (0.02/24)    < SYSDATE  
                              AND (F.NUMCOMPROBANTE != 'E34' or (F.NUMCOMPROBANTE = 'E34' AND F.NCFMODIFICADO = (SELECT F1.ENCF  
                                                                                                                  FROM FacturaDGII F1
                                                                                                                 WHERE F1.ENCF     = F.NCFMODIFICADO
                                                                                                                   AND F1.STSFACDGII IN('ACC','ACP')))
                                  ) ";//superior a los 3 segundos

            var usernameVoXelGroup = _configuracionVoXelGroup.Value.Username;
            var passwordVoXelGroup = _configuracionVoXelGroup.Value.Password;


            try
            {

                //Inicializaciones
                // string authorization = dGII.Base64Encode(usernameVoXelGroup + ":" + passwordVoXelGroup);
 
                var configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                var intervaloUpdate = int.Parse(configurations["Jobs:Job3:IntervalValueUpdate"]);


                var command = CreateCommand(sql, CommandType.Text, parameters);
                String cEncf = "";
                String cId = "";
                String cIdFactura = "";
                String cNumCom = "";
                String cIdefact = "";
                String cRecrearQr = "";
                using (OracleDataReader reader = (OracleDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cEncf = reader["Encf"].ToString();
                        cId = reader["Id"].ToString();
                        cIdFactura = reader["IdFactura"].ToString();
                        cNumCom = reader["NumCom"].ToString();
                        cIdefact = reader["Idefact"].ToString();
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlContent = reader["ECFXML"].ToString(); // Asumiendo que el campo XML es de tipo CLOB
                    //    cRecrearQr = reader["CREARQR"].ToString();
                        ActualizaFechaEjecucion(cNumCom, cEncf, intervaloUpdate);
                        try
                        {
                            /*    if (cRecrearQr == "N")
                                {*/
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(xmlContent);
                            var serviceUrlVoXelGroup = _configuracionVoXelGroup.Value.serviceUrl + "Outbox/" + cNumCom + ".xml";
                           HttpResponseMessage respuesta = dGII.VoXelGroupPutOutBoxV1(usernameVoXelGroup, passwordVoXelGroup, serviceUrlVoXelGroup, xmlDoc.OuterXml);
                            // Lee el contenido de la respuesta como una cadena
                            string responseBody = await respuesta.Content.ReadAsStringAsync();
                            String estado = "PEN";
                            if (respuesta.ReasonPhrase == "OK")
                            {
                                estado = "ENV";
                            }
                            else if (responseBody.Contains("DocRef:") && (respuesta.ReasonPhrase == "OK"))
                            {
                                estado = "ENV";
                            }
                            else if (responseBody.Contains("DocRef:"))
                            {
                                estado = "BLO";
                            }
                            else
                            {
                                actualizarResultadoPeticion(cIdefact, cNumCom, cEncf, responseBody);
                                estado = "BLO";
                            }
                            actualizarCampoFacturaQR(cIdefact, cNumCom, cEncf, "", "", xmlContent.ToString(), responseBody, estado, "", "", respuesta.StatusCode.ToString(), "", "U");

                        }
                        catch (Exception ex)
                        {

                            Logger.Info($"Envios_voxel_manual  {ex.ToString()}");
                            Auditoria_error("Envios_voxel_manual  " + ex.ToString());
                            actualizarResultadoPeticion(cIdefact, cNumCom, cEncf, ex.Message);


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ;
                Logger.Info($"ObtenerDatosFactura 478 {ex.ToString()}");
                Auditoria_error("* " + ex.ToString());

            }



        }
        public async void ActualizaFechaEjecucion(string pNumCom, string pEncf,int intervaloUpdate)
        {
            try
            {

                await EnsureConnectionOpenAsync();
                string query = $@" UPDATE FACTURADGII
                                  SET  FECEJECUCION= SYSDATE  + ({intervaloUpdate} / 86400000)
                                WHERE NUMCOM = :pNumComV
                                  AND ENCF   = :pEncfV";
                var parameters = Array.Empty<OracleParameter>();

                parameters = new OracleParameter[]
                 {
                  new OracleParameter("pNumComV", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =   pNumCom
                    },
                    new OracleParameter("pEncfV", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pEncf
                    }
                 };
                var command = CreateCommand(query, CommandType.Text, parameters);

                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Logger.Info($"ObtenerDatosFactura 1313 {ex.ToString()}");
                Auditoria_error(ex.Message);
            }


        }
        private async void ActualizaSsl(string pSsl, string pPws)
        {
            try
            {

                await EnsureConnectionOpenAsync();
                string query = $@" BEGIN
                                      DELETE ACSEL.MANT_FACT_TOKEN WHERE TIPO = 'SSL';
                                      INSERT INTO  ACSEL.MANT_FACT_TOKEN(fechaCreacion, Ssl,Pws, Tipo)
                                           VALUES(SYSDATE, :pSsl,acsel.Qb_Encripcion.FB_ENCRIPTAR(:pPws),'SSL');
                                     COMMIT;
                                     END;
                                    ";
                var parameters = Array.Empty<OracleParameter>();

                parameters = new OracleParameter[]
                 {
                  new OracleParameter("pSsl", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value =   pSsl
                    },
                    new OracleParameter("pPws", OracleDbType.Varchar2)
                    {
                        Direction = ParameterDirection.Input,
                        Value = pPws
                    }
                 };
                var command = CreateCommand(query, CommandType.Text, parameters);

                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Logger.Info($"ObtenerDatosFactura 1313 {ex.ToString()}");
                Auditoria_error(ex.Message);
            }


        }


      
    }

}




