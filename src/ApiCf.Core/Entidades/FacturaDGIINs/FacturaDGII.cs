using Abp.Domain.Entities;

namespace ApiCf.Entidades.FacturaDGIINs
{
    public class FacturaDGII : Entity<int>
    {
        public string TIPOECF { get; set; }
        public string ENCF { get; set; }
        //public string TIPOINGRESO { get; set; }
        public string TIPOPAGO { get; set; }
        public string FECHALIMITEPAGO { get; set; }
        public string RNCEMISOR { get; set; }
        public string RAZONSOCIALEMISOR { get; set; }
        public string NOMBRECOMERCIAL { get; set; }
        public string DIRECCIONEMISOR { get; set; }
        public string NUMEROFACTURAINTERNA { get; set; }
        public string FECHAEMISION { get; set; }
        public string POLIZA { get; set; }
        //public string COMPROBANTE { get; set; }
        public string FECHACOMPROBANTE { get; set; }
        public string RNCCOMPRADOR { get; set; }
        public string RAZONSOCIALCOMPRADOR { get; set; }
        public string DIRECCIONCOMPRADOR { get; set; }
        public string MONTOGRAVADOTOTAL { get; set; }
        //public string MONTOGRAVADOI1 { get; set; }
        public string ITBIS1 { get; set; }
        public string TOTALITBIS { get; set; }
        public string TOTALITBIS1 { get; set; }
        public string FORMAPAGO { get; set; }
        public string MONTOTOTAL { get; set; }
        public string MONTOGRAVADOTOTALOTRAMONEDA { get; set; }
        public string MONTOGRAVADO1OTRAMONEDA { get; set; }
        public string TOTALITBISOTRAMONEDA { get; set; }
        public string TOTALITBIS1OTRAMONEDA { get; set; }
        public string MONTOTOTALOTRAMONEDA { get; set; }
        public string INDICADORFACTURACION { get; set; }
        public string TIPOMONEDA { get; set; }
        public string TIPOCAMBIO { get; set; }
        public string MONTOEXENTOOTRAMONEDA { get; set; }
        public string CODIGO { get; set; }
        public string NOMBRESERVICIO { get; set; }
        public string INDICADORTIPOSERVICIO { get; set; }
        public string RAMOS { get; set; }
        public string TELEFONO { get; set; }
        public string EMAIL { get; set; }
        public string SUMAASEG { get; set; }
        public string INTERMEDIARIO { get; set; }
        public string VIGENCIA { get; set; }
        public string MONEDA { get; set; }
        public string CONCEPTO { get; set; }
        public string EFECTIVIDAD { get; set; }
        public string IDEPOL { get; set; }
        public string IDEOP { get; set; }
        public string IDEFACT { get; set; }


        public string NOMBREITEM { get; set; }
        public string INDICADORBIENOSERVICIO { get; set; }
        public string CANTIDADITEM { get; set; }
        public string UNIDADMEDIDA { get; set; }
        public string PRECIOUNITARIOITEM { get; set; }
        public string MONTOITEM { get; set; }
        public string NUMEROLINEA { get; set; }
        public string FECHAVENCIMIENTOSECUENCIA { get; set; }
        public string TIPOINGRESOS { get; set; }
        public string TOTALPAGINAS { get; set; }
        public string TOTALES { get; set; }

        public string INDICADORNOTACREDITO { get; set; }
        public string INDICADORENVIODIFERIDO { get; set; }
        public string INDICADORMONTOGRAVADO { get; set; }
        public string MONTOPAGO { get; set; }
        public string IDENTIFICADOREXTRANJERO { get; set; }
        public string MONTOGRAVADOI1 { get; set; }
        public string MONTOGRAVADOI2 { get; set; }
        public string MONTOGRAVADOI3 { get; set; }
        public string MONTOEXENTO { get; set; }
        public string ITBIS2 { get; set; }
        public string ITBIS3 { get; set; }
        public string TOTALITBIS2 { get; set; }
        public string TOTALITBIS3 { get; set; }
        public string MONTOIMPUESTOADICIONAL { get; set; }
        public string IMPUESTOSADICIONALES { get; set; }
        public string TIPOIMPUESTO { get; set; }
        public string TASAIMPUESTOADICIONAL { get; set; }
        public string MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICO { get; set; }
        public string MONTOIMPUESTOSELECTIVOCONSUMOADVALOREM { get; set; }
        public string OTROSIMPUESTOSADICIONALES { get; set; }
        public string MONTONOFACTURABLE { get; set; }
        public string TOTALITBISRETENIDO { get; set; }
        public string TOTALISRRETENCION { get; set; }
        public string TOTALITBISPERCEPCION { get; set; }
        public string TOTALISRPERCEPCION { get; set; }
        public string MONTOIMPUESTOADICIONALOTRAMONEDA { get; set; }
        public string IMPUESTOSADICIONALESOTRAMONEDA { get; set; }
        public string TIPOIMPUESTOOTRAMONEDA { get; set; }
        public string TASAIMPUESTOADICIONALOTRAMONEDA { get; set; }
        public string MONTOIMPUESTOSELECTIVOCONSUMOESPECIFICOOTRAMONEDA { get; set; }
        public string MONTOIMPUESTOSELECTIVOCONSUMOADVALOREMOTRAMONEDA { get; set; }
        public string OTROSIMPUESTOSADICIONALESOTRAMONEDA { get; set; }
        public string MONTOGRAVADO2OTRAMONEDA { get; set; }
        public string MONTOGRAVADO3OTRAMONEDA { get; set; }
        public string TOTALITBIS2OTRAMONEDA { get; set; }
        public string TOTALITBIS3OTRAMONEDA { get; set; }
        public string TIPOSUBDESCUENTO { get; set; }
        public string SUBDESCUENTOPORCENTAJE { get; set; }
        public string MONTOSUBDESCUENTO { get; set; }
        public string PRECIOOTRAMONEDA { get; set; }
        public string DESCUENTOOTRAMONEDA { get; set; }
        public string MONTOITEMOTRAMONEDA { get; set; }
        public string UNIDADREFERENCIA { get; set; }
        public string RECARGOMONTO { get; set; }
        public string CANTIDADREFERENCIA { get; set; }


        public string NCFMODIFICADO { get; set; }
        public string FECHANCFMODIFICADO { get; set; }
        public string CODIGOMODIFICACION { get; set; }

        public string INDICADORAGENTERETENCIONOPERCEPCION { get; set; }
        public string MONTOISRRETENIDO { get; set; }
        public string MONTOITBISRETENIDO { get; set; }


    public string QRPassword { get; set; }
        public string NUMCOM { get; set; }
        public string error { get; set; }

    }
}


