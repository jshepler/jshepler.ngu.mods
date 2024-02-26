using SimpleJSON;

namespace jshepler.ngu.mods.WebService.Twitch
{
    internal class MetaData
    {
        internal string messageType;
        internal string subscriptionType;
        internal string subscriptionVersion;

        public MetaData(JSONNode n)
        {
            if (n.HasKey("message_type"))
                messageType = n["message_type"].Value;

            if (n.HasKey("subscription_type"))
                subscriptionType = n["subscription_type"].Value;

            if (n.HasKey("subscription_version"))
                subscriptionVersion = n["subscription_version"].Value;
        }
    }
}
