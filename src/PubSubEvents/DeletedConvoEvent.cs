using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="PubSubEvent"/> is raised whenever a the user has successfully deleted a <see cref="Convo"/> server-side.<para> </para>
    /// The passed <c>string</c> parameter is the deleted convo's id.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent" />
    public class DeletedConvoEvent : PubSubEvent<string>
    {
    }
}
