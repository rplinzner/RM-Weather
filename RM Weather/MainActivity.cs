using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;

namespace RM_Weather
{
    [Activity(Label = "RM Weather by Rafal and Michal",
        Theme = "@android:style/Theme.Material.Light",
        MainLauncher = true)]
    public class MainActivity : Activity
    {
        private MyNamespace.List ObjectToShow;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button cityButton = FindViewById<Button>(Resource.Id.SearchCityButton);
           

            cityButton.Click += delegate
            {
                Intent openActivity = new Intent(this, typeof(FindCityActivity));
                StartActivityForResult(openActivity, 0);
            };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok)
            {
                ObjectToShow = JsonConvert.DeserializeObject<MyNamespace.List>(data.GetStringExtra("object"));
                LoadData(ObjectToShow);
            }
        }

        private async void LoadData(MyNamespace.List Object)
        {
            if (Object != null)
            {
                PreciseNamespace.RootObject PreciseData = await GetPreciseResponse.CitySearchSerivce(Object.id);
                Toast.MakeText(this, "Data Retrieved Succesfully", ToastLength.Long).Show();
                FindViewById<TextView>(Resource.Id.locationText).Text = Object.name;
                FindViewById<TextView>(Resource.Id.tempText).Text = Object.main.temp.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.windText).Text = Object.wind.speed.ToString() + " km/h";
                FindViewById<TextView>(Resource.Id.humidityText).Text = Object.main.humidity.ToString() + "%";
                FindViewById<TextView>(Resource.Id.visibilityText).Text = Object.weather[0].main ;

                DateTime time = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                DateTime sunrise = time.AddSeconds((double)PreciseData.sys.sunrise);
                DateTime sunset = time.AddSeconds((double)PreciseData.sys.sunset);
                
                FindViewById<TextView>(Resource.Id.sunriseText).Text = sunrise.ToString() + " UTC";
                FindViewById<TextView>(Resource.Id.sunsetText).Text = sunset.ToString() + " UTC";

            }
            else
            {
                Toast.MakeText(this, "Data not retrieved :(", ToastLength.Long).Show();
            }
        }

        
    }
}