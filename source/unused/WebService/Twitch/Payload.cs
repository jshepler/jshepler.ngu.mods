using SimpleJSON;

namespace jshepler.ngu.mods.WebService.Twitch
{
    internal class Payload
    {
        internal Session session;
        internal Subscription subscription;
        internal Event @event;

        public Payload(JSONNode n)
        {
            if (n.HasKey("session"))
                session = new Session(n["session"]);

            if (n.HasKey("subscription"))
                subscription = new Subscription(n["subscription"]);

            if (n.HasKey("event"))
                @event = new Event(n["event"]);
        }
    }
}
