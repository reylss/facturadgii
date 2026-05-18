using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ApiCf.SharedNs
{
    public static class Shared
    {
        private static readonly string ENCRYPTION_KEY = "key";
        private static readonly byte[] SALT = Encoding.ASCII.GetBytes(ENCRYPTION_KEY.Length.ToString());
        private const string ENCRYPTED_PARAMETER_NAME = "enc=";

        public static string DecryptProline(string inputText)
        {
            using (RijndaelManaged rijndaelCipher = new RijndaelManaged())
            {
                try
                {
                    byte[] encryptedData = Convert.FromBase64String(inputText);
                    using (PasswordDeriveBytes secretKey = new PasswordDeriveBytes(ENCRYPTION_KEY, SALT))
                    {
                        // Using
                        ICryptoTransform decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));
                        // Using
                        MemoryStream memoryStream = new MemoryStream(encryptedData);
                        try
                        {
                            // Using
                            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                            try
                            {
                                byte[] plainText = new byte[encryptedData.Length + 1];
                                int decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);
                                return Encoding.Unicode.GetString(plainText, 0, decryptedCount);
                            }
                            catch (Exception ex)
                            {
                                return "ERROR: " + ex;
                            }
                            finally
                            {
                                cryptoStream.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            return "ERROR: " + ex;
                        }
                        finally
                        {
                            memoryStream.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex;
                }
                finally
                {

                }
            }
        }
        public static string EncryptProline(string inputText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            byte[] plainText = Encoding.Unicode.GetBytes(inputText);
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(ENCRYPTION_KEY, SALT);
            ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                try
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                    try
                    {
                        cryptoStream.Write(plainText, 0, plainText.Length);
                        cryptoStream.FlushFinalBlock();
                        return ENCRYPTED_PARAMETER_NAME + Convert.ToBase64String(memoryStream.ToArray());
                    }
                    finally
                    {
                        ((IDisposable)cryptoStream).Dispose();
                    }
                }
                finally
                {
                    ((IDisposable)memoryStream).Dispose();
                }
            }
            finally
            {
                ((IDisposable)encryptor).Dispose();
            }
        }

        private static readonly Random getrandom = new Random();
        public static int GetRandom(int min, int max)
        {
            lock (getrandom) // synchronize
            {
                return getrandom.Next(min, max);
            }
        }
    }
}


