using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This event is raised whenever the backend has provided one or more convos
    /// in which the logged in user is involved that were NOT YET on the local device (thus causing the convos list to be updated).
    /// </summary>
    public class UpdatedUserConvos : PubSubEvent
    {
    }
}
