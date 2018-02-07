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
using System.Diagnostics;
using Sensi.EventArgs;

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

        public event EventHandler<ThermostatOnlineEventArgs> ThermostatOnline;
        public event EventHandler<EnvironmentalControlsUpdatedEventArgs> EnvironmentalControlsUpdated;
        public event EventHandler<OperationalStatusUpdatedEventArgs> OperationalStatusUpdated;
        public event EventHandler<CapabilitiesUpdatedEventArgs> CapabilitiesUpdated;
        public event EventHandler<SettingsUpdatedEventArgs> SettingsUpdated;
        public event EventHandler<ProductUpdatedEventArgs> ProductUpdated;
        
        public SensiConnection()
        {
            Trace.WriteLine("SensiConnection ctor");

            this._cookieJar = new CookieContainer();
            this._hubConnection = new HubConnection(_urlBase + "realtime", false);
            this._hubConnection.CookieContainer = this._cookieJar;
            this._hubProxy = this._hubConnection.CreateHubProxy("thermostat-v1");

            this._hubConnection.StateChanged += (state) =>
            {
                Debug.WriteLine($"State Changed from {state.OldState} to {state.NewState}");
            };
            
            this._hubConnection.ConnectionSlow += () => { Debug.WriteLine("Connection Slow"); };
            this._hubConnection.Reconnected += () => { Debug.WriteLine("Reconnected"); };
            this._hubConnection.Reconnecting += () => { Debug.WriteLine("Reconnecting"); };
            this._hubConnection.Error += (ex) => { Trace.WriteLine(ex, "HubConnection Error");

                if (ex is System.Net.WebException && ex.Message == "The remote server returned an error: (401) Unauthorized.")
                {
                    try
                    {
                        Trace.WriteLine("Attempting Reauth");
                        this.Authorize().Wait();
                        this.Negotiate().Wait();
                    } catch (Exception ex2) { Trace.WriteLine(ex2); }
                }
            };
            
            this._hubConnection.Received += (data) => { Debug.WriteLine("Data Received"); };

            this._hubProxy.On<object>("initalized", (data) =>
             {
                 Trace.WriteLine(data, "Initalized");
             });

            this._hubProxy.On<object, object>("online", (icd, data) =>
             {
                 Trace.WriteLine(Environment.NewLine + data, $"Received Online Message for ICD [{icd}]");

                 try
                 {
                     var msg = JsonConvert.DeserializeObject<OnlineResponse>(data.ToString());
                     var args = (ThermostatOnlineEventArgs)msg;
                     args.Icd = icd.ToString();

                     ThermostatOnline?.Invoke(this, args);
                 }
                 catch (Exception ex)
                 {
                     Trace.WriteLine(ex);
                 }
             });

            this._hubProxy.On<object, object>("update", (icd, data) =>
            {
                Trace.WriteLine(Environment.NewLine + data, $"Received Update Message for ICD [{icd}]");
                
                OnlineResponse msg = null;

                try
                {
                    msg = JsonConvert.DeserializeObject<OnlineResponse>(data.ToString());
                }
                catch (Exception ex) { Trace.WriteLine(ex); }

                if (msg != null)
                {
                    if (msg != null && msg.OperationalStatus != null)
                        OperationalStatusUpdated?.Invoke(this, new OperationalStatusUpdatedEventArgs { Icd = icd.ToString(), OperationalStatus = msg.OperationalStatus });

                    if (msg != null && msg.EnvironmentControls != null)
                        EnvironmentalControlsUpdated?.Invoke(this, new EnvironmentalControlsUpdatedEventArgs { Icd = icd.ToString(), EnvironmentControls = msg.EnvironmentControls });

                    if (msg != null && msg.Capabilities != null)
                        CapabilitiesUpdated?.Invoke(this, new CapabilitiesUpdatedEventArgs { Icd = icd.ToString(), Capabilities = msg.Capabilities });

                    if (msg != null && msg.Settings != null)
                        SettingsUpdated?.Invoke(this, new SettingsUpdatedEventArgs { Icd = icd.ToString(), Settings = msg.Settings });

                    if (msg != null && msg.Product != null)
                        ProductUpdated?.Invoke(this, new ProductUpdatedEventArgs { Icd = icd.ToString(), Product = msg.Product });
                }

            });
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
            Debug.WriteLine("Authorizing");
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

        public async Task<IEnumerable<ThermostatResponse>> ListThermostats()
        {
            var result = await SendRequestAsync<IEnumerable<ThermostatResponse>>(HttpMethod.Get, "api/thermostats");
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
            Debug.WriteLine("Negotiate");
            var result = await SendRequestAsync<NegotiateResponse>(HttpMethod.Get, "realtime/negotiate");
            this._token = result.ConnectionToken;
            return result;
        }

        public async Task<bool> BeginRealtime()
        {
            Trace.WriteLine("Starting Connect");
            await this.Authorize();
            await this.Negotiate();

            bool isConnected = false;
            await _hubConnection.Start(new LongPollingTransport());

            if (_hubConnection.State == ConnectionState.Connected)
            {
                isConnected = true;
                //_hubConnection.Closed += _hubConnection_Closed;
                _hubConnection.Closed += () => { Debug.WriteLine("HubConnection Closed"); };
            }

            Trace.Write("HubConnection State is "+_hubConnection.State);
            return isConnected;
        }
        /*
        private async void _hubConnection_Closed()
        {
            TimeSpan retryDuration = TimeSpan.FromSeconds(30);

            while (DateTime.UtcNow < DateTime.UtcNow.Add(retryDuration))
            {
                Trace.WriteLine("Attempting Reconnect");
                bool connected = await BeginRealtime();
                if (connected)
                    return;
            }
        }
        */
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

        #region -- Realtime Events -- 

        #endregion
    }
}
