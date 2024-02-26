using SimpleJSON;

namespace jshepler.ngu.mods.WebService.Twitch
{
    internal class Subscription
    {
        string id;
        string status;
        string type;

        public Subscription(JSONNode n)
        {
            if (n.HasKey("id"))
                id = n["id"].Value;

            if (n.HasKey("status"))
                status = n["status"].Value;

            if (n.HasKey("type"))
                type = n["type"].Value;
        }
    }
}
