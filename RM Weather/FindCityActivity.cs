using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Newtonsoft.Json;

namespace RM_Weather
{
    [Activity(Label = "Search for City")]
    public class FindCityActivity : Activity
    {

        private TextView FoundCount;
        private string Cityquery;
        private ListView list;
        private List<MyNamespace.List> ObjectsFound;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SearchCity);
            EditText cityEditText = FindViewById<EditText>(Resource.Id.CityEntry);
            cityEditText.EditorAction += EditorActionHandler;
            FoundCount = FindViewById<TextView>(Resource.Id.CityCountFound);
            list = FindViewById<ListView>(Resource.Id.CityList);

            list.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {

                string selectedFromList = list.GetItemAtPosition(e.Position).ToString();
                string tempSubstring = selectedFromList.Substring(0, 1);
                int index = Int32.Parse(tempSubstring) - 1;
                Intent myIntent = new Intent(this, typeof(MainActivity));
                myIntent.PutExtra("object", JsonConvert.SerializeObject(ObjectsFound[index]));
                SetResult(Result.Ok, myIntent);
                Finish();

            };

        }

        private void EditorActionHandler(object sender, TextView.EditorActionEventArgs e)
        {
            e.Handled = false;
            if (e.ActionId == ImeAction.Search)
            {
                Cityquery = FindViewById<EditText>(Resource.Id.CityEntry).Text;
                CitySearchSerivce();
                e.Handled = true;
            }
        }

        private async void CitySearchSerivce()
        {
            string key = "e33cd5a033ded6b870ec49734dcde1da";
            Cityquery.Replace(" ", "%20");
            string queryString = "http://api.openweathermap.org/data/2.5/find?mode=json&type=like&q="
                                 + Cityquery + "&units=metric" + "&appid=" + key;

            MyNamespace.RootObject dane;
            List<string> table = new List<string>();

            using (var httpClient = new HttpClient())
            {
                FoundCount.Text = "Pending...";

                var json = await httpClient.GetStringAsync(queryString);
                dane = JsonConvert.DeserializeObject<MyNamespace.RootObject>(json);

            }

            ObjectsFound = dane.list;
            FoundCount.Text = "Found: " + dane.count + " results";
            int index = 1;
            foreach (var dana in dane.list)
            {
                table.Add(index + ". " + "Country: " + dana.sys.country + " Name: " + dana.name + "\nLat/Lon: " + dana.coord.lat + " " + dana.coord.lon + "\nTemp: " + dana.main.temp + " C");
                ++index;
            }
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleExpandableListItem1, table);
            list.Adapter = adapter;

        }




    }
}