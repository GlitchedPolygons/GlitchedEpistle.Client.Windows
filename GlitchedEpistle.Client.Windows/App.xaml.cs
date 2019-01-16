using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

using GlitchedPolygons.Services.JwtService;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Asymmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using Unity;
using Unity.Lifetime;
using Prism.Events;

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
        public const string VERSION = "1.0.0";

        /// <summary>
        /// The application's root directory where all the user settings, convos, etc... are stored.
        /// </summary>
        public static readonly string ROOT_DIRECTORY = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GlitchedPolygons",
            "GlitchedEpistle"
        );

        /// <summary>
        /// The singleton app instance (prevent multiple Epistle instances running).
        /// </summary>
        private static readonly Mutex SINGLETON = new Mutex(true, Assembly.GetCallingAssembly().GetName().Name);

        /// <summary>
        /// The IoC container.
        /// </summary>
        private readonly IUnityContainer container = new UnityContainer();

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Insta-kill the app if there is already another instance running!
            if (!SINGLETON.WaitOne(TimeSpan.Zero, true))
            {
                Application.Current.Shutdown();
            }

            // Register transient types:
            container.RegisterType<JwtService>();
            container.RegisterType<ILogger, Logger>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<ICompressionUtility, GZipUtility>();
            container.RegisterType<IAsymmetricCryptographyRSA, AsymmetricCryptographyRSA>();

            // Register IoC singletons:
            container.RegisterType<ISettings, SettingsJson>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            container.RegisterType<IWindowFactory, WindowFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IViewModelFactory, ViewModelFactory>(new ContainerControlledLifetimeManager());

            // Open the main app's window.
            var mainView = container.Resolve<MainView>();
            mainView.DataContext = container.Resolve<MainViewModel>();
            Application.Current.MainWindow = mainView;
            Application.Current.MainWindow?.Show();
        }

        /// <summary>
        /// Resolves a <see langword="class"/> instance through the IoC container. Only call this method from factories!
        /// </summary>
        /// <typeparam name="T">The class to resolve/create.</typeparam>
        /// <returns>Instantiated/resolved class.</returns>
        public T Resolve<T>() where T : class => container?.Resolve<T>();
    }
}
