using System;
using SimpleJSON;

namespace jshepler.ngu.mods.WebService.Twitch
{
    internal class TwitchEventArgs : EventArgs
    {
        internal TwitchMessage Message;
        internal string RawMessage;

        public TwitchEventArgs(TwitchMessage message, string rawMessage)
        {
            Message = message;
            RawMessage = rawMessage;
        }

        internal static TwitchEventArgs From(TwitchMessage message, string rawMessage)
        {
            return new TwitchEventArgs(message, rawMessage);
        }

        internal static TwitchEventArgs From(string message)
        {
            var node = JSONNode.Parse(message);
            var data = new TwitchMessage(node);

            return new TwitchEventArgs(data, message);
        }
    }
}
