using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="Prism.Events.PubSubEvent{String}"/> (where <c>TPayload</c> is of type <see langword="string"/>)
    /// is raised whenever the user has successfully created a new <see cref="Convo"/>.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent{String}" />
    public class ConvoCreationSucceededEvent : PubSubEvent<string> { }
}
