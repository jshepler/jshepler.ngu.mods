using SimpleJSON;

namespace jshepler.ngu.mods.WebService.Twitch
{
    internal class Session
    {
        internal string id;
        internal string status;
        internal string reconnectUrl;

        public Session(JSONNode n)
        {
            if (n.HasKey("id"))
                id = n["id"].Value;

            if (n.HasKey("status"))
                status = n["status"].Value;

            if (n.HasKey("reconnect_url"))
                reconnectUrl = n["reconnect_url"].Value;
        }
    }
}
