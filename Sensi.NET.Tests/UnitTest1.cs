using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Sensi.NET.Tests
{
    [TestClass]
    public class UnitTest1
    {
        public string Username
        {
            get
            {
                AppSettingsReader appSettingsReader = new AppSettingsReader();
                return (string)appSettingsReader.GetValue("Username", typeof(string));
            }
        }
        public string Password
        {
            get
            {
                AppSettingsReader appSettingsReader = new AppSettingsReader();
                return (string)appSettingsReader.GetValue("Password", typeof(string));
            }
        }
        SensiConnection cn;

        [TestMethod]
        public void ValidateUnitTestConfig()
        {
            Assert.IsNotNull(this.Username);
            Assert.IsNotNull(this.Password);
        }

        [TestMethod]
        public void Authorize()
        {

            cn = new SensiConnection(this.Username, this.Password);
            var response = cn.Authorize().Result;

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public void Negotiate()
        {
            Authorize();
            var result = cn.Negotiate().Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Logout()
        {
            Authorize();
            cn.Logout();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Ping()
        {
            Authorize();
            var result = cn.Ping();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contractor()
        {
            Authorize();
            var result = cn.GetContractor(1).Result;

            Assert.IsNotNull(result);

        }

        [TestMethod]
        public void GetTimeZones()
        {
            Authorize();
            var result = cn.GetTimeZones("CA").Result;

            Assert.IsNotNull(result);

        }

        [TestMethod]
        public void ListThermostats()
        {
            Authorize();
            Negotiate();
            var result = cn.ListThermostats().Result;

            Assert.IsTrue(result.Count() > 0);
        }

        [TestMethod]
        public void GetWeather()
        {
            Authorize();
            
            var thermostats = cn.ListThermostats().Result;
            Assert.IsTrue(thermostats.Count() > 0);

            var icd = thermostats.First().ICD;
            var weather = cn.GetWeatherAsync(icd).Result;

            Assert.IsNotNull(weather);

        }
    }
}
