using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCf.Entidades.GenericNs
{
  public  class RutaCertificado
    {
        public string CertificadoPFX { get; set; }
        public string Password { get; set; }
        public string UrlQR { get; set; }
        public string UrlMenorQR { get; set; }
       
    }
}

