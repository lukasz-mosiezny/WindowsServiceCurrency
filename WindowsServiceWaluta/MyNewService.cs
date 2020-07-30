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
        }

        protected override void OnStart(string[] args)
        {
            // Change parameter to get any exchange rate for EUR.
            GetLatestValue("PLN");

            // Get value of PLN every hour.
            Timer timer = new Timer();
            timer.Interval = 3600000;
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
        }
        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            GetLatestValue("PLN");
        }
        public void GetLatestValue(string currency)
        {
            float value;
            string urlBase = "https://api.exchangeratesapi.io/latest?symbols=";

            // Get only 3 first characters of string. e.g PLN,CZK returns only PLN.
            if (currency.Length > 3)
            {
                currency = currency.Substring(0, 3);
            }
            string urlLatest = urlBase + currency;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    var json = wc.DownloadString(urlLatest);

                    var tmp = JObject.Parse(json);
                    //rates -> PLN -> Value
                    // Get first(only) value.
                    value = (float)tmp.Properties().First().First().FirstOrDefault();

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
