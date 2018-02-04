using Newtonsoft.Json;
using Sensi.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;

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

        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;

        public SensiConnection()
        {
            this._cookieJar = new CookieContainer();
            this._hubConnection = new HubConnection(_urlBase + "realtime", false);
            this._hubConnection.CookieContainer = this._cookieJar;
            this._hubProxy = this._hubConnection.CreateHubProxy("thermostat-v1");
        }
        public SensiConnection(string Username, string Password) : this()
        {
            this._username = Username;
            this._password = Password;
        }

        #region -- API Calls --

        private async Task<T> SendRequestAsync<T>(HttpMethod verb, string path, object req = null)
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

        public void Logout()
        {
            var result = SendRequestAsync<object>(HttpMethod.Delete, "api/authorize").Result;
            return;
        }

        public async Task<Contractor> GetContractor(int id)
        {
            var result = await SendRequestAsync<Contractor>(HttpMethod.Get, "api/contractors/" + id);
            return result;
        }


        public async Task<Dictionary<string, string>> GetTimeZones(string countryCode)
        {
            var result = await SendRequestAsync<Dictionary<string, string>>(HttpMethod.Get, "api/timezones/" + countryCode);
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




        #endregion

        #region -- Realtime Calls --

        public bool Ping()
        {
            var result = SendRequestAsync<PingResponse>(HttpMethod.Get, "realtime/ping").Result;
            return result?.Response == "pong";
        }

        public async Task<NegotiateResponse> Negotiate()
        {
            var result = await SendRequestAsync<NegotiateResponse>(HttpMethod.Get, "realtime/negotiate");
            this._token = result.ConnectionToken;
            return result;
        }

        public async Task BeginRealtime()
        {
            await _hubConnection.Start(new LongPollingTransport());
        }

        public async Task Subscribe(string icd) => await _hubProxy.Invoke("Subscribe", icd);
        public async Task Unsubscribe(string icd) => await _hubProxy.Invoke("Unsubscribe", icd);
        public async Task ChangeSetting(string icd, string feature, string value) => await _hubProxy.Invoke("ChangeSetting", icd, feature, value);
        public async Task SetHeat(string icd, int temp, TemperatureUnits unit) => await _hubProxy.Invoke("SetHeat", icd, temp, unit.GetTemperatureCode());
        public async Task SetAutoHeat(string icd, int temp, TemperatureUnits unit) => await _hubProxy.Invoke("SetAutoHeat", icd, temp, unit.GetTemperatureCode());
        public async Task SetCool(string icd, int temp, TemperatureUnits unit) => await _hubProxy.Invoke("SetCool", icd, temp, unit.GetTemperatureCode());
        public async Task SetAutoCool(string icd, int temp, TemperatureUnits unit) => await _hubProxy.Invoke("SetAutoCool", icd, temp, unit.GetTemperatureCode());
        public async Task SetFanMode(string icd, FanMode mode) => await _hubProxy.Invoke("SetFanMode", icd, mode.ToString());
        public async Task SetHoldMode(string icd, HoldMode mode) => await _hubProxy.Invoke("SetHoldMode", icd, mode.ToString());
        public async Task SetScheduleMode(string icd, ScheduleMode mode) => await _hubProxy.Invoke("SetScheduleMode", icd, mode.ToString());
        public async Task SetSystemMode(string icd, SystemMode mode) => await _hubProxy.Invoke("SetSystemMode", icd, mode.ToString());
        public async Task DeleteSchedule(string icd, int objectId) => await _hubProxy.Invoke("DeleteSchedule", icd, objectId);
        public async Task SaveSchedule(string icd, int objectId) => await _hubProxy.Invoke("SaveSchedule", icd, objectId);
        public async Task SetScheduleActive(string icd, int objectId) => await _hubProxy.Invoke("SetScheduleActive", icd, objectId);

        //Change Settings
        public async Task SetBacklightMode(string icd, Mode mode) => await _hubProxy.Invoke("ChangeSetting", icd, "ContinuousBacklight", mode.ToString());
        public async Task SetDisplayUnits(string icd, TemperatureUnits unit) => await _hubProxy.Invoke("ChangeSetting", icd, "Degrees", unit.GetTemperatureCode());
        public async Task SetHumidityDisplay(string icd, Mode mode) => await _hubProxy.Invoke("ChangeSetting", icd, "LocalHumidityDisplay", mode);
        public async Task SetTimeDisplay(string icd, Mode mode) => await _hubProxy.Invoke("ChangeSetting", icd, "LocalTimeDisplay", mode);
        public async Task SetKeypadLockout(string icd, Mode mode) => await _hubProxy.Invoke("ChangeSetting", icd, "KeypadLockout", mode);
        public async Task SetTemperatureOffset(string icd, int degrees) => await _hubProxy.Invoke("ChangeSetting", icd, "TemperatureOffset", degrees);
        public async Task SetHeatCycleRate(string icd, HeatCycleRate rate) => await _hubProxy.Invoke("ChangeSetting", icd, "HeatCycleRate", rate.ToString());
        public async Task SetComfortRecovery(string icd, Mode mode) => await _hubProxy.Invoke("ChangeSetting", icd, "ComfortRecovery", mode);

        #endregion
    }
}
