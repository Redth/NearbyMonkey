using System;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Messages;
using Android.OS;
using Android.Widget;
using NearbyMonkey.Core;
using NearbyMessage = Android.Gms.Nearby.Messages.Message;

[assembly: MetaData("com.google.android.nearby.messages.API_KEY", Value = "AIzaSyADhSj8jmW3Y7N4AwCfhkHS5wP3WFa-F9c")]

namespace NearbyMonkey
{
    [Activity(Label = "Nearby Monkey", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity 
        : global::Android.Support.V7.App.AppCompatActivity,
            GoogleApiClient.IOnConnectionFailedListener,
            GoogleApiClient.IConnectionCallbacks
    {
        string userId = string.Empty;
        GoogleApiClient googleApiClient;
        EmotionsMessageListener emotionsMsgListener;
        NearbyMessage publishedMessage;
        MessagesAdapter adapter;
        TaskCompletionSource<bool> tcsConnected = new TaskCompletionSource<bool>();
        ListView listView;
        Button buttonPublish;
        Spinner spinnerName;
        Spinner spinnerEmotion;
        Spinner spinnerSpecies;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Make our top bar a bit more appealing
            SupportActionBar.SetIcon(Resource.Mipmap.Icon);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayUseLogoEnabled(true);
            SupportActionBar.SetLogo(Resource.Mipmap.Icon);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Wire up the UI
            listView = FindViewById<ListView>(Resource.Id.listView);
            buttonPublish = FindViewById<Button>(Resource.Id.buttonPublish);
            spinnerName = FindViewById<Spinner>(Resource.Id.spinnerName);
            spinnerSpecies = FindViewById<Spinner>(Resource.Id.spinnerSpecies);
            spinnerEmotion = FindViewById<Spinner>(Resource.Id.spinnerEmotion);

            // Setup our spinner controls
            SetupSpinner(spinnerName, Values.Names);
            SetupSpinner(spinnerSpecies, Values.Species);
            SetupSpinner(spinnerEmotion, Values.Emotions);

            // Create a new adapter to list other messages
            adapter = new MessagesAdapter
            {
                Parent = this
            };
            listView.Adapter = adapter;

            // Generate a unique user id, one time, and save it for future use
            var pref = GetPreferences(Android.Content.FileCreationMode.Private);
            if (!pref.Contains("userid"))
                pref.Edit().PutString("userid", Guid.NewGuid().ToString()).Commit();
            userId = pref.GetString("userid", "");

            // When the publish button is clicked, update our published Message
            buttonPublish.Click += async delegate
            {

                buttonPublish.Enabled = false;

                try
                {
                    await Publish();
                }
                catch
                {
                    Toast.MakeText(this, "Update Failed", ToastLength.Short).Show();
                }

                buttonPublish.Enabled = true;
            };

            // Setup our GoogleApiClient connection
            googleApiClient = new GoogleApiClient.Builder(this)
                .AddApi(NearbyClass.MessagesApi)
                .EnableAutoManage(this, this)
                .AddConnectionCallbacks(this)
                .Build();

            // Create a new listener for Nearby messages
            emotionsMsgListener = new EmotionsMessageListener
            {
                OnFoundHandler = msg =>
                {
                    LogMessage ("Found Msg: {0}", msg);
                    adapter.Messages.Add(msg);
                    RunOnUiThread(() => adapter.NotifyDataSetChanged());
                },
                OnLostHandler = msg =>
                {
                    LogMessage ("Lost Msg: {0}", msg);
                    adapter.Messages.Remove(msg);
                    RunOnUiThread(() => adapter.NotifyDataSetChanged());
                }
            };

            // Request the required permissions
            await RequestNearbyPermissionsAsync();
        }

        async Task RequestNearbyPermissionsAsync()
        {
            // Wait until Google Play Services is connected
            if (await IsConnected())
            {

                // Request permissions
                var permStatus = await NearbyClass.Messages.GetPermissionStatusAsync(googleApiClient);

                // If our request failed, we'll need to see if there's a way to resolve
                if (!permStatus.IsSuccess)
                {
                    LogMessage("Nearby permission request failed...");

                    // If we have a resolution for requesting permissions, start it
                    if (permStatus.HasResolution)
                    {
                        LogMessage("Has resolution for Nearby permission request failure...");
                        permStatus.StartResolutionForResult(this, 1001);
                    }
                    else
                    {
                        // No resolution, just abandon the app
                        Toast.MakeText(this, "Nearby Messaging Disabled, exiting App", ToastLength.Long).Show();
                        Finish();
                    }
                }
                else
                {
                    // Permission request succeeded, continue with subscribing and publishing messages
                    await Subscribe();
                    await Publish();
                }
            }
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // We came back from the resolution to request permissions
            if (requestCode == 1001)
            {
                // Request them again
                await RequestNearbyPermissionsAsync();
            }
        }

        // We want to randomize the initial selections
        Random RANDOM = new Random((int)DateTime.Now.Ticks);

        void SetupSpinner(Spinner spinner, string[] entries)
        {
            // Create a new adapter for the spinner
            var spinAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, Android.Resource.Id.Text1, entries);
            spinner.Adapter = spinAdapter;

            // Now select a random item
            var selectIndex = RANDOM.Next(0, spinAdapter.Count - 1);
            spinner.SetSelection(selectIndex);
        }

        public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
        {
            // Connection failed to Google Play services, set our result false
            tcsConnected.TrySetResult(false);

            // There's no way to recover at this point, so let the user know, and exit
            Toast.MakeText(this, "Failed to connect to Google Play Services", ToastLength.Long).Show();
            Finish();
        }

        public void OnConnected(Bundle connectionHint)
        {
            // Google Play services connected, set our completion source to true!
            tcsConnected.TrySetResult(true);
        }

        public void OnConnectionSuspended(int cause)
        {
            // Not doing anything currently when connection is suspended
        }

        // Checks the state of our Google Play Services connection
        // Returns the awaitable task for the connection
        Task<bool> IsConnected()
        {
            return tcsConnected.Task;
        }

        // Subscribes to the Messages API
        async Task Subscribe()
        {
            // Wait for a connection to GPS
            if (!await IsConnected())
                return;

            var status = await NearbyClass.Messages.SubscribeAsync(googleApiClient, emotionsMsgListener);
            if (!status.IsSuccess)
                LogMessage(status.StatusMessage);
        }

        // Unsubscribes from the Messages API
        async Task Unsubscribe()
        {
            // Wait for a connection to GPS (although this should always be the case if it's called)
            if (!await IsConnected())
                return;
            
            var status = await NearbyClass.Messages.UnsubscribeAsync(googleApiClient, emotionsMsgListener);
            if (!status.IsSuccess)
                LogMessage(status.StatusMessage);
        }

        // Publishes a new Message to the Nearby API
        async Task Publish()
        {
            // Wait for connection
            if (!await IsConnected())
                return;

            // Create new Nearby message to publish with the spinner choices
            var emotionMessage = new EmotionMessage
            {
                UserId = userId,
                Name = spinnerName.SelectedItem.ToString(),
                Species = spinnerSpecies.SelectedItem.ToString(),
                Emotion = spinnerEmotion.SelectedItem.ToString()
            };

            // Remove any existing messages for this user from our list
            // Add the new message and update the dataset
            adapter.Messages.RemoveAll(m => m.UserId == userId);
            adapter.Messages.Add(emotionMessage);
            adapter.NotifyDataSetChanged();

            // If we already published a message, unpublish it first
            if (publishedMessage != null)
                await Unpublish();

            // Create a new nearby message with our serialized object
            publishedMessage = new NearbyMessage(emotionMessage.Serialize());

            // Publish our new message
            var status = await NearbyClass.Messages.PublishAsync(googleApiClient, publishedMessage);
            if (!status.IsSuccess)
                LogMessage(status.StatusMessage);
        }

        // Unpublishes a message from the Nearby api
        async Task Unpublish()
        {
            // Wait for GPS connection
            if (!await IsConnected())
                return;

            // If we actually published a message, unpublish it
            if (publishedMessage != null)
            {
                var status = await NearbyClass.Messages.Unpublish(googleApiClient, publishedMessage);
                if (!status.Status.IsSuccess)
                    LogMessage(status.Status.StatusMessage);
            }
        }

        protected override void OnStop()
        {
            // Unpublish messages and then when it's done, unsubscribe
            Unpublish().ContinueWith(t => Unsubscribe());

            base.OnStop();
        }

        void LogMessage (string format, params object[] args)
        {
            Android.Util.Log.Debug("NEARBY-MONKEY", string.Format(format, args));
        }
    }
}
