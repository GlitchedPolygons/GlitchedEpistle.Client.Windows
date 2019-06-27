using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="PubSubEvent"/> is raised whenever a the user has successfully modified a <see cref="Convo"/>'s metadata server-side.<para> </para>
    /// The passed <c>string</c> parameter is the convo id.<para> </para>
    /// Can also be raised when you're a non-admin participant of a (currently open and active)
    /// <see cref="Convo"/> and the admin changed the metadata server-side.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent" />
    public class ChangedConvoMetadataEvent : PubSubEvent<string>
    {
    }
}
