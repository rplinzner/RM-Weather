using System;
using System.Globalization;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Widget;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

//Weather APP by Rafal Plinzner And Michal Bialecki
namespace RM_Weather
{
    [Activity(Label = "RM Weather by Rafal and Michal",
        MainLauncher = true)]
    public class MainActivity : Activity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        readonly string[] _permissionsLocation =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        const int RequestLocationId = 0;

        private Position _location;
        //private int _objectToShow;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button cityButton = FindViewById<Button>(Resource.Id.SearchCityButton);
            Button locationButton = FindViewById<Button>(Resource.Id.SearchLocationButton);

            cityButton.Click += delegate
            {
                Intent openActivity = new Intent(this, typeof(FindCityActivity));
                StartActivityForResult(openActivity, 0);
            };

            locationButton.Click += async delegate
            {
                await TryGetLocationAsync();
                var obj = await GetLatLonResponse.LatLonResponseTask(_location);
                LoadData(obj);
            };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok)
            {
                LoadData(data.GetIntExtra("object", 0));
            }
        }

        private async void LoadData(int id)
        {
            if (id == 0) return;
            var preciseObj = await GetPreciseResponse.CitySearchSerivce(id);

            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime sunrise = time.AddSeconds(preciseObj.sys.sunrise);
            DateTime sunset = time.AddSeconds(preciseObj.sys.sunset);

            SetWeatherIcion(preciseObj.weather[0].id, sunrise, sunset);
            FindViewById<TextView>(Resource.Id.locationText).Text = preciseObj.name + ", " + preciseObj.sys.country;
            FindViewById<TextView>(Resource.Id.tempText).Text =
                preciseObj.main.temp.ToString(CultureInfo.InvariantCulture) + "°C";
            FindViewById<TextView>(Resource.Id.windText).Text =
                preciseObj.wind.speed.ToString(CultureInfo.InvariantCulture) + " km/h";
            FindViewById<TextView>(Resource.Id.humidityText).Text =
                preciseObj.main.humidity.ToString(CultureInfo.InvariantCulture) + "%";
            FindViewById<TextView>(Resource.Id.visibilityText).Text = preciseObj.weather[0].main;

            FindViewById<TextView>(Resource.Id.sunriseText).Text =
                sunrise.ToString(CultureInfo.InvariantCulture) + " UTC";
            FindViewById<TextView>(Resource.Id.sunsetText).Text =
                sunset.ToString(CultureInfo.InvariantCulture) + " UTC";

            FindViewById<TextView>(Resource.Id.LatLonText).Text =
                preciseObj.coord.lat.ToString(CultureInfo.InvariantCulture) + " " +
                preciseObj.coord.lon.ToString(CultureInfo.InvariantCulture);
        }

        private void SetWeatherIcion(int actualId, DateTime sunrise, DateTime sunset)
        {
            int id = actualId / 100;
            String icon = "";
            if (actualId == 800)
            {
                DateTime currentTime = DateTime.UtcNow;
                if (currentTime >= sunrise && currentTime < sunset)
                {
                    icon = Resources.GetString(Resource.String.weather_sunny);
                }
                else
                {
                    icon = Resources.GetString(Resource.String.weather_clear_night);
                }
            }
            else
            {
                switch (id)
                {
                    case 2:
                        icon = Resources.GetString(Resource.String.weather_thunder);
                        break;
                    case 3:
                        icon = Resources.GetString(Resource.String.weather_drizzle);
                        break;
                    case 7:
                        icon = Resources.GetString(Resource.String.weather_foggy);
                        break;
                    case 8:
                        icon = Resources.GetString(Resource.String.weather_cloudy);
                        break;
                    case 6:
                        icon = Resources.GetString(Resource.String.weather_snowy);
                        break;
                    case 5:
                        icon = Resources.GetString(Resource.String.weather_rainy);
                        break;
                }
            }
            var font = Typeface.CreateFromAsset(Assets, "weathericons-regular-webfont.ttf");
            TextView weatherIcon = FindViewById<TextView>(Resource.Id.weather_icon);
            weatherIcon.Typeface = font;
            weatherIcon.Text = icon;
        }

        async Task TryGetLocationAsync()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                await GetLocationAsync();
                return;
            }
            await GetLocationPermissionAsync();
        }

        async Task GetLocationPermissionAsync()
        {
            //Check to see if any permission in our group is available, if one, then all are
            const string permission = Manifest.Permission.AccessFineLocation;
            if (CheckSelfPermission(permission) == (int)Permission.Granted)
            {
                await GetLocationAsync();
                return;
            }
            //need to request permission
            if (ShouldShowRequestPermissionRationale(permission))
            {
                //Explain to the user why we need to read the contacts
                AlertDialog.Builder dialog = new AlertDialog.Builder(this, Resource.Style.AppCompatAlertDialogStyle);
                AlertDialog alert = dialog.Create();
                alert.SetTitle("Warning");
                alert.SetMessage("Location access is required to show weather. Grant permissions and search one more time");
                alert.SetButton("OK", (c, ev) =>
                {
                    RequestPermissions(_permissionsLocation, RequestLocationId);
                });

                alert.Show();
                return;
            }
            //Finally request permissions with the list of permissions and Id
            RequestPermissions(_permissionsLocation, RequestLocationId);
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestLocationId:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            //Permission granted
                            Toast.MakeText(this, "Location permission is available, getting lat/long.", ToastLength.Short).Show();
                            await GetLocationAsync();
                        }
                        else
                        {
                            //Permission Denied :(
                            //Disabling location functionality
                            Toast.MakeText(this, "Location permission is denied.", ToastLength.Short).Show();
                        }
                    }
                    break;
            }
        }
        async Task GetLocationAsync()
        {
            Toast.MakeText(this, "Getting Location", ToastLength.Short).Show();
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 100;
                TimeSpan ts = new TimeSpan(20000);
                var position = await locator.GetPositionAsync(ts);
                Toast.MakeText(this, $"Lat: {position.Latitude}  Long: {position.Longitude}", ToastLength.Short).Show();
                _location = position;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Unable to get location: " + ex, ToastLength.Short).Show();
            }
        }
    }
}