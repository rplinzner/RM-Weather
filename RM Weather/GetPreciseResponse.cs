﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace RM_Weather
{
    public class GetPreciseResponse
    {
        public static async Task<PreciseNamespace.RootObject> CitySearchSerivce(int id)
        {
            string key = "e33cd5a033ded6b870ec49734dcde1da";

            string queryString = "http://api.openweathermap.org/data/2.5/weather?id="
                                 + id.ToString() + "&units=metric" + "&appid=" + key;

            PreciseNamespace.RootObject dane;
            List<string> table = new List<string>();
            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync(queryString);
                dane = JsonConvert.DeserializeObject<PreciseNamespace.RootObject>(json);

            }

            return dane;

        }
    }
}