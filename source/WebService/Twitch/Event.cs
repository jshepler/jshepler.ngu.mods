using SimpleJSON;

namespace jshepler.ngu.mods.WebService.Twitch
{
    internal class Event
    {
        internal Reward reward;

        public Event(JSONNode n)
        {
            if (n.HasKey("reward"))
                reward = new Reward(n["reward"]);
        }
    }

    internal class Reward
    {
        internal string id;
        internal string title;
        internal int cost;

        public Reward(JSONNode n)
        {
            id = n["id"].Value;
            title = n["title"].Value;
            cost = n["cost"].AsInt;
        }
    }
}
