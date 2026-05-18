using Abp.Extensions;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
namespace ApiCf.CustomRepositories
{
    public class GenerarQR
    {

        public static string GenerateQRCode(string texto)
        {
            int size = 32;
            int margin = 4;
             Color foregroundColor = Color.Black;// Color.FromArgb(255, 155, 225, 233); //Color.FromArgb(255, 155, 225, 233);// Color.Aqua;
            Color backgroundColor = Color.White;// Color.White;/// Color.FromArgb(230, 250, 239);
            string logoPath = string.Empty;
            int logoSizePercentage = 26; // Tamaño del logo como porcentaje del tamaño del código QR
            string outputPath = "qr_code_with_logo.png";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.H);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap logo;
            Bitmap qrCodeImage;
            if (!logoPath.IsNullOrEmpty())
            {
                logo = new Bitmap(logoPath);
                // Convertir el código QR en una imagen
                qrCodeImage = qrCode.GetGraphic(32, foregroundColor, backgroundColor, logo, 90, 6, false);
            }
            else
            {
                qrCodeImage = qrCode.GetGraphic(15, foregroundColor, backgroundColor, false);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                //AddLogoToQRCode(qrCodeImage, logoUrl, logoSizePercentage);
                qrCodeImage.Save(stream, ImageFormat.Jpeg);
                byte[] imageBytes = stream.ToArray();

                return Convert.ToBase64String(imageBytes);

            }
            

        }
        public static string GenerateQRCodeWithOptions(string textOrUrl, int size, int margin, QRCodeGenerator.ECCLevel errorCorrection, Color foregroundColor, Color backgroundColor, string logoUrl, int logoSizePercentage)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(textOrUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            //            Bitmap qrCodeImage = ModifyQRCodeWithLogo(qrCode, logo,200);
            // Crear el generador de código QR 
            //  QRCodeData qrCodeData = qrGenerator.CreateQrCode(textOrUrl, errorCorrection);

            // Crear el código QR a partir de los datos generados
            //            QRCode qrCode = new QRCode(qrCodeData);

            // Obtener la matriz de bits de la imagen del código QR
            Bitmap qrCodeImage = qrCode.GetGraphic(size, foregroundColor, backgroundColor, true);

            // Guardar la imagen del código QR en un archivo
            //  qrCodeImage.Save(outputPath, ImageFormat.Png);

            // Si se proporciona una URL de logo, agregar el logo al código QR
            /* if (!string.IsNullOrEmpty(logoUrl))
             {
                 AddLogoToQRCode(qrCodeImage, logoUrl, logoSizePercentage);
             //    qrCodeImage.Save(outputPath, ImageFormat.Png); // Guardar la imagen actualizada
             }
             */

            using (MemoryStream stream = new MemoryStream())
            {
                qrCodeImage.Save(stream, ImageFormat.Png);
                byte[] imageBytes = stream.ToArray();

                if (!string.IsNullOrEmpty(logoUrl))
                {
                    AddLogoToQRCode(qrCodeImage, logoUrl, logoSizePercentage);

                }
                return Convert.ToBase64String(imageBytes);

            }
        }

        public static void AddLogoToQRCode(Bitmap qrCodeImage, string logoUrl, int logoSizePercentage)
        {
            try
            {
                // Descargar la imagen del logo desde la URL
                using (var wc = new System.Net.WebClient())
                {
                    byte[] bytes = wc.DownloadData(logoUrl);
                    using (var ms = new System.IO.MemoryStream(bytes))
                    {
                        Bitmap logo = new Bitmap(ms);

                        // Calcular el tamaño del logo
                        int logoWidth = qrCodeImage.Width * logoSizePercentage / 100;
                        int logoHeight = qrCodeImage.Height * logoSizePercentage / 100;
                        logo = new Bitmap(logo, new Size(logoWidth, logoHeight));

                        // Calcular la posición del logo (centrado)
                        int xPos = (qrCodeImage.Width - logo.Width) / 2;
                        int yPos = (qrCodeImage.Height - logo.Height) / 2;

                        // Fusionar el logo con la imagen del código QR
                        Graphics g = Graphics.FromImage(qrCodeImage);
                        g.DrawImage(logo, new Point(xPos, yPos));
                        g.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al agregar el logo al código QR: " + ex.Message);
            }
        }

        /*------------*/
        public static string GenerateQRCode1(string url)
        {
            // Configurar opciones del código QR
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.H);
            Color borderColor = Color.Black;// Color.FromArgb(255, 155, 225, 233); //Color.FromArgb(255, 155, 225, 233);// Color.Aqua;
            Color backgroundColor = Color.White;// Color.White;/// Color.FromArgb(230, 250, 239);
            int borderWidth = 10;
            string appearance = "custom";
            QRCode qrCode = new QRCode(qrCodeData);
            string logoPath = "C:\\Repo\\ApiCf\\aspnet-core\\src\\ApiCf.EntityFrameworkCore\\Shared\\Imagen\\condor.png";
            Bitmap logo = new Bitmap(logoPath);
            // Crear bitmap del código QR
            Bitmap qrCodeImage = qrCode.GetGraphic(26, Color.Black, Color.White, GetIcon(logo), borderWidth);

            // Aplicar apariencia personalizada
            if (appearance == "custom")
            {
                ApplyCustomAppearance(qrCodeImage, borderColor, backgroundColor);
            }


            using (MemoryStream stream = new MemoryStream())
            {
                //AddLogoToQRCode(qrCodeImage, logoUrl, logoSizePercentage);
                qrCodeImage.Save(stream, ImageFormat.Png);
                byte[] imageBytes = stream.ToArray();

                return Convert.ToBase64String(imageBytes);

            }
        }

        private static void ApplyCustomAppearance(Bitmap qrCodeImage, Color borderColor, Color backgroundColor)
        {
            // Aplicar borde personalizado
            using (Graphics g = Graphics.FromImage(qrCodeImage))
            using (Pen pen = new Pen(borderColor, 5))
            {
                g.DrawRectangle(pen, 0, 0, qrCodeImage.Width - 1, qrCodeImage.Height - 1);
            }

            // Cambiar color de fondo
            qrCodeImage.SetResolution(300, 300);
            using (Graphics g = Graphics.FromImage(qrCodeImage))
            using (SolidBrush brush = new SolidBrush(backgroundColor))
            {
                g.FillRectangle(brush, 0, 0, qrCodeImage.Width, qrCodeImage.Height);
            }
        }

        private static Bitmap GetIcon(Image logo)
        {
            // Colocar logo en el centro del código QR
            if (logo != null)
            {
                Bitmap icon = new Bitmap(100, 100);
                using (Graphics g = Graphics.FromImage(icon))
                {
                    g.DrawImage(logo, new Rectangle(0, 0, 100, 100));
                }
                return icon;
            }
            return null;
        }
    }

}
