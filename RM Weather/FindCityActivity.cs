using System;
using System.Collections.Generic;
using System.Net.Http;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.InputMethods;
using Android.Widget;
using Newtonsoft.Json;

namespace RM_Weather
{
    [Activity(Label = "Search by City")]
    public class FindCityActivity : Activity
    {

        private TextView _foundCount;
        private string _cityquery;
        private ListView _list;
        private List<MyNamespace.List> _objectsFound;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SearchCity);
            EditText cityEditText = FindViewById<EditText>(Resource.Id.CityEntry);
            cityEditText.EditorAction += EditorActionHandler;
            _foundCount = FindViewById<TextView>(Resource.Id.CityCountFound);
            _list = FindViewById<ListView>(Resource.Id.CityList);

            _list.ItemClick += (sender, e) =>
            {
                string selectedFromList = _list.GetItemAtPosition(e.Position).ToString();
                string tempSubstring = selectedFromList.Substring(0, 1);
                int index = Int32.Parse(tempSubstring) - 1;
                Intent myIntent = new Intent(this, typeof(MainActivity));
                myIntent.PutExtra("object", _objectsFound[index].id);  //sent data to previous activity
                SetResult(Result.Ok, myIntent);
                Finish();
            };
        }

        private void EditorActionHandler(object sender, TextView.EditorActionEventArgs e)
        {
            e.Handled = false;
            if (e.ActionId == ImeAction.Search)
            {
                _cityquery = FindViewById<EditText>(Resource.Id.CityEntry).Text;
                CitySearchSerivce();
                e.Handled = true;
            }
        }

        private async void CitySearchSerivce()
        {
            string key = "e33cd5a033ded6b870ec49734dcde1da";
            _cityquery.Replace(" ", "%20");
            string queryString = "http://api.openweathermap.org/data/2.5/find?mode=json&type=like&q="
                                 + _cityquery + "&cnt=10" + "&units=metric" + "&appid=" + key;

            MyNamespace.RootObject dane;
            List<string> table = new List<string>();

            using (var httpClient = new HttpClient())
            {
                _foundCount.Text = "Pending...";
                var json = await httpClient.GetStringAsync(queryString);
                dane = JsonConvert.DeserializeObject<MyNamespace.RootObject>(json);
            }

            _objectsFound = dane.list;
            _foundCount.Text = "Found: " + dane.count + " results";
            int index = 1;
            foreach (var dana in dane.list)
            {
                table.Add(index + ". " + "Country: " + dana.sys.country + " Name: " + dana.name + "\nLat/Lon: " + dana.coord.lat + " " + dana.coord.lon + "\nTemp: " + dana.main.temp + " C");
                ++index;
            }
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleExpandableListItem1, table);
            _list.Adapter = adapter;
        }
    }
}