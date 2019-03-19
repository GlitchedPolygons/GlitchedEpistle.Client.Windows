using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This event is raised whenever the user has submitted the registration form successfully, and has verified the procedure by submitting his 2FA code.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent{UserCreationResponse}" />
    public class UserCreationVerifiedEvent : PubSubEvent
    {
    }
}
