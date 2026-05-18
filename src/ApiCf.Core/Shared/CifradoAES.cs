using ApiCf.Entidades.ListaValorNs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ApiCf.SharedNs
{
    public class CifradoAES
    {
        private readonly IListaValorRepository _listaValorRepository;

        public CifradoAES(IListaValorRepository listaValorRepository)
        {
            _listaValorRepository = listaValorRepository;
        }


        private readonly static Encoding encoding = Encoding.UTF8;

        public static string Encriptar(string password, string key)
        {
            try
            {
                using var aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.Key = encoding.GetBytes(key);
                aes.IV = new byte[16] { 16, 64, 69, 201, 34, 36, 119, 81, 50, 101, 22, 113, 237, 68, 118, 173 };
                ICryptoTransform AESEncrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] buffer = encoding.GetBytes(password);
                string encryptedText = Convert.ToBase64String(AESEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));
                string mac = "";
                mac = BitConverter.ToString(HmacSHA256(Convert.ToBase64String(aes.IV) + encryptedText, key)).Replace("-", "").ToLower();
                var keyValues = new Dictionary<string, object>()
                {
                    {
                        "iv",
                        Convert.ToBase64String(aes.IV)
                    },
                    {
                        "value",
                        encryptedText
                    },
                    {
                        "mac",
                        mac
                    }
                };
                return Convert.ToBase64String(encoding.GetBytes(JsonSerializer.Serialize(keyValues)));
            }
            catch (Exception e)
            {
                throw new Exception("Error encriptando: " + e.Message);
            }
        }

        public static string Desenciptar(string password, string key)
        {
            try
            {
                using var aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.Key = encoding.GetBytes(key);
                byte[] base64Decoded = Convert.FromBase64String(password);
                string base64DecodedStr = encoding.GetString(base64Decoded);
                var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(base64DecodedStr);
                aes.IV = Convert.FromBase64String(payload["iv"]);
                ICryptoTransform AESDecrypt = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] buffer = Convert.FromBase64String(payload["value"]);
                return encoding.GetString(AESDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch (Exception e)
            {
                throw new Exception("Error desenciptando password: " + e.Message);
            }
        }

        private static byte[] HmacSHA256(string data, string key)
        {
            using HMACSHA256 hmac = new HMACSHA256(encoding.GetBytes(key));
            return hmac.ComputeHash(encoding.GetBytes(data));
        }

        public static string EncryptMD5Password(string password)
        {
            var md5hasher = new MD5CryptoServiceProvider();
            byte[] hashedBytes;
            var encoder = new UTF8Encoding();
            hashedBytes = md5hasher.ComputeHash(encoder.GetBytes(password));
            return BitConverter.ToString(hashedBytes);
        }


        private PoliticasSeguridadClases.PoliticasSeguridad ObtenerPoliticasSeguridad()
        {

            var listaValor = _listaValorRepository.ObtenerListaValor("POLISEGU").Result;
            var politicas = new PoliticasSeguridadClases.PoliticasSeguridad()
            {
                DiasExpiracionClave = listaValor.Where(x => x.Codigo == "DIEXCLAV").FirstOrDefault().Descripcion.ToInt(),
                LlaveSeguridad = listaValor.Where(x => x.Codigo == "LLAVSEGU").FirstOrDefault().Descripcion,
                IntentosFallidosBloqueoUsuario = listaValor.Where(x => x.Codigo == "INFABQUS").FirstOrDefault().Descripcion.ToInt(),
                CantidadMinimaCaracteres = listaValor.Where(x => x.Codigo == "CAMICARA").FirstOrDefault().Descripcion.ToInt(),
                DebeContenerCombinacionMayusculasMinusculas = listaValor.Where(x => x.Codigo == "DBCTCBMM").FirstOrDefault().Descripcion.ToBoolStr(),
                DebeContenerCaracterEspecial = listaValor.Where(x => x.Codigo == "DBCTCAES").FirstOrDefault().Descripcion.ToBoolStr(),
                DebeContenerNumeros = listaValor.Where(x => x.Codigo == "DBCTNUME").FirstOrDefault().Descripcion.ToBoolStr(),
                PuedeContenerNombreEmpresa = listaValor.Where(x => x.Codigo == "PDCTNMEM").FirstOrDefault().Descripcion.ToBoolStr(),
                PuedeContenerNombreUsuario = listaValor.Where(x => x.Codigo == "PDCTNMUS").FirstOrDefault().Descripcion.ToBoolStr(),
                PuedeContenerNombreReal = listaValor.Where(x => x.Codigo == "PDCTNMRE").FirstOrDefault().Descripcion.ToBoolStr()
            };

            return politicas;
        }

        public string CifrarPassword(string plainPassword)
        {
            var politicasSeguridad = ObtenerPoliticasSeguridad();
            return Encriptar(plainPassword, politicasSeguridad.LlaveSeguridad);
        }

    }

}




