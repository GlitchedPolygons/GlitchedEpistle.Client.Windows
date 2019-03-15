using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="Prism.Events.PubSubEvent{TPayload}"/> (where <c>TPayload</c> is of type <see cref="Convo"/>)
    /// is raised whenever the user has submitted the convo credentials form successfully, and has now joined the conversation successfully.<para> </para>
    /// The subscriber should be adding the convo data to the device in a persistent way (e.g. file IO).
    /// </summary>
    public class JoinedConvoEvent : PubSubEvent<Convo> { }
}
