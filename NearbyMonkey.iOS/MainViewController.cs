
using System;
using System.Linq;
using System.Collections.Generic;

using MonoTouch.Dialog;

using Foundation;
using UIKit;
using NearbyMonkey.Core;
using Google.Nearby;

namespace NearbyMonkey.iOS
{
	public partial class MainViewController : DialogViewController
	{
		string userId;
		MessageManager manager;
		ISubscription subscription;
		BindingContext context;
		MonkeyDialog dialog;
		Section messagesSection;
		EmotionMessage myMessage;
		List<EmotionMessage> monkeysMessages;

		public MainViewController () : base (UITableViewStyle.Grouped, null, true)
		{
			// Assign data that will be shown on UI
			SetupData ();

			var btnUnpublish = new UIBarButtonItem ("Unpublish", UIBarButtonItemStyle.Plain, (sender, e) => Unpublish (myMessage));
			NavigationItem.RightBarButtonItem = btnUnpublish;

			// Present data on UI with MonoTouch Dialog help
			context = new BindingContext (this, dialog, "Nearby Monkey");
			Root = context.Root;
			messagesSection = new Section ("Monkeys Messages");
			Root.Add (messagesSection);

			// Generate a unique user id, one time, and save it for future use
			GetUserId ();
		}

		// When the Update button is clicked, update our published Message
		public void UpdateMessage ()
		{
			// Retrieves updated data from UI  
			context.Fetch ();

			// Remove our last message from MonoTouch Dialog
			if (myMessage != null)
				RemoveMonkeyMessage (myMessage);

			// Create new Nearby message to publish with the list choices
			myMessage = new EmotionMessage {
				UserId = userId,
				Name = dialog.Names.ElementAt (dialog.CurrentName),
				Species = dialog.Species.ElementAt (dialog.CurrentSpecie),
				Emotion = dialog.Emotions.ElementAt (dialog.CurrentEmotion)
			};

			// Unpublish message, if we already published a message
			Unpublish (myMessage);

			// Publish the new message
			Publish (myMessage);
		}

		// Publish the receive message
		void Publish (EmotionMessage emotionMessage)
		{
			AppDelegate.PublishedMessage = manager.Publication (Message.Create (NSData.FromArray (emotionMessage.Serialize ())));
			AddMonkeyMessage (emotionMessage);
		}

		// To unpublish a message you need to dispose the published message
		void Unpublish (EmotionMessage emotionMessage)
		{
			if (AppDelegate.PublishedMessage == null)
				return;
			AppDelegate.PublishedMessage.Dispose ();
			AppDelegate.PublishedMessage = null;
			RemoveMonkeyMessage (emotionMessage);
		}

		// Assign data that will be shown on UI
		void SetupData ()
		{
			var seed = DateTime.Now.Millisecond;
			dialog = new MonkeyDialog {
				CurrentSpecie = new Random (seed).Next (0, Values.Species.Length),
				Species = Values.Species.ToList (),
				CurrentName = new Random (seed).Next (0, Values.Names.Length),
				Names = Values.Names.ToList (),
				CurrentEmotion = new Random (seed).Next (0, Values.Emotions.Length),
				Emotions = Values.Emotions.ToList ()
			};

			monkeysMessages = new List<EmotionMessage> ();
			manager = new MessageManager (AppDelegate.NearbyApiKey);
			subscription = manager.Subscription (MessageFound, MessageLost);
		}

		// Method to be called when a monkey published his/her message
		void MessageFound (Message message)
		{
			var emotionMessage = EmotionMessage.Deserialize (message.Content.ToArray ());
			AddMonkeyMessage (emotionMessage);
		}

		// Method to be called when a monkey unpublished his/her message
		void MessageLost (Message message)
		{
			var emotionMessage = EmotionMessage.Deserialize (message.Content.ToArray ());
			RemoveMonkeyMessage (emotionMessage);
		}

		// Add monkey message to MonoTouch Dialog
		void AddMonkeyMessage (EmotionMessage message)
		{
			monkeysMessages.Add (message);
			messagesSection.Add (new StringElement (message.ToString ()));
			Root.Reload (messagesSection, UITableViewRowAnimation.Fade);
		}

		// Remove monkey message from MonoTouch Dialog
		void RemoveMonkeyMessage (EmotionMessage message)
		{
			var index = monkeysMessages.IndexOf (message);
			if (index < 0)
				return;
			monkeysMessages.RemoveAt (index);
			messagesSection.Remove (index);
			Root.Reload (messagesSection, UITableViewRowAnimation.Fade);
		}

		// Generate a unique user id, one time, and save it for future use
		void GetUserId ()
		{
			userId = NSUserDefaults.StandardUserDefaults.StringForKey ("userid");
			if (userId != null)
				return;
			NSUserDefaults.StandardUserDefaults.SetString (Guid.NewGuid ().ToString (), "userid");
			userId = NSUserDefaults.StandardUserDefaults.StringForKey ("userid");
		}
	}
}
