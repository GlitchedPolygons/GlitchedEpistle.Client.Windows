using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="Prism.Events.PubSubEvent{UserCreationResponse}"/> (where <c>TPayload</c> is of type <see cref="UserCreationResponseDto"/>)
    /// is raised whenever the user has submitted the registration form successfully, and is now awaiting to receive his 2FA TOTP secret + backup codes to show on a QR and unordered list, respectively.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent{UserCreationResponse}" />
    public class UserCreationSucceededEvent : PubSubEvent<UserCreationResponseDto>
    {
    }
}
