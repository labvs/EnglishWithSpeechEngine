using System;
using System.IO;
using System.Net;
using System.Text;

namespace SteamAuth
{
    public static class WebRequests
    {
        public static string SendGetRequest(string url, CookieContainer cookie = null, int timeout = 0)
        {
            try
            {
                string content = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (cookie != null)
                    request.CookieContainer = cookie;

               

                request.ProtocolVersion = HttpVersion.Version10;
                request.Timeout = timeout != 0 ? timeout : 60000;
                request.ContinueTimeout = 50000;
                request.ReadWriteTimeout = 50000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stream = new StreamReader(response.GetResponseStream());
                    content = stream.ReadToEnd();
                    stream.Close();
                }
                return content;
            }
            catch (WebException e)
            {
                try
                {
                    string resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    //MyLog.Logger.Warn($"Ошбка при загрузке страницы = {url} ошибка = {resp}");
                    return resp;
                }
                catch (Exception)
                {
                    return "";
                }

                return "";
            }
        }

        public static string SendPostRequest(string command, string url, CookieContainer cookie = null,
            string referer = "", int timeout = 0)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = cookie;

                byte[] data = Encoding.UTF8.GetBytes(command);
                request.Method = "POST";

                request.Timeout = timeout > 0 ? timeout : 60000;

                request.ContentType = "audio/x-wav";
                request.ContentLength = data.Length;

                if (referer != "")
                    request.Referer = referer;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                WebResponse response = (HttpWebResponse)request.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return responseString;
            }
            catch (WebException e)
            {
                try
                {
                    string resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                   
                    return resp;
                }
                catch (Exception)
                {

                }
                return "";
            }
        }

        public static string SendPostRequestWithErrors(string command, string url, CookieContainer cookie = null,
            string referer = "", int timeout = 0)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = cookie;

                byte[] data = Encoding.UTF8.GetBytes(command);
                request.Method = "POST";

                request.Timeout = timeout > 0 ? timeout : 60000;

                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                request.ContentLength = data.Length;

                if (referer != "")
                    request.Referer = referer;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                WebResponse response = (HttpWebResponse)request.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return responseString;
            }
            catch (WebException e)
            {
                try
                {
                    string resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                   
                    return resp;
                }
                catch (Exception)
                {

                }
                return "";
            }
        }
    }
}
