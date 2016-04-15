using System;

namespace NearbyMonkey.Core
{
    public class EmotionMessage
    {
        public string UserId { get; set; }

        public string Species { get; set; }

        public string Name { get; set; }

        public string Emotion { get; set; }

        public byte[] Serialize()
        {
            string str = $"{UserId}|{Name}|{Species}|{Emotion}";
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        public static EmotionMessage Deserialize(byte[] value)
        {
            var str = System.Text.Encoding.UTF8.GetString(value, 0, value.Length);

            var parts = str.Split('|');

            if (parts.Length >= 4)
            {
                return new EmotionMessage
                {
                    UserId = parts[0],
                    Name = parts[1],
                    Species = parts[2],
                    Emotion = parts[3]
                };
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            var eobj = obj as EmotionMessage;

            if (eobj == null)
                return false;

            return eobj.UserId == UserId
            && eobj.Name == Name
            && eobj.Emotion == Emotion
            && eobj.Species == Species;
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Species} {Name} is feeling {Emotion}";
        }
    }
}

