using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using ApiCf.Entidades.GenericNs;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ApiCf.Shared
{
    public class DGII
    {
        public   string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.ASCII.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public HttpResponseMessage VoXelGroupPutOutBox(string Username, string Password, string ServiceUrl, XmlDocument xmlDocument)
        {
            //try
            //{

            //Variables
            var username = Username;
            var password = Password;
            var serviceUrl = ServiceUrl;

            //Inicializaciones
            string authorization = Base64Encode(username + ":" + password);

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Put, serviceUrl);
            request.Headers.Add("ContentType", "text/xml; encoding='utf-8'");
            request.Headers.Add("Authorization", "Basic " + authorization);
            var content = new StringContent(xmlDocument.InnerXml.ToString(), null, "text/plain");
            request.Content = content;
            var response = client.Send(request);

            //var respuesta = response.Content.ReadAsStringAsync();


            return response;
            //}
            //catch (Exception ex)
            //{
            //    return "error: " + ex.Message;
            //}
        }

        public HttpResponseMessage VoXelGroupGetInBox(string Username, string Password, string ServiceUrl)
        {
            //Variables
            var username = Username;
            var password = Password;
            var serviceUrl = ServiceUrl;
            //Inicializaciones
            string authorization = Base64Encode(username + ":" + password);

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, serviceUrl);
            request.Headers.Add("ContentType", "text/xml; encoding='utf-8'");
            request.Headers.Add("Authorization", "Basic " + authorization);
            var response = client.Send(request);

            return response;
        }
        public HttpResponseMessage VoXelGroupDeleteInBox(string Username, string Password, string ServiceUrl)
        {
            //Variables
            var username = Username;
            var password = Password;
            var serviceUrl = ServiceUrl;

            //Inicializaciones
            string authorization = Base64Encode(username + ":" + password);

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Delete, serviceUrl);
            request.Headers.Add("ContentType", "text/xml; encoding='utf-8'");
            request.Headers.Add("Authorization", "Basic " + authorization);
            var response = client.Send(request);

            return response;
        }
        public HttpResponseMessage VoXelGroupPutOutBoxV1(string Username, string Password, string ServiceUrl, string xmlDocument)
        {
            //try
            //{

            //Variables
            var username = Username;
            var password = Password;
            var serviceUrl = ServiceUrl;

            //Inicializaciones
            string authorization = Base64Encode(username + ":" + password);

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Put, serviceUrl);
            request.Headers.Add("ContentType", "text/xml; encoding='utf-8'");
            request.Headers.Add("Authorization", "Basic " + authorization);//c2VndXJvc3N1cmFwcm86Y2ZyUjZUQDhyWUoy\");
            var content = new StringContent(xmlDocument.ToString(), null, "text/plain");
            request.Content = content;
            var response = client.Send(request);

            //var respuesta = response.Content.ReadAsStringAsync();


            return response;
            //}
            //catch (Exception ex)
            //{
            //    return "error: " + ex.Message;
            //}
        }

    }
}

