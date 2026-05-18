using Newtonsoft.Json;
using ApiCf.CustomRepositories;
using ApiCf.Entidades.FacturaDGIINs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using static ApiCf.EntityFrameworkCore.CustomRepositories.SignedDGII;
//using static ApiCf.EntityFrameworkCore.CustomRepositories.SignedDGII;


namespace ApiCf.EntityFrameworkCore.CustomRepositories
{
    public class SignedDGII
    {
        string FechaHoraFirma { set; get; }
        public string getFechaFirma()
        {
            return this.FechaHoraFirma;
        }
        public XmlDocument VoucherFormatECF(List<FacturaDGII> facturaDGIIs, List<Comprobantes> PoliticasDGII)
        {
            try
            {
                var facturaDGII = facturaDGIIs.FirstOrDefault();

                XmlDocument xml = new XmlDocument();

                ECF eCF = new ECF();

                // --------------------------------------------
                // Encabezado
                // --------------------------------------------
                Encabezado encabezado = new Encabezado();
                encabezado.Version = "1.0";
                this.FechaHoraFirma = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                eCF.FechaHoraFirma = this.FechaHoraFirma;

                var properties = facturaDGII.GetType().GetProperties();
                List<Errores> errores = new List<Errores>();
                foreach (var comprobante in PoliticasDGII)
                {
                    if (comprobante.Indicador == "1")
                    {
                        if (facturaDGII.GetType().GetProperty(comprobante.Campo.Trim()) != null)
                        {
                            if (facturaDGII.GetType().GetProperty(comprobante.Campo.Trim()).GetValue(facturaDGII, null) == "")
                            {
                                Errores error = new Errores();
                                error.Error = " Campo " + comprobante.Campo + " No puede estar Nulo";
                                errores.Add(error);
                            }
                        }
                        else
                        {
                            Errores error = new Errores();
                            error.Error = " Campo " + comprobante.Campo + " No existe en la consulta";
                            errores.Add(error);
                        }
                    }
                } 
                if (errores.Count() <= 0)
                { 
                     
                    IdDoc idDoc = new IdDoc();

                    //31: Factura de Crédito Fiscal Electrónica
                    //32: Factura de Consumo Electrónica
                    //33: Nota de Débito Electrónica
                    //34: Nota de Crédito Electrónica
                    //41: Compras Electrónico
                    //43: Gastos Menores Electrónico
                    //44: Regímenes Especiales Electrónica
                    //45: Gubernamental Electrónico
                    //46: Comprobante para Exportaciones Electrónico
                    //47: Comprobante para Pagos al Exterior Electrónico
                    idDoc.TipoeCF = facturaDGII.TIPOECF;

                    idDoc.eNCF = facturaDGII.ENCF;
                    if (facturaDGII.FECHAVENCIMIENTOSECUENCIA != "" && idDoc.TipoeCF != "32") { idDoc.FechaVencimientoSecuencia = facturaDGII.FECHAVENCIMIENTOSECUENCIA; }
                    if (facturaDGII.INDICADORNOTACREDITO != "") { idDoc.IndicadorNotaCredito = facturaDGII.INDICADORNOTACREDITO; }
                    if (facturaDGII.INDICADORENVIODIFERIDO != "") { idDoc.IndicadorEnvioDiferido = facturaDGII.INDICADORENVIODIFERIDO; }
                    if (facturaDGII.INDICADORMONTOGRAVADO != "" && idDoc.TipoeCF != "44") { idDoc.IndicadorMontoGravado = facturaDGII.INDICADORMONTOGRAVADO; }

                    //Código Tipo:
                    //01: Ingresos por operaciones(No financieros).
                    //02: Ingresos Financieros
                    //03: Ingresos Extraordinarios
                    //04: Ingresos por Arrendamientos
                    //05: Ingresos por Venta de Activo Depreciable
                    //06: Otros Ingresos
                    if (facturaDGII.TIPOINGRESOS != "")
                    {
                        idDoc.TipoIngresos = facturaDGII.TIPOINGRESOS;
                    }

                    //a) Código Tipo:
                    //1: Contado
                    //2: Crédito
                    if (facturaDGII.TIPOPAGO != "") { idDoc.TipoPago = facturaDGII.TIPOPAGO; }

                    if (facturaDGII.FECHALIMITEPAGO != "") { idDoc.FechaLimitePago = facturaDGII.FECHALIMITEPAGO; }
                    if (facturaDGII.TOTALPAGINAS != "1") { idDoc.TotalPaginas = facturaDGII.TOTALPAGINAS; }


                    encabezado.IdDoc = idDoc;

                    if (facturaDGII.TIPOECF != "34")
                    {
                        if (facturaDGII.FORMAPAGO != "" || facturaDGII.MONTOPAGO != "")
                        {
                            TablaFormasPago tablaFormasPago = new TablaFormasPago();

                            FormaDePago formaDePago = new FormaDePago();
                            if (facturaDGII.FORMAPAGO != "") { formaDePago.FormaPago = facturaDGII.FORMAPAGO; }
                            if (facturaDGII.MONTOPAGO != "") { formaDePago.MontoPago = facturaDGII.MONTOPAGO; }
                            tablaFormasPago.FormaDePago = formaDePago;

                            idDoc.TablaFormasPago = tablaFormasPago;
                        }
                    }

                    Emisor emisor = new Emisor();
                    emisor.RNCEmisor = facturaDGII.RNCEMISOR;
                    emisor.RazonSocialEmisor = facturaDGII.RAZONSOCIALEMISOR;
                    emisor.NombreComercial = facturaDGII.NOMBRECOMERCIAL;//Nombre_Comercial
                    emisor.DireccionEmisor = facturaDGII.DIRECCIONEMISOR;//DIRECCION_Emisor
                    if (facturaDGII.NUMEROFACTURAINTERNA != "")
                    {
                        emisor.NumeroFacturaInterna = facturaDGII.NUMEROFACTURAINTERNA;//NO_DOCUMENTO
                    }
                    emisor.FechaEmision = facturaDGII.FECHAEMISION;//FECDOCU
                    encabezado.Emisor = emisor;
                    // if (facturaDGII.RNCCOMPRADOR != "" || facturaDGII.IDENTIFICADOREXTRANJERO != "" || facturaDGII.RAZONSOCIALCOMPRADOR != "" || facturaDGII.DIRECCIONCOMPRADOR != "")

                    if ((facturaDGII.RNCCOMPRADOR != "" || facturaDGII.IDENTIFICADOREXTRANJERO != "") && facturaDGII.RAZONSOCIALCOMPRADOR != "" && facturaDGII.DIRECCIONCOMPRADOR != "")
                    {
                        Comprador comprador = new Comprador();
                        if (facturaDGII.RNCCOMPRADOR != "") { comprador.RNCComprador = facturaDGII.RNCCOMPRADOR; }//RNC_Comprador
                        if (facturaDGII.IDENTIFICADOREXTRANJERO != "") {
                            comprador.IdentificadorExtranjero = facturaDGII.IDENTIFICADOREXTRANJERO;
                        }
                        if (facturaDGII.RAZONSOCIALCOMPRADOR != "") { comprador.RazonSocialComprador = facturaDGII.RAZONSOCIALCOMPRADOR; }//RAzonsocialcomprador
                        if (facturaDGII.DIRECCIONCOMPRADOR != "") { comprador.DireccionComprador = facturaDGII.DIRECCIONCOMPRADOR; }//Direccion_Comprador
                        encabezado.Comprador = comprador;
                    }




                    Totales totales = new Totales();
                    if (facturaDGII.MONTOGRAVADOTOTAL != "") { totales.MontoGravadoTotal = facturaDGII.MONTOGRAVADOTOTAL; }//MontoGravadoTotal
                                                                                                                           //totales.MontoGravadoI1 = facturaDGII.MONTOGRAVADOI1;//MontoGravado1 
                    if (facturaDGII.MONTOGRAVADOI1 != "") { totales.MontoGravadoI1 = facturaDGII.MONTOGRAVADOI1; }//MontoGravado2
                    if (facturaDGII.MONTOGRAVADOI2 != "") { totales.MontoGravadoI2 = facturaDGII.MONTOGRAVADOI2; }//MontoGravado2
                    if (facturaDGII.MONTOGRAVADOI3 != "") { totales.MontoGravadoI3 = facturaDGII.MONTOGRAVADOI3; }//MontoGravado3 
                    if (facturaDGII.MONTOEXENTO != "") { totales.MontoExento = facturaDGII.MONTOEXENTO; }
                    if (facturaDGII.ITBIS1 != "") { totales.ITBIS1 = facturaDGII.ITBIS1; }//ISC
                    if (facturaDGII.ITBIS2 != "") { totales.ITBIS2 = facturaDGII.ITBIS2; }//ISC
                    if (facturaDGII.ITBIS3 != "") { totales.ITBIS3 = facturaDGII.ITBIS3; }//ISC
                    if (facturaDGII.TOTALITBIS != "") { totales.TotalITBIS = facturaDGII.TOTALITBIS; }//Impuesto_ISC
                    if (facturaDGII.TOTALITBIS1 != "") { totales.TotalITBIS1 = facturaDGII.TOTALITBIS1; }//Impuesto_ISC
                    if (facturaDGII.TOTALITBIS2 != "") { totales.TotalITBIS2 = facturaDGII.TOTALITBIS2; }
                    if (facturaDGII.TOTALITBIS3 != "") { totales.TotalITBIS3 = facturaDGII.TOTALITBIS3; }//Impuesto_ISC
                    if (facturaDGII.MONTOIMPUESTOADICIONAL != "") { totales.MontoImpuestoAdicional = facturaDGII.MONTOIMPUESTOADICIONAL; }

                    if (facturaDGII.TIPOIMPUESTO != "" && (facturaDGII.TASAIMPUESTOADICIONAL != "" || facturaDGII.OTROSIMPUESTOSADICIONALES != ""))
                    {
                        ImpuestosAdicionales impuestosAdicionales = new ImpuestosAdicionales();

                        ImpuestoAdicional impuestoAdicional = new ImpuestoAdicional();
                        if (facturaDGII.TIPOIMPUESTO != "") { impuestoAdicional.TipoImpuesto = facturaDGII.TIPOIMPUESTO; }
                        if (facturaDGII.TASAIMPUESTOADICIONAL != "") { impuestoAdicional.TasaImpuestoAdicional = facturaDGII.TASAIMPUESTOADICIONAL; }
                        if (facturaDGII.OTROSIMPUESTOSADICIONALES != "") { impuestoAdicional.OtrosImpuestosAdicionales = facturaDGII.OTROSIMPUESTOSADICIONALES; }
                        impuestosAdicionales.ImpuestoAdicional = impuestoAdicional;

                        totales.ImpuestosAdicionales = impuestosAdicionales;
                    }

                    if (facturaDGII.MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICO != "") { totales.MontoImpuestoSelectivoConsumoEspecifico = facturaDGII.MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICO; }
                    if (facturaDGII.MONTOIMPUESTOSELECTIVOCONSUMOADVALOREM != "") { totales.MontoImpuestoSelectivoConsumoAdvalorem = facturaDGII.MONTOIMPUESTOSELECTIVOCONSUMOADVALOREM; }

                    totales.MontoTotal = facturaDGII.MONTOTOTAL;//PrimaTotal
                    if (facturaDGII.MONTONOFACTURABLE != "") { totales.MontoNoFacturable = facturaDGII.MONTONOFACTURABLE; }
                    if (facturaDGII.TOTALITBISRETENIDO != "") { totales.TotalITBISRetenido = facturaDGII.TOTALITBISRETENIDO; }
                    if (facturaDGII.TOTALISRRETENCION != "") { totales.TotalISRRetencion = facturaDGII.TOTALISRRETENCION; }
                    if (facturaDGII.TOTALITBISPERCEPCION != "") { totales.TotalITBISPercepcion = facturaDGII.TOTALITBISPERCEPCION; }
                    if (facturaDGII.TOTALISRPERCEPCION != "") { totales.TotalISRPercepcion = facturaDGII.TOTALISRPERCEPCION; }

                    if (facturaDGII.INDICADORAGENTERETENCIONOPERCEPCION == "1")
                    {
                        if (facturaDGII.MONTOITBISRETENIDO != "") { totales.TotalITBISRetenido = facturaDGII.MONTOITBISRETENIDO; }
                        if (facturaDGII.MONTOISRRETENIDO != "") { totales.TotalISRRetencion = facturaDGII.MONTOISRRETENIDO; }
                    }
                    else if (facturaDGII.INDICADORAGENTERETENCIONOPERCEPCION == "2")
                    {
                        if (facturaDGII.MONTOITBISRETENIDO != "") { totales.TotalITBISPercepcion = facturaDGII.MONTOITBISRETENIDO; }
                        if (facturaDGII.MONTOISRRETENIDO != "") { totales.TotalISRPercepcion = facturaDGII.MONTOISRRETENIDO; }
                    }



                    encabezado.Totales = totales;
                    if (!String.IsNullOrEmpty(facturaDGII.TIPOMONEDA)) { 
                    if (facturaDGII.TIPOMONEDA.Contains("USD"))
                    {
                        //Validar si estos totales esta en pesos o USD  
                        OtraMoneda otraMoneda = new OtraMoneda();
                        if (facturaDGII.TIPOMONEDA != "") { otraMoneda.TipoMoneda = facturaDGII.TIPOMONEDA; }//TipoMoneda
                        if (facturaDGII.TIPOCAMBIO != "") { otraMoneda.TipoCambio = facturaDGII.TIPOCAMBIO; }//Tasadecambio
                        //if (facturaDGII.MONTOEXENTOOTRAMONEDA != "" || facturaDGII.MONTOGRAVADOTOTALOTRAMONEDA == "") { otraMoneda.MontoExentoOtraMoneda = facturaDGII.MONTOEXENTOOTRAMONEDA; }
                        if (facturaDGII.MONTOEXENTOOTRAMONEDA != "") { otraMoneda.MontoExentoOtraMoneda = facturaDGII.MONTOEXENTOOTRAMONEDA; }

                        if (facturaDGII.MONTOGRAVADOTOTALOTRAMONEDA != "") { otraMoneda.MontoGravadoTotalOtraMoneda = facturaDGII.MONTOGRAVADOTOTALOTRAMONEDA; }
                        if (facturaDGII.MONTOGRAVADO1OTRAMONEDA != "") { otraMoneda.MontoGravado1OtraMoneda = facturaDGII.MONTOGRAVADO1OTRAMONEDA; }
                        if (facturaDGII.MONTOGRAVADO2OTRAMONEDA != "") { otraMoneda.MontoGravado2OtraMoneda = facturaDGII.MONTOGRAVADO2OTRAMONEDA; }
                        if (facturaDGII.MONTOGRAVADO3OTRAMONEDA != "") { otraMoneda.MontoGravado3OtraMoneda = facturaDGII.MONTOGRAVADO3OTRAMONEDA; }
                        if (facturaDGII.TOTALITBISOTRAMONEDA != "") { otraMoneda.TotalITBISOtraMoneda = facturaDGII.TOTALITBISOTRAMONEDA; }
                        if (facturaDGII.TOTALITBIS1OTRAMONEDA != "") { otraMoneda.TotalITBIS1OtraMoneda = facturaDGII.TOTALITBIS1OTRAMONEDA; }
                        if (facturaDGII.TOTALITBIS2OTRAMONEDA != "") { otraMoneda.TotalITBIS2OtraMoneda = facturaDGII.TOTALITBIS2OTRAMONEDA; }
                        if (facturaDGII.TOTALITBIS3OTRAMONEDA != "") { otraMoneda.TotalITBIS3OtraMoneda = facturaDGII.TOTALITBIS3OTRAMONEDA; }
                        if (facturaDGII.MONTOIMPUESTOADICIONALOTRAMONEDA != "") { otraMoneda.MontoImpuestoAdicionalOtraMoneda = facturaDGII.MONTOIMPUESTOADICIONALOTRAMONEDA; }

                        if (facturaDGII.TIPOIMPUESTOOTRAMONEDA != "" || facturaDGII.TASAIMPUESTOADICIONALOTRAMONEDA != "" || facturaDGII.OTROSIMPUESTOSADICIONALESOTRAMONEDA != "")
                        {
                            ImpuestosAdicionalesOtraMoneda impuestosAdicionalesOtraMoneda = new ImpuestosAdicionalesOtraMoneda();

                            ImpuestoAdicionalOtraMoneda impuestoAdicionalOtraMoneda = new ImpuestoAdicionalOtraMoneda();
                            if (facturaDGII.TIPOIMPUESTOOTRAMONEDA != "") { impuestoAdicionalOtraMoneda.TipoImpuestoOtraMoneda = facturaDGII.TIPOIMPUESTOOTRAMONEDA; }
                            if (facturaDGII.TASAIMPUESTOADICIONALOTRAMONEDA != "") { impuestoAdicionalOtraMoneda.TasaImpuestoAdicionalOtraMoneda = facturaDGII.TASAIMPUESTOADICIONALOTRAMONEDA; }
                            if (facturaDGII.OTROSIMPUESTOSADICIONALESOTRAMONEDA != "") { impuestoAdicionalOtraMoneda.OtrosImpuestosAdicionalesOtraMoneda = facturaDGII.OTROSIMPUESTOSADICIONALESOTRAMONEDA; }
                            impuestosAdicionalesOtraMoneda.ImpuestoAdicionalOtraMoneda = impuestoAdicionalOtraMoneda;

                            otraMoneda.ImpuestosAdicionalesOtraMoneda = impuestosAdicionalesOtraMoneda;
                        }

                        if (facturaDGII.MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICOOTRAMONEDA != "") { otraMoneda.MontoImpuestoSelectivoConsumoEspecificoOtraMoneda = facturaDGII.MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICOOTRAMONEDA; }
                        if (facturaDGII.MONTOIMPUESTOSELECTIVOCONSUMOADVALOREMOTRAMONEDA != "") { otraMoneda.MontoImpuestoSelectivoConsumoAdvaloremOtraMoneda = facturaDGII.MONTOIMPUESTOSELECTIVOCONSUMOADVALOREMOTRAMONEDA; }
                        if (facturaDGII.MONTOTOTALOTRAMONEDA != "") { otraMoneda.MontoTotalOtraMoneda = facturaDGII.MONTOTOTALOTRAMONEDA; }
                        encabezado.OtraMoneda = otraMoneda;
                    }
                   }
                    eCF.Encabezado = encabezado;

                    if (facturaDGII.NCFMODIFICADO != "" || facturaDGII.FECHANCFMODIFICADO != "" || facturaDGII.CODIGOMODIFICACION != "")
                    {
                        InformacionReferencia informacionReferencia = new InformacionReferencia();
                        if (facturaDGII.NCFMODIFICADO != "") { informacionReferencia.NCFModificado = facturaDGII.NCFMODIFICADO; }
                         //   informacionReferencia.RNCOtroContribuyente = facturaDGII.IDENTIFICADOREXTRANJERO;
                        
                        if (facturaDGII.FECHANCFMODIFICADO != "") { informacionReferencia.FechaNCFModificado = facturaDGII.FECHANCFMODIFICADO; }
                        if (facturaDGII.CODIGOMODIFICACION != "") { informacionReferencia.CodigoModificacion = facturaDGII.CODIGOMODIFICACION; }
                        eCF.InformacionReferencia = informacionReferencia;
                    }
                     

                    // --------------------------------------------
                    // DetallesItems
                    // --------------------------------------------
                    DetallesItems detallesItems = new DetallesItems();

                    //List<Item> items = new List<Item>();

                    //validar los items a agregar a la factura
                    Item item = new Item();

                    item.NumeroLinea = 1;

                    //ablaCodigosItem tablaCodigosItem = new TablaCodigosItem();
                    //CodigosItem codigosItem = new CodigosItem();
                    //codigosItem.TipoCodigo = "Interna"; //No Aplica
                    //codigosItem.CodigoItem = "CC0001"; //No Aplica
                    //tablaCodigosItem.CodigosItem = codigosItem;
                    //item.TablaCodigosItem = tablaCodigosItem;

                    //Indica si el ítem es exento, si es gravado, o
                    //No facturable., Indicará las distintas tasas:
                    //0: No Facturable ITBIS 
                    //1: ítem gravado a ITBIS tasa1(18 %).ITBIS 
                    //2: ítem gravado a ITBIS tasa2(16 %).ITBIS 
                    //3: ítem gravado a ITBIS tasa3(0 %).
                    //E: Exento
                    item.IndicadorFacturacion = facturaDGII.INDICADORFACTURACION;//IndicadorFacturacion

                    if (facturaDGII.INDICADORAGENTERETENCIONOPERCEPCION != "")
                    {
                        Retencion retencion = new Retencion();
                        retencion.IndicadorAgenteRetencionoPercepcion = facturaDGII.INDICADORAGENTERETENCIONOPERCEPCION;
                        if (facturaDGII.MONTOITBISRETENIDO != "") { retencion.MontoITBISRetenido = facturaDGII.MONTOITBISRETENIDO; }
                        if (facturaDGII.MONTOISRRETENIDO != "") { retencion.MontoISRRetenido = facturaDGII.MONTOISRRETENIDO; }
                        item.Retencion = retencion;
                    }

                    item.NombreItem = facturaDGII.NOMBRESERVICIO;//NombreServicio
                                                                 //1 Bien 
                                                                 //2 Servicio
                    item.IndicadorBienoServicio = facturaDGII.INDICADORTIPOSERVICIO;//IndicadorTipoServicio
                    item.CantidadItem = facturaDGII.CANTIDADITEM;
                    //12 Dia
                    //43 unidad
                    item.UnidadMedida = facturaDGII.UNIDADMEDIDA;
                    if (facturaDGII.CANTIDADREFERENCIA != "") { item.CantidadReferencia = facturaDGII.CANTIDADREFERENCIA; }
                    if (facturaDGII.UNIDADREFERENCIA != "") { item.UnidadReferencia = facturaDGII.UNIDADREFERENCIA; }

                    if (facturaDGII.TIPOSUBDESCUENTO != "" || facturaDGII.SUBDESCUENTOPORCENTAJE != "" || facturaDGII.MONTOSUBDESCUENTO != "")
                    {

                        TablaSubDescuento tablaSubDescuento = new TablaSubDescuento();
                        SubDescuento subDescuento = new SubDescuento();
                        if (facturaDGII.TIPOSUBDESCUENTO != "") { subDescuento.TipoSubDescuento = facturaDGII.TIPOSUBDESCUENTO; }
                        if (facturaDGII.SUBDESCUENTOPORCENTAJE != "") { subDescuento.SubDescuentoPorcentaje = facturaDGII.SUBDESCUENTOPORCENTAJE; }
                        if (facturaDGII.MONTOSUBDESCUENTO != "") { subDescuento.MontoSubDescuento = facturaDGII.MONTOSUBDESCUENTO; }
                        tablaSubDescuento.SubDescuento = subDescuento;
                        item.TablaSubDescuento = tablaSubDescuento;
                    }

                    if ((facturaDGII.PRECIOOTRAMONEDA != "" && facturaDGII.PRECIOOTRAMONEDA != "0") || (facturaDGII.DESCUENTOOTRAMONEDA != "" && facturaDGII.DESCUENTOOTRAMONEDA != "0") || (facturaDGII.MONTOITEMOTRAMONEDA != "" && facturaDGII.MONTOITEMOTRAMONEDA != "0"))
                    {
                        OtraMonedaDetalle otraMonedaDetalle = new OtraMonedaDetalle();
                        if (facturaDGII.PRECIOOTRAMONEDA != "") { otraMonedaDetalle.PrecioOtraMoneda = facturaDGII.PRECIOOTRAMONEDA; }
                        if (facturaDGII.DESCUENTOOTRAMONEDA != "") { otraMonedaDetalle.DescuentoOtraMoneda = facturaDGII.DESCUENTOOTRAMONEDA; }
                        if (facturaDGII.MONTOITEMOTRAMONEDA != "" && facturaDGII.MONTOITEMOTRAMONEDA != "0") { otraMonedaDetalle.MontoItemOtraMoneda = facturaDGII.MONTOITEMOTRAMONEDA; }
                        item.OtraMonedaDetalle = otraMonedaDetalle;
                    }
                    /*MODIFICADO POR REYNALDO */
                    if (facturaDGII.TASAIMPUESTOADICIONAL != "")
                    {
                        TablaImpuestoAdicional tablaImpuestoAdicional = new TablaImpuestoAdicional();

                        if (facturaDGII.TIPOIMPUESTO != "")
                        {
                            ImpuestoAdicional impuestoAdicional = new ImpuestoAdicional();
                            if (facturaDGII.TIPOIMPUESTO != "") { impuestoAdicional.TipoImpuesto = facturaDGII.TIPOIMPUESTO; }
                            tablaImpuestoAdicional.ImpuestoAdicional = impuestoAdicional;
                        }

                        item.TablaImpuestoAdicional = tablaImpuestoAdicional;
                    }
                    /*MODIFICADO POR REYNALDO 
                    item.PrecioUnitarioItem = facturaDGII.MONTOTOTAL;
                    item.MontoItem = facturaDGII.MONTOTOTAL;*/
                    item.PrecioUnitarioItem = facturaDGII.MONTOITEM;
                    item.MontoItem = facturaDGII.MONTOITEM;
                    if (facturaDGII.RECARGOMONTO != "") { item.RecargoMonto = facturaDGII.RECARGOMONTO; }

                    detallesItems.Item = item;

                    eCF.DetallesItems = detallesItems;

               }
                else
                {
                    eCF.Error = errores;
                } 

                using (XmlWriter writer = xml.CreateNavigator().AppendChild())
                {
                    new XmlSerializer(eCF.GetType()).Serialize(writer, eCF);

                }

                return xml;
            }
            catch (Exception e)
            {
                throw new Exception("Error encriptando: " + e.Message);
            }

        }

        public class IdDoc
        {
            public string TipoeCF { get; set; }
            public string eNCF { get; set; }
            public string FechaVencimientoSecuencia { get; set; }
            public string IndicadorNotaCredito { get; set; }//ToDo
            public string IndicadorEnvioDiferido { get; set; }//ToDo
            public string IndicadorMontoGravado { get; set; }
            public string TipoIngresos { get; set; }
            public string TipoPago { get; set; }
            public string FechaLimitePago { get; set; }
            public TablaFormasPago TablaFormasPago { get; set; }
            public string TotalPaginas { get; set; }//ToDo
        }
        public class TablaFormasPago
        {
            public FormaDePago FormaDePago { get; set; }
        }
        public class FormaDePago
        {
            //1: Efectivo
            //2: Cheque/Transferencia/Depósito
            //3: Tarjeta de Débito/Crédito
            //4: Venta a Crédito
            //5: Bonos o Certificados de regalo
            //6: Permuta
            //7: Nota de crédito
            //8: Otras Formas de pago
            public string FormaPago { get; set; }//ToDo
            public string MontoPago { get; set; }//ToDo
        }

        public class Emisor
        {
            public string RNCEmisor { get; set; }
            public string RazonSocialEmisor { get; set; }
            public string NombreComercial { get; set; }
            public string DireccionEmisor { get; set; }
            public string NumeroFacturaInterna { get; set; }
            public string FechaEmision { get; set; }
        }

        public class Comprador
        {
            public string RNCComprador { get; set; }
            public string IdentificadorExtranjero { get; set; }//ToDo
            public string RazonSocialComprador { get; set; }
            public string DireccionComprador { get; set; }
        }

        public class Totales
        {
            public string MontoGravadoTotal { get; set; }
            public string MontoGravadoI1 { get; set; }
            public string MontoGravadoI2 { get; set; }//ToDo
            public string MontoGravadoI3 { get; set; }//ToDo
            public string MontoExento { get; set; }//ToDo
            public string ITBIS1 { get; set; }
            public string ITBIS2 { get; set; }//ToDo
            public string ITBIS3 { get; set; }//ToDo
            public string TotalITBIS { get; set; }
            public string TotalITBIS1 { get; set; }
            public string TotalITBIS2 { get; set; }//ToDo
            public string TotalITBIS3 { get; set; }//ToDo
            public string MontoImpuestoAdicional { get; set; }//ToDo
            public ImpuestosAdicionales ImpuestosAdicionales { get; set; }//ToDo          
            public string MontoImpuestoSelectivoConsumoEspecifico { get; set; }//ToDo
            public string MontoImpuestoSelectivoConsumoAdvalorem { get; set; }//ToDo
            public string MontoTotal { get; set; }
            public string MontoNoFacturable { get; set; }//ToDo
            public string TotalITBISRetenido { get; set; }//ToDo
            public string TotalISRRetencion { get; set; }//ToDo
            public string TotalITBISPercepcion { get; set; }//ToDo
            public string TotalISRPercepcion { get; set; }//ToDo
        }

        public class ImpuestosAdicionales
        {
            public ImpuestoAdicional ImpuestoAdicional { get; set; }
        }

        public class ImpuestoAdicional
        {
            public string TipoImpuesto { get; set; }//ToDo
            public string TasaImpuestoAdicional { get; set; }//ToDo
            public string OtrosImpuestosAdicionales { get; set; }//ToDo
        }

        public class OtraMoneda
        {
            public string TipoMoneda { get; set; }
            public string TipoCambio { get; set; }
            public string MontoGravadoTotalOtraMoneda { get; set; }
            public string MontoGravado1OtraMoneda { get; set; }
            public string MontoGravado2OtraMoneda { get; set; }//ToDo
            public string MontoGravado3OtraMoneda { get; set; }//ToDo
            public string MontoExentoOtraMoneda { get; set; }//ToDo
            public string TotalITBISOtraMoneda { get; set; }
            public string TotalITBIS1OtraMoneda { get; set; }
            public string TotalITBIS2OtraMoneda { get; set; }//ToDo
            public string TotalITBIS3OtraMoneda { get; set; }//ToDo
            public string MontoImpuestoAdicionalOtraMoneda { get; set; }//ToDo
            public ImpuestosAdicionalesOtraMoneda ImpuestosAdicionalesOtraMoneda { get; set; }//ToDo
            public string MontoImpuestoSelectivoConsumoEspecificoOtraMoneda { get; set; }//ToDo
            public string MontoImpuestoSelectivoConsumoAdvaloremOtraMoneda { get; set; }//ToDo

            public string MontoTotalOtraMoneda { get; set; }
        }

        public class ImpuestosAdicionalesOtraMoneda
        {
            public ImpuestoAdicionalOtraMoneda ImpuestoAdicionalOtraMoneda { get; set; }
        }

        public class ImpuestoAdicionalOtraMoneda
        {
            public string TipoImpuestoOtraMoneda { get; set; }//ToDo
            public string TasaImpuestoAdicionalOtraMoneda { get; set; }//ToDo
            public string OtrosImpuestosAdicionalesOtraMoneda { get; set; }//ToDo
        }

        public class Encabezado
        {
            public string Version { get; set; }
            public IdDoc IdDoc { get; set; }
            public Emisor Emisor { get; set; }
            public Comprador Comprador { get; set; }
            public Totales Totales { get; set; }
            public OtraMoneda OtraMoneda { get; set; }
        }

        public class CodigosItem
        {
            public string TipoCodigo { get; set; }
            public string CodigoItem { get; set; }
        }

        public class TablaCodigosItem
        {
            public CodigosItem CodigosItem { get; set; }
        }

        public class SubDescuento
        {
            public string TipoSubDescuento { get; set; }
            public string SubDescuentoPorcentaje { get; set; }//ToDo
            public string MontoSubDescuento { get; set; }
        }

        public class TablaSubDescuento
        {
            public SubDescuento SubDescuento { get; set; }
        }

        public class OtraMonedaDetalle
        {
            public string PrecioOtraMoneda { get; set; }
            public string DescuentoOtraMoneda { get; set; }
            public string MontoItemOtraMoneda { get; set; }
        }

        public class DescuentosORecargos
        {
            public DescuentoORecargo DescuentoORecargo { get; set; }
        }

        public class DescuentoORecargo
        {
            public string NumeroLinea { get; set; }//ToDo
            public string TipoAjuste { get; set; }//ToDo
            public string TipoValor { get; set; }//ToDo
            public string ValorDescuentooRecargo { get; set; }//ToDo
            public string MontoDescuentooRecargo { get; set; }//ToDo
            public string IndicadorFacturacionDescuentooRecargo { get; set; }//ToDo
        }


        public class InformacionAdicional
        {
            public string IdentificadorExtranjero { get; set; }//ToDo
        }
       public class InformacionReferencia
        {
            public string NCFModificado { get; set; }//ToDo
            public string RNCOtroContribuyente { get; set; }//ToDo
            public string FechaNCFModificado { get; set; }//ToDo
            public string CodigoModificacion { get; set; }//ToDo
        }

        public class Item
        {
            public int NumeroLinea { get; set; }
            public TablaCodigosItem TablaCodigosItem { get; set; }
            public string IndicadorFacturacion { get; set; }
            public Retencion Retencion { get; set; }

            public string NombreItem { get; set; }
            public string IndicadorBienoServicio { get; set; }
            public string CantidadItem { get; set; }
            public string UnidadMedida { get; set; }
            public string CantidadReferencia { get; set; }//ToDo
            public string UnidadReferencia { get; set; }//ToDo
            public string PrecioUnitarioItem { get; set; }
            public string DescuentoMonto { get; set; }
            public TablaSubDescuento TablaSubDescuento { get; set; }
            //public string PrecioUnitarioItem { get; set; }//ToDo
            public string RecargoMonto { get; set; }//ToDo
            public TablaSubRecargo TablaSubRecargo { get; set; }
            public DescuentosORecargos DescuentosORecargos { get; set; }
            public TablaImpuestoAdicional TablaImpuestoAdicional { get; set; }
            public OtraMonedaDetalle OtraMonedaDetalle { get; set; }

            public string MontoItem { get; set; }
        }

        public class TablaImpuestoAdicional
        {
            public ImpuestoAdicional ImpuestoAdicional { get; set; }
        }

        public class TablaSubRecargo
        {
            public string TipoSubRecargo { get; set; }//ToDo
            public string SubRecargoPorcentaje { get; set; }//ToDo
            public string MontoSubRecargo { get; set; }//ToDo
        }

        public class Retencion
        {

            public string IndicadorAgenteRetencionoPercepcion { get; set; }//ToDo
            public string MontoITBISRetenido { get; set; }//ToDo

            public string MontoISRRetenido { get; set; }//ToDo
        }

        public class DetallesItems
        {
            public Item Item { get; set; }
        }

        public class CanonicalizationMethod
        {
            public string Algorithm { get; set; }
        }

        public class SignatureMethod
        {
            public string Algorithm { get; set; }
        }

        public class Transform
        {
            public string Algorithm { get; set; }
        }

        public class Transforms
        {
            public Transform Transform { get; set; }
        }

        public class DigestMethod
        {
            public string Algorithm { get; set; }
        }

        public class Reference
        {
            public Transforms Transforms { get; set; }
            public DigestMethod DigestMethod { get; set; }
            public string DigestValue { get; set; }
            public object URI { get; set; }
            public string text { get; set; }
        }

        public class SignedInfo
        {
            public CanonicalizationMethod CanonicalizationMethod { get; set; }
            public SignatureMethod SignatureMethod { get; set; }
            public Reference Reference { get; set; }
        }

        public class X509Data
        {
            public string X509Certificate { get; set; }
        }

        public class KeyInfo
        {
            public X509Data X509Data { get; set; }
        }

        public class Signature
        {
            public SignedInfo SignedInfo { get; set; }
            public string SignatureValue { get; set; }
            public KeyInfo KeyInfo { get; set; }
            public string xmlns { get; set; }
            public string text { get; set; }
        }

        public class ECF
        {
            public Encabezado Encabezado { get; set; }
            public DetallesItems DetallesItems { get; set; }
            public InformacionReferencia InformacionReferencia { get; set; }
            public string FechaHoraFirma { get; set; }
            public Signature Signature { get; set; }
            public string xsd { get; set; }
            public string xsi { get; set; }
            public string text { get; set; }
            public List<Errores> Error { get; set; }
        }

        public class JsonPoliticasDGII
        {
            public int TipoComprobante { get; set; }
            public ICollection<Comprobantes> Comprobante { get; set; }

        }

        public class Comprobantes
        {
            public string Campo { get; set; }
            public string Indicador { get; set; }
        }

        public class Errores
        {
            public string Error { get; set; }
        }


    }
}

