using Newtonsoft.Json;

namespace ApiCf.Entidades.ConsultaPadronNs
{


    public partial class ConsultaPadron
    {
        [JsonProperty("errorCode")]
        public long ErrorCode { get; set; }

        [JsonProperty("errorText")]
        public string ErrorText { get; set; }

        [JsonProperty("padron")]
        public Padron Padron { get; set; }
    }

    public partial class Padron
    {
        [JsonProperty("munCed")]
        public long MunCed { get; set; }

        [JsonProperty("seqCed")]
        public long SeqCed { get; set; }

        [JsonProperty("dvCed")]
        public long DvCed { get; set; }

        [JsonProperty("nombres")]
        public string Nombres { get; set; }

        [JsonProperty("apellido1")]
        public string Apellido1 { get; set; }

        [JsonProperty("apellido2")]
        public string Apellido2 { get; set; }

        [JsonProperty("fechaNac")]
        public string FechaNac { get; set; }

        [JsonProperty("lugarNac")]
        public string LugarNac { get; set; }

        [JsonProperty("cedANum")]
        public object CedANum { get; set; }

        [JsonProperty("cedASeri")]
        public object CedASeri { get; set; }

        [JsonProperty("cedASexo")]
        public string CedASexo { get; set; }

        [JsonProperty("sexo")]
        public string Sexo { get; set; }

        [JsonProperty("estCivil")]
        public string EstCivil { get; set; }

        [JsonProperty("estatus")]
        public string Estatus { get; set; }

        [JsonProperty("fotoUrl")]
        public string FotoUrl { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("responseTime")]
        public long ResponseTime { get; set; }
    }


}




