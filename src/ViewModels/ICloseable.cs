using System;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    /// <summary>
    /// Interface for requesting a <see cref="System.Windows.Window"/>'s closure through its <see cref="ViewModel"/>.
    /// </summary>
    public interface ICloseable
    {
        /// <summary>
        /// Occurs when some <see cref="ViewModel"/> requested a view's closure
        /// (code-behind should react to this <c>event</c> and close itself when it's raised).
        /// </summary>
        event EventHandler<EventArgs> RequestedClose;
    }
}
