using Newtonsoft.Json;
using Sensi.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sensi
{
    public class SensiConnection
    {
        private const string _urlBase = "https://bus-serv.sensicomfort.com/";
        private CookieContainer _cookieJar;
        private string _token;
        private DateTime? _tokenExpiration;

        private string _username;
        private string _password;

        public SensiConnection()
        {
            this._cookieJar = new CookieContainer();
        }
        public SensiConnection(string Username, string Password) 
        {
            this._cookieJar = new CookieContainer();
            this._username = Username;
            this._password = Password;
        }

        private async Task<T> SendRequestAsync<T>(HttpMethod verb, string path, object req=null)
        {
            byte[] byteArray = new byte[0];

            if (req != null)
                byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(_urlBase + path);
            webRequest.Accept = "application/json; version=1, */*; q=0.01";
            webRequest.CookieContainer = this._cookieJar;
            webRequest.Method = verb.Method;
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = byteArray.Length;
            webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");

            if (verb != HttpMethod.Get && req != null)
            {
                var requestTask = await webRequest.GetRequestStreamAsync();
                using (Stream dataStream = requestTask)
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
            }

            using (var response = await webRequest.GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var responseString = reader.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<T>(responseString);
                    
                    return result;
                }
            }

        }

        public async Task<AuthorizeResponse> Authorize()
        {
            var args = new { UserName = this._username, Password = this._password };
            var result = await SendRequestAsync<AuthorizeResponse>(HttpMethod.Post, "api/authorize", args);
            if (result != null)
            {
                this._token = result.Token;
                this._tokenExpiration = result.Expires;
            }
            return result;
        }
        public bool Ping()
        {
            var result = SendRequestAsync<PingResponse>(HttpMethod.Get, "realtime/ping").Result;
            return result?.Response == "pong";
        }
        public void Logout()
        {
            var result = SendRequestAsync<object>(HttpMethod.Delete, "api/authorize").Result;
            return;
        }

        public async Task<Contractor> GetContractor(int id)
        {
            var result = await SendRequestAsync<Contractor>(HttpMethod.Get, "api/contractors/"+id);
            return result;
        }


        public async Task<Dictionary<string,string>> GetTimeZones(string countryCode)
        {
            var result = await SendRequestAsync<Dictionary<string,string>>(HttpMethod.Get, "api/timezones/"+countryCode);
            return result;
        }

        public async Task<IEnumerable<Thermostat>> ListThermostats()
        {
            var result = await SendRequestAsync<IEnumerable<Thermostat>>(HttpMethod.Get, "api/thermostats");
            return result;
        }

        public async Task<WeatherResponse> GetWeatherAsync(string icd)
        {
            var result = await SendRequestAsync<WeatherResponse>(HttpMethod.Get, "api/weather/" + icd);
            return result;
        }


        public async Task<NegotiateResponse> Negotiate()
        {
            var result = await SendRequestAsync<NegotiateResponse>(HttpMethod.Get, "realtime/negotiate");
            this._token = result.ConnectionToken;
            return result;
        }


    }
}
