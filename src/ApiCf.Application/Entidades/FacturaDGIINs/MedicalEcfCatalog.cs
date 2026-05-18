using System;
using System.Collections.Generic;

namespace ApiCf.Entidades.FacturaDGIINs
{
    public static class MedicalEcfCatalog
    {
        public static readonly HashSet<string> SupportedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "31",
            "32",
            "33",
            "34",
            "41",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49"
        };

        public static readonly HashSet<string> SupportedMedicalScenarios = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CONSULTA",
            "PROCEDIMIENTO",
            "ESTUDIO",
            "LABORATORIO",
            "HONORARIOS",
            "SEGURO_MEDICO",
            "COBERTURA_ARS",
            "PACIENTE_PARTICULAR",
            "FACTURACION_CORPORATIVA"
        };
    }
}
