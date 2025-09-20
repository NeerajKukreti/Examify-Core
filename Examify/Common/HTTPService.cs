//using System;
//using System.Collections.Generic;
//using System.Web;
//using System.Net;
//using System.Text;
//using System.Security.Cryptography.X509Certificates;
//using System.Net.Security;
//using System.IO;

//namespace Examify.Common
//{
//    public class HTTPService1
//    {
//        public HTTPService1()
//        {
//            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(AcceptCertificate);
//        }

//        public string Post(Uri host, string path, Dictionary<string, string> parameters, NetworkCredential credential)
//        {
//            try
//            {
//                //Uri url = new Uri(host, path);
//                string url = host + "/" + path;
//                StringBuilder payload = new StringBuilder();

//                if (parameters == null || parameters.Count <= 0)
//                {
//                    payload.Clear();
//                }
//                else
//                {
//                    foreach (KeyValuePair<string, string> parameter in parameters)
//                    {
//                        payload.Append(parameter.Key + "=" + parameter.Value + "&");
//                    }
//                }
//                MvcHtmlString encodedPayload = MvcHtmlString.Create(payload.ToString());
//                UTF8Encoding encoding = new UTF8Encoding();
//                byte[] data = encoding.GetBytes(encodedPayload.ToHtmlString());

//                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
//                request.Method = "POST";
//                request.Credentials = credential;
//                request.ContentLength = data.Length;
//                request.KeepAlive = false;
//                request.ContentType = "application/x-www-form-urlencoded";
//                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";

//                MvcHtmlString htmlString1;
//                MvcHtmlString htmlString2;
//                /*foreach (KeyValuePair<string, string> header in headers)
//                {
//                    htmlString1 = MvcHtmlString.Create(header.Key);
//                    htmlString2 = MvcHtmlString.Create(header.Value);
//                    request.Headers.Add(htmlString1.ToHtmlString(), htmlString2.ToHtmlString());
//                }*/

//                using (Stream requestStream = request.GetRequestStream())
//                {
//                    requestStream.Write(data, 0, data.Length);
//                    requestStream.Close();
//                }
//                HttpWebResponse response;

//                try
//                {
//                    response = request.GetResponse() as HttpWebResponse;
//                }
//                catch (WebException ex)
//                {
//                    response = ex.Response as HttpWebResponse;
//                }


//                using (Stream responseStream = response.GetResponseStream())
//                {
//                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
//                    {
//                        throw new HttpException((int)response.StatusCode, response.StatusDescription);
//                    }

//                    string sVal = "";
//                    using (Stream resStream = response.GetResponseStream())
//                    {
//                        //StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
//                        //response.Headers.Add("Application/Jsonpe");
//                        StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
//                        sVal = reader.ReadToEnd();
//                        sVal = sVal.Replace("<", "&lt");
//                        sVal = sVal.Replace(">", "&gt");
//                    }
//                    return sVal;
//                }
//            }
//            catch (Exception e)
//            {
//                //if (e is WebException && ((WebException)e).Status == WebExceptionStatus.ReceiveFailure)
//                //{
//                string a = "";
//                WebResponse errResp = ((WebException)e).Response;
//                using (Stream respStream = errResp.GetResponseStream())
//                {
//                    // read the error response
//                    StreamReader reader = new StreamReader(respStream);
//                    a = reader.ReadToEnd();
//                }
//                //}
//                throw;
//            }
//        }

//        public string Get(Uri host, string path, Dictionary<string, string> parameters, NetworkCredential credential)
//        {
//            byte[] data = null;
//            WebClient client = new WebClient();
//            var values = new System.Collections.Specialized.NameValueCollection();

//            string strResult = "";

//            foreach (KeyValuePair<string, string> parameter in parameters)
//            {
//                values.Add(parameter.Key, string.IsNullOrEmpty(parameter.Value) ? "" : parameter.Value );

//            }

//            //Uri url = new Uri(host+path);
//            string url = host + "/" + path;
//            client.Credentials = credential;
//            client.QueryString = values;
//            byte[] result = null;
//            try
//            {
//                result = client.DownloadData(HttpUtility.UrlDecode(url));
//                strResult = Encoding.UTF8.GetString(result);
//                strResult = strResult.Replace("<", "&lt");
//                strResult = strResult.Replace(">", "&gt");                
//            }
//            catch (Exception ex)
//            {
//                //if (e is WebException && ((WebException)e).Status == WebExceptionStatus.ReceiveFailure)
//                //{
//                //string a = "";
//                //WebResponse errResp = ((WebException)ex).Response;
//                //using (Stream respStream = errResp.GetResponseStream())
//                //{
//                //    // read the error response
//                //    StreamReader reader = new StreamReader(respStream);
//                //    a = reader.ReadToEnd();
//                //}
//                ////}
//               throw new WebException("Webservice call failed. Url: " + url  + ",Error: " +  ex.Message +", inner exception :"+ex.InnerException);
//            } 
//            return strResult;
//        }


//        /*
//        I use this class for internal web services.  For external web services, you'll want
//        to put some logic in here to determine whether or not you should accept a certificate
//        or not if the domain name in the cert doesn't match the url you are accessing.
//        */
//        private static bool AcceptCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
//        {
//            return true;
//        }
//    }
//}