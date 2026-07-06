using SimpleJSON;

namespace jshepler.ngu.mods.WebService.Twitch
{
    internal class TwitchMessage
    {
        internal static class MessageTypes
        {
            internal const string Welcome = "session_welcome";
            internal const string KeepAlive = "session_keepalive";
            internal const string Notification = "notification";
            internal const string Reconnect = "session_reconnect";
            internal const string Revocation = "revocation";
        }

        internal MetaData metaData;
        internal Payload payload;

        public TwitchMessage(JSONNode node)
        {
            if (node.HasKey("metadata"))
                metaData = new MetaData(node["metadata"]);

            if (node.HasKey("payload"))
                payload = new Payload(node["payload"]);
        }
    }
}
