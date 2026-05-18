using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCf.Entidades.UsuarioNs
{

    [Table("USUARIO")]
    public class Usuario : Entity<int>
    {
        [Column("CODGRPUSR")]
        public string CodigoGrupoUsuario { get; set; }
        [Column("CODUSR")]
        public string CodigoUsuario { get; set; }
        [Column("NOMUSR")]
        public string NombreUsuario { get; set; }
        [Column("CODSUC")]
        public string CodigoSucursal { get; set; }
        [Column("SUPUSR")]
        public string SuperUsuario { get; set; }
        [Column("MODO")]
        public string Modo { get; set; }
        [Column("DIASCAMB")]
        public int? DiasCamb { get; set; }
        [Column("INDCAMBIAPASS")]
        public string IndCambiaPass { get; set; }
        [Column("FECVENC")]
        public DateTime? FechaVencimiento { get; set; }
        [Column("FECING")]
        public DateTime? FechaIngreso { get; set; }
        [Column("STSUSR")]
        public string Estado { get; set; }
        [Column("CODCIA")]
        public string CodCia { get; set; }
        [Column("CODPROFILE")]
        public string CodProfile { get; set; }
        [Column("INDCIERRECONTA")]
        public string IndCierreConta { get; set; }
        [Column("TIPOUSR")]
        public string TipoUsuario { get; set; }
        [Column("CODUSREXTERNO")]
        public string CodigoUsuarioExterno { get; set; }
        [Column("CODLINEA")]
        public string CodigoLinea { get; set; }
        [Column("COMENT")]
        public string Comentario { get; set; }
        [Column("INDSUPERIOR")]
        public string IndicadorSuperior { get; set; }
        [Column("OIDUSR")]
        public string OidUsr { get; set; }
        [Column("CODEMPLEADO")]
        public string CodigoEmpleado { get; set; }
        [Column("CODSUCURSAL")]
        public string CodSucursal { get; set; }
        [Column("CODUNIDAD")]
        public string CodigoUnidad { get; set; }
        [Column("PASSWORD")]
        public string Password { get; set; }
        [Column("EMAIL")]
        public string Email { get; set; }
        [Column("AUT_LDAP")]
        public string AutLdap { get; set; }
        [Column("CEDULARNC")]
        public string CedulaRnc { get; set; }
        [Column("FECHAULTIMOCAMBIOCLAVE")]
        public DateTime? FechaUltimoCambioClave { get; set; }
        [Column("CANTIDADINTENTOSFALLIDOS")]
        public int? CantidadIntentosFallidos { get; set; }
    }
}




