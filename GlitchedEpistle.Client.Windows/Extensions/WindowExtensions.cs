using System;
using System.Windows;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions
{
    /// <summary>
    /// Extension methods for all <see cref="Window"/>s.
    /// </summary>
    public static class WindowExtensions
    {
        /// <summary>
        /// Makes the <see cref="Window"/> react to the <see cref="ICloseable.RequestedClose"/> <see langword="event"/>. by subscribing <see cref="Window.Close"/> to it.
        /// Only call this ONCE and only FROM THE <see cref="Window"/>'S CONSTRUCTOR! 
        /// </summary>
        /// <param name="window">The <see cref="Window"/> that needs to be closeable through its <see cref="ViewModel"/>.</param>
        public static void MakeCloseable(this Window window)
        {
            if (window.DataContext is ICloseable dataContext)
            {
                void OnRequestedClosure(object sender, EventArgs args) => window.Close();
                dataContext.RequestedClose += OnRequestedClosure;
                window.Closing += (sender, args) => dataContext.RequestedClose -= OnRequestedClosure;
            }
        }
    }
}
