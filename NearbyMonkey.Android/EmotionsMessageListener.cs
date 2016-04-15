using System;
using Android.Gms.Nearby.Messages;
using NearbyMonkey.Core;
using NearbyMessage = Android.Gms.Nearby.Messages.Message;

namespace NearbyMonkey
{
    // Nearby Message API listener helper class
    class EmotionsMessageListener : MessageListener
    {
        public Action<EmotionMessage> OnFoundHandler { get; set; }
        public Action<EmotionMessage> OnLostHandler { get; set; }

        public override void OnFound (NearbyMessage message)
        {
            var emotionMsg = EmotionMessage.Deserialize (message.GetContent ());
            OnFoundHandler?.Invoke (emotionMsg);
        }

        public override void OnLost (NearbyMessage message)
        {
            base.OnLost (message);

            var emotionMsg = EmotionMessage.Deserialize (message.GetContent ());
            OnLostHandler?.Invoke (emotionMsg);
        }
    }
}
