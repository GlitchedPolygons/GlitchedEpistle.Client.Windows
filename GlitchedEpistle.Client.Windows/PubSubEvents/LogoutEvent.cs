using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// When this event is raised, it causes the user to be logged out.
    /// Thus, to log out the user from anywhere in the app, just raise this event via the <see cref="IEventAggregator"/>.
    /// </summary>
    public class LogoutEvent : PubSubEvent
    {
    }
}
