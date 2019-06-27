using GlitchedPolygons.GlitchedEpistle.Client.Models;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="Prism.Events.PubSubEvent{String}"/> (where <c>TPayload</c> is of type <c>string</c>)
    /// is raised whenever the user has successfully created a new <see cref="Convo"/>.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent{String}" />
    public class ConvoCreationSucceededEvent : PubSubEvent<string>
    {
    }
}
