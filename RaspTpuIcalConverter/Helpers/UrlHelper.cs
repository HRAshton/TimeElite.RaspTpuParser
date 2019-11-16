using System;
using System.IO;
using System.Net;
using System.Text;

namespace RaspTpuIcalConverter.Helpers
{
    public class UrlHelper
    {
        public bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        public string GetRequestContent(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = new WebProxy(new Uri("http://10.0.25.3:8080")) { UseDefaultCredentials = true };
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var receiveStream = response.GetResponseStream() ?? throw new NullReferenceException();

            var readStream = response.CharacterSet == null
                ? new StreamReader(receiveStream)
                : new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

            var data = readStream.ReadToEnd();

            response.Close();
            readStream.Close();

            return data;

        }
    }
}