using Unity;
using System.Windows;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories
{
    /// <inheritdoc/>
    public class ViewModelFactory : IViewModelFactory
    {
        /// <summary>
        /// Gets a <see cref="ViewModel" /> with resolved dependencies through the <see cref="App" />'s <see cref="UnityContainer" />.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ViewModel" /> you want to get.</typeparam>
        /// <returns>The retrieved <see cref="ViewModel" /> instance, ready to be assigned to a <see cref="Window.DataContext" />.</returns>
        public T Create<T>() where T : ViewModel => (Application.Current as App)?.Resolve<T>();
    }
}
