﻿using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Geolocator.Abstractions;

namespace RM_Weather
{
    class GetLatLonResponse
    {
        
        public static async Task<int> LatLonResponseTask(Position position)
        {
            if (position != null)
            {
                string key = "API_KEY_HERE";
                string queryString = "http://api.openweathermap.org/data/2.5/weather?lat="
                                     + position.Latitude + "&lon=" + position.Longitude + "&units=metric" +
                                     "&appid=" + key;

                PreciseNamespace.RootObject dane;
                using (var httpClient = new HttpClient())
                {
                    var json = await httpClient.GetStringAsync(queryString);
                    dane = JsonConvert.DeserializeObject<PreciseNamespace.RootObject>(json);
                }
                return dane.id;
            }
            return 0;
        }
    }
}
