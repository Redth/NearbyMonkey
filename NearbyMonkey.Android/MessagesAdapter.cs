using System.Collections.Generic;
using Android.App;
using Android.Widget;
using NearbyMonkey.Core;
using NearbyMessage = Android.Gms.Nearby.Messages.Message;

namespace NearbyMonkey
{
        
    class MessagesAdapter : BaseAdapter<EmotionMessage>
    {
        public Activity Parent { get;set; }

        public List<EmotionMessage> Messages { get; set; } = new List<EmotionMessage> ();

        public override long GetItemId (int position)
        {
            return position;
        }

        public override Android.Views.View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            var msg = Messages [position];

            var view = convertView ?? Parent.LayoutInflater.Inflate (Android.Resource.Layout.SimpleListItem1, parent, false);

            view.FindViewById<TextView> (Android.Resource.Id.Text1).Text = msg.ToString ();

            return view;
        }

        public override int Count {
            get {
                return Messages.Count;
            }
        }

        public override EmotionMessage this [int index] {
            get {
                return Messages [index];
            }
        }
    }

}
