using System;
using MonoTouch.Dialog;
using System.Collections.Generic;
using UIKit;

namespace NearbyMonkey.iOS
{
	public class MonkeyDialog
	{
		[RadioSelection ("Species")]
		public int CurrentSpecie;
		public List<string> Species;

		[RadioSelection ("Names")]
		public int CurrentName;
		public List<string> Names;

		[RadioSelection ("Emotions")]
		public int CurrentEmotion;
		public List<string> Emotions;

		[Section]
		
		[OnTap ("UpdateMessage")]
		[Alignment (UITextAlignment.Center)]
		public string Update;
	}
}

