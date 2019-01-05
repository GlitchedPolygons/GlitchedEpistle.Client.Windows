﻿using Unity;
using System;
using System.Windows;
using System.Collections.Generic;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string VERSION = "1.0.0";

        private readonly IUnityContainer container = new UnityContainer();
        private readonly Dictionary<Type, Window> windows = new Dictionary<Type, Window>(4);

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            container.RegisterType<ILogger, Logger>();
            container.RegisterType<ISettings, SettingsJson>();

            Application.Current.MainWindow = container.Resolve<MainWindow>();
            Application.Current.MainWindow?.Show();
        }

        /// <summary>
        /// Gets a <see cref="Window"/> with resolved dependencies through the <see cref="App"/>'s <see cref="UnityContainer"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Window"/> you want to get.</typeparam>
        /// <param name="ensureSingleWindow">If set to <see langword="true"/>, only one active <see cref="Window"/> of the provided type <c>T</c> can exist at one time.</param>
        /// <returns>The retrieved <see cref="Window"/> instance, ready to be shown (via <see cref="Window.Show"/>).</returns>
        public T GetWindow<T>(bool ensureSingleWindow) where T : Window
        {
            if (!ensureSingleWindow)
            {
                return container.Resolve<T>();
            }

            var type = typeof(T);
            if (type == typeof(MainWindow))
            {
                throw new ArgumentException($"{nameof(App)}::{nameof(GetWindow)}: The provided {nameof(Window)} type parameter is of type {nameof(MainWindow)}, which is not allowed (since it's the main window, only the creating class instance should have control over it).", nameof(T));
            }

            if (!windows.TryGetValue(type, out var window) || window == null)
            {
                window = container.Resolve<T>();
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
