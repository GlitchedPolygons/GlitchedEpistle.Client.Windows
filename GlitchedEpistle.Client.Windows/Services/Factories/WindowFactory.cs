using System;
using System.Windows;
using System.Collections.Generic;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories
{
    /// <inheritdoc/>
    public class WindowFactory : IWindowFactory
    {
        private readonly Dictionary<Type, Window> windows = new Dictionary<Type, Window>(4);
        
        /// <inheritdoc/>
        public T GetWindow<T>(bool ensureSingleWindow) where T : Window
        {
            var app = Application.Current as App;
            if (app is null) return null;

            if (!ensureSingleWindow)
            {
                return app.Resolve<T>();
            }

            var type = typeof(T);
            if (type == typeof(MainView))
            {
                throw new ArgumentException($"{nameof(App)}::{nameof(GetWindow)}: The provided {nameof(Window)} type parameter is of type {nameof(MainView)}, which is not allowed (since it's the main window, only the creating class instance should have control over it).", nameof(T));
            }

            if (!windows.TryGetValue(type, out var window) || window == null)
            {
                window = app.Resolve<T>();
                window.Closed += OnWindowClosed;
                windows[type] = window;
            }

            return window as T;
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            var type = sender.GetType();
            windows[type] = null;
        }
    }
}
