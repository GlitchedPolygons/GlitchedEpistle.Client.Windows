using System;
using System.Windows;
using System.Collections.Generic;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;

using Unity;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories
{
    /// <inheritdoc/>
    public class WindowFactory : IWindowFactory
    {
        private readonly IViewModelFactory viewModelFactory;
        private readonly Dictionary<Type, Window> windows = new Dictionary<Type, Window>(4);

        public WindowFactory(IViewModelFactory viewModelFactory)
        {
            this.viewModelFactory = viewModelFactory;
        }

        /// <summary>
        /// Gets a <see cref="Window" /> with resolved dependencies through the <see cref="App" />'s <see cref="UnityContainer" />.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Window" /> you want to get.</typeparam>
        /// <param name="ensureSingleWindow">If set to <see langword="true" />, only one active <see cref="Window" /> of the provided type <c>T</c> can exist at one time.</param>
        /// <returns>The retrieved <see cref="Window" /> instance, ready to be shown (via <see cref="Window.Show" />).</returns>
        public T Create<T>(bool ensureSingleWindow) where T : Window
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
                throw new ArgumentException($"{nameof(App)}::{nameof(Create)}: The provided {nameof(Window)} type parameter is of type {nameof(MainView)}, which is not allowed (since it's the main window, only the creating class instance should have control over it).", nameof(T));
            }

            if (!windows.TryGetValue(type, out var window) || window == null)
            {
                window = app.Resolve<T>();
                window.Closed += OnWindowClosed;
                windows[type] = window;
            }

            return window as T;
        }

        /// <summary>
        /// Gets or creates a window and opens/activates it immediately after.
        /// </summary>
        /// <typeparam name="TView">The type of the <see cref="Window" /> (view).</typeparam>
        /// <typeparam name="TViewModel">The type of the associated <see cref="ViewModel" /> DataContext.</typeparam>
        /// <param name="dialog">if set to <c>true</c> the <see cref="Window" /> will be opened as a dialog.</param>
        /// <param name="ensureSingleInstance">if set to <c>true</c> only one instance of the specified <see cref="Window" /> can ever exist at once.</param>
        /// <returns>The opened window's viewmodel.</returns>
        public TViewModel OpenWindow<TView, TViewModel>(bool dialog, bool ensureSingleInstance) where TView : Window where TViewModel : ViewModel
        {
            // When opening views that only exist one at a time,
            // it's important not to recreate the viewmodel every time,
            // as that would override any changes made.
            // Therefore, check if the view already has a data context that isn't null.

            var view = Create<TView>(ensureSingleInstance);

            if (view.DataContext is null)
                view.DataContext = viewModelFactory.Create<TViewModel>();

            if (dialog)
            {
                view.ShowDialog();
            }
            else
            {
                view.Show();
                view.Activate();
            }

            return view.DataContext as TViewModel;
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            var type = sender.GetType();
            windows[type] = null;
        }
    }
}
