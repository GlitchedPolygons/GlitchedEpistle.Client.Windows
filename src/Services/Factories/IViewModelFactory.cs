using System.Windows;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;

using Unity;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories
{
    /// <summary>
    /// Interface for the <see cref="ViewModel"/> factory.
    /// </summary>
    public interface IViewModelFactory
    {
        /// <summary>
        /// Gets a <see cref="ViewModel"/> with resolved dependencies through the <see cref="App"/>'s <see cref="UnityContainer"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ViewModel"/> you want to get.</typeparam>
        /// <returns>The retrieved <see cref="ViewModel"/> instance, ready to be assigned to a <see cref="Window.DataContext"/>.</returns>
        T Create<T>() where T : ViewModel;
    }
}
