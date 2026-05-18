using Newtonsoft.Json.Linq;
using ApiCf.Entidades.FacturaDGIINs;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ApiCf.Entidades.FacturaDGIINs
{
    public interface IFacturaDGIIRepository
    {
        Task<List<FacturaDGII>> ObtenerDatosFactura(String cId, String cToken, String cProcedencia);
        Task<String> ObtenerDatosFacturaJson(FacturaDGII facturaDGII);

        Task<String> ValidarId(String cId, String cToken);
        Task<String> getGenerateQRCode(string cEcf, string cUrl);


        Task<String> GenerarSignXmlWithP12(String cEncf);
        Task<String> GenerarQr(String cRutaQr);
        void EnviarEncfXML();
        void GenerarSSL();

    }
}




