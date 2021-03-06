﻿/*
    Glitched Epistle - Windows Client
    Copyright (C) 2020 Raphael Beck

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

#region
using System;
using System.IO;
using System.Windows;
using System.Threading;
using System.Reflection;
using System.Globalization;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.ServerHealth;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.KeyExchange;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization;

using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.Services.Cryptography.Symmetric;
using GlitchedPolygons.Services.Cryptography.Asymmetric;

using Prism.Events;

using Unity;
using Unity.Lifetime;

using Localization = GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization.Localization;
#endregion

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The client version number.
        /// </summary>
        public static string Version => Assembly.GetEntryAssembly()?.GetName().Version.ToString();

        /// <summary>
        /// Gets the currently active GUI theme (appearance of the app).
        /// </summary>
        /// <value>The current theme.</value>
        public string CurrentTheme { get; private set; } = Themes.DARK_THEME;

        /// <summary>
        /// The IoC container.
        /// </summary>
        private readonly IUnityContainer container = new UnityContainer();

        /// <summary>
        /// MethodQ instance to inject (singleton) into DI container.
        /// </summary>
        private readonly IMethodQ methodQ = new MethodQ();

        private static Mutex mutex;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Directory.CreateDirectory(Paths.ROOT_DIRECTORY);

            // Register transient types:
            container.RegisterType<ILogger, TextLogger>();
            container.RegisterType<IKeyExchange, KeyExchange>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IConvoService, ConvoService>();
            container.RegisterType<ICompressionUtility, BrotliUtility>();
            container.RegisterType<ICompressionUtilityAsync, BrotliUtilityAsync>();
            container.RegisterType<IAsymmetricKeygenRSA, AsymmetricKeygenRSA>();
            container.RegisterType<ISymmetricCryptography, SymmetricCryptography>();
            container.RegisterType<IAsymmetricCryptographyRSA, AsymmetricCryptographyRSA>();
            container.RegisterType<IServerConnectionTest, ServerConnectionTest>();
            container.RegisterType<IMessageSender, MessageSender>();
            container.RegisterType<IPasswordChanger, PasswordChanger>();
            container.RegisterType<IProfilePictureChanger, ProfilePictureChanger>();
            container.RegisterType<IRegistrationService, RegistrationService>();

            // Register IoC singletons:
            container.RegisterType<User>(new ContainerControlledLifetimeManager()); // This is the application's user.
            container.RegisterInstance(methodQ, new ContainerControlledLifetimeManager());
            container.RegisterType<ILocalization, Localization>(new ContainerControlledLifetimeManager());
            container.RegisterType<IAppSettings, AppSettingsJson>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUserSettings, UserSettingsJson>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            container.RegisterType<IViewModelFactory, ViewModelFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IWindowFactory, WindowFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IConvoPasswordProvider, ConvoPasswordProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<IMessageFetcher, MessageFetcher>(new ContainerControlledLifetimeManager());

            var settings = container.Resolve<IAppSettings>();

            try
            {
                Client.Windows.Properties.Resources.Culture
                    = Thread.CurrentThread.CurrentCulture
                    = Thread.CurrentThread.CurrentUICulture
                    = new CultureInfo(settings["Language", "en"]);
            }
            catch
            {
                settings["Language"] = (Client.Windows.Properties.Resources.Culture = Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo("en")).Name;
            }

            mutex = new Mutex(true, $"GlitchedEpistle_{Version}", out bool newInstance);

            if (!newInstance)
            {
                MessageBox.Show(Client.Windows.Properties.Resources.GlitchedEpistleAlreadyRunningErrorMessage);
                Current.Shutdown();
            }

            // Open the main app's window.
            var mainView = container.Resolve<MainView>();
            mainView.DataContext = container.Resolve<MainViewModel>();
            (Current.MainWindow = mainView)?.Show();

            ChangeTheme(settings["Theme", Themes.DARK_THEME]);
        }

        /// <summary>
        /// Resolves a <c>class</c> instance through the IoC container. Only call this method from factories!
        /// </summary>
        /// <typeparam name="T">The class to resolve/create.</typeparam>
        /// <returns>Instantiated/resolved class.</returns>
        public T Resolve<T>() where T : class
        {
            return container?.Resolve<T>();
        }

        /// <summary>
        /// Changes the application GUI's theme.
        /// </summary>
        /// <param name="theme">The theme to switch to.</param>
        /// <returns>Whether the theme change occurred or not (e.g. in case of changing to a theme that's already active, or in case of a failure, this method returns <c>false</c>).</returns>
        public bool ChangeTheme(string theme)
        {
            if (theme.NullOrEmpty() || theme.Equals(CurrentTheme))
            {
                return false;
            }

            string path = null;
            ILogger logger = container?.Resolve<ILogger>();

            switch (theme)
            {
                case Themes.DARK_THEME:
                    path = "/Resources/Themes/DarkTheme.xaml";
                    break;
                case Themes.LIGHT_THEME:
                    path = "/Resources/Themes/LightTheme.xaml";
                    break;
                case Themes.OLED_THEME:
                    path = "/Resources/Themes/OLEDTheme.xaml";
                    break;
            }

            if (path.NullOrEmpty())
            {
                logger?.LogWarning($"Theme \"{theme}\" couldn't be found/does not exist.");
                return false;
            }

            try
            {
                Resources.MergedDictionaries[0] = new ResourceDictionary { Source = new Uri(path, UriKind.Relative) };
                CurrentTheme = theme;
                return true;
            }
            catch (Exception e)
            {
                logger?.LogWarning($"Theme \"{path}\" couldn't be applied. Reverting to default theme... Thrown exception: {e.ToString()}");
                return false;
            }
        }
    }
}
