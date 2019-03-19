using System.Windows;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;

using Unity;

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

        /// <summary>
        /// Gets or creates a window and opens/activates it immediately after.
        /// </summary>
        /// <typeparam name="TView">The type of the <see cref="Window"/> (view).</typeparam>
        /// <typeparam name="TViewModel">The type of the associated <see cref="ViewModel"/> DataContext.</typeparam>
        /// <param name="dialog">if set to <c>true</c> the <see cref="Window"/> will be opened as a dialog.</param>
        /// <param name="ensureSingleInstance">if set to <c>true</c> only one instance of the specified <see cref="Window"/> can ever exist at once.</param>
        TViewModel OpenWindow<TView, TViewModel>(bool dialog, bool ensureSingleInstance) where TView : Window where TViewModel : ViewModel;
    }
}
