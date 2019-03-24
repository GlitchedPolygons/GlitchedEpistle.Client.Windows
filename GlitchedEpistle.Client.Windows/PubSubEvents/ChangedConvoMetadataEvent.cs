using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="PubSubEvent"/> is raised whenever a the user has successfully modified a <see cref="Convo"/>'s metadata server-side.<para> </para>
    /// The passed <c>string</c> parameter is the convo id.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent" />
    public class ChangedConvoMetadataEvent : PubSubEvent<string>
    {
    }
}
