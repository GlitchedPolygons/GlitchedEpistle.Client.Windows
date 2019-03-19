using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This parameterless <see cref="PubSubEvent"/> is raised whenever a the user has successfully confirmed the deletion of his application data (user directory).
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent" />
    public class ResetConfirmedEvent : PubSubEvent
    {
    }
}
