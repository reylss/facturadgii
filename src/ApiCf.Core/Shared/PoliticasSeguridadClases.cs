namespace ApiCf.SharedNs
{
    public class PoliticasSeguridadClases
    {
        public class PoliticasSeguridad
        {
            public int DiasExpiracionClave { get; set; }
            public string LlaveSeguridad { get; set; }
            public int IntentosFallidosBloqueoUsuario { get; set; }
            public int CantidadMinimaCaracteres { get; set; }
            public bool DebeContenerCombinacionMayusculasMinusculas { get; set; }
            public bool DebeContenerCaracterEspecial { get; set; }
            public bool DebeContenerNumeros { get; set; }
            public bool PuedeContenerNombreEmpresa { get; set; }
            public bool PuedeContenerNombreUsuario { get; set; }
            public bool PuedeContenerNombreReal { get; set; }
        }

        public class ItemLista
        {
            public string Codigo { get; set; }
            public string Descripcion { get; set; }
        }
    }
}




