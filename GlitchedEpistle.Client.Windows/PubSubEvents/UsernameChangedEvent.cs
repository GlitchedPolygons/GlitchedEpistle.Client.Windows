using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="Prism.Events.PubSubEvent{String}"/> (where <c>TPayload</c> is of type <see cref="System.String"/>)
    /// is raised whenever the username setting has been changed and saved (usually when closing the user settings window).
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent{String}" />
    public class UsernameChangedEvent : PubSubEvent<string>
    {
    }
}
