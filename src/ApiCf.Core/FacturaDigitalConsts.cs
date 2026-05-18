using ApiCf.Debugging;

namespace ApiCf
{
    public class ApiCfConsts
    {
        public const string LocalizationSourceName = "ApiCf";
        public const string ConnectionStringName = "Default";
        public const bool MultiTenancyEnabled = false;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "1f14d5727c284ddd8b283a1e0b36ea27";
    }
}




