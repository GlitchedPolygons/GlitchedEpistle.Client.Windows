using System.Windows;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories
{
    /// <inheritdoc/>
    public class ViewModelFactory : IViewModelFactory
    {
        /// <inheritdoc/>
        public T Create<T>() where T : ViewModel => (Application.Current as App)?.Resolve<T>();
    }
}
