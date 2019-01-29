using Unity;
using System.Windows;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories
{
    /// <summary>
    /// Interface for the <see cref="Window"/> factory.
    /// </summary>
    public interface IWindowFactory
    {
        /// <summary>
        /// Gets a <see cref="Window"/> with resolved dependencies through the <see cref="App"/>'s <see cref="UnityContainer"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Window"/> you want to get.</typeparam>
        /// <param name="ensureSingleWindow">If set to <see langword="true"/>, only one active <see cref="Window"/> of the provided type <c>T</c> can exist at one time.</param>
        /// <returns>The retrieved <see cref="Window"/> instance, ready to be shown (via <see cref="Window.Show"/>).</returns>
        T Create<T>(bool ensureSingleWindow) where T : Window;
    }
}
