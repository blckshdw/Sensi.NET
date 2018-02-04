using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [TestMethod]
        public void ValidateUnitTestConfig()
        {
            Assert.IsNotNull(this.Username);
            Assert.IsNotNull(this.Password);
        }

        [TestMethod]
        public void Authorize()
        {

            SensiConnection cn = new SensiConnection(this.Username, this.Password);
            var response = cn.Authorize().Result;

            Assert.IsNotNull(response);
        }
    }
}
