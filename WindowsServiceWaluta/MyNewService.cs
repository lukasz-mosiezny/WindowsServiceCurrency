using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json.Linq;

namespace WindowsServiceWaluta
{
    public partial class MyNewService : ServiceBase
    {
        private int eventID = 1;
        private Timer oneHourTimer = new Timer();

        public MyNewService()
        {
            InitializeComponent();
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("MySourceC"))
            {
                EventLog.CreateEventSource("MySourceC", "CurrencyLog");
            }
            eventLog1.Source = "MySourceC";
            eventLog1.Log = "CurrencyLog";
            
            oneHourTimer.Interval = 3600000;
            oneHourTimer.Elapsed += new ElapsedEventHandler(this.OnOneHourTimer);
        }

        protected override void OnStart(string[] args)
        {
            // Change parameter to get any exchange rate for EUR.
            GetLatestValue("PLN");
            
            oneHourTimer.Start();
        }
        public void OnOneHourTimer(object sender, ElapsedEventArgs args)
        {
            GetLatestValue("PLN");
        }
        // Returns url of single currency.
        public string GetCurrencyURL(string currency)
        {
            string urlBase = "https://api.exchangeratesapi.io/latest?symbols=";

            // Get only 3 first characters of string. e.g PLN,CZK returns only PLN.
            if (currency.Length > 3)
            {
                currency = currency.Substring(0, 3);
            }
            return urlBase + currency;
        }
        public void GetLatestValue(string currency)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    var url = GetCurrencyURL(currency);
                    var json = wc.DownloadString(url);

                    var tmp = JObject.Parse(json);
                    //rates -> PLN -> Value
                    // Get first(only) value.
                    float value = (float)tmp.Properties().First().First().FirstOrDefault();

                    eventLog1.WriteEntry("EUR/" + currency + " rate: " + value.ToString(), EventLogEntryType.Information, eventID++);
                }
                catch (Exception e)
                {
                    // Probably wrong parameter in GetLatestValue function.
                    eventLog1.WriteEntry(e.Message);
                }
            }
        }
    }
}
