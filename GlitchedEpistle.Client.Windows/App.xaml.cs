using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Coupons;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Asymmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Messages;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Symmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.ServerHealth;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.RepositoryPattern;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.Services.JwtService;
using GlitchedPolygons.Services.MethodQ;

using Prism.Events;

using Unity;
using Unity.Lifetime;

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
        public const string VERSION = "1.0.7";

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

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mutex = new Mutex(true, Assembly.GetCallingAssembly().GetName().Name, out bool newInstance);
            if (!newInstance)
            {
                MessageBox.Show("There is already one instance of Glitched Epistle running!");
                Current.Shutdown();
            }

            Directory.CreateDirectory(Paths.ROOT_DIRECTORY);
            Directory.CreateDirectory(Paths.CONVOS_DIRECTORY);
            
            // Register transient types:
            container.RegisterType<JwtService>();
            container.RegisterType<ILogger, Logger>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IConvoService, ConvoService>();
            container.RegisterType<ICouponService, CouponService>();
            container.RegisterType<ICompressionUtility, GZipUtility>();
            container.RegisterType<IAsymmetricKeygen, AsymmetricKeygenRSA4096>();
            container.RegisterType<ISymmetricCryptography, SymmetricCryptography>();
            container.RegisterType<IAsymmetricCryptographyRSA, AsymmetricCryptographyRSA>();
            container.RegisterType<IMessageCryptography, MessageCryptography>();
            container.RegisterType<IServerConnectionTest, ServerConnectionTest>();

            // Register IoC singletons:
            container.RegisterType<User>(new ContainerControlledLifetimeManager()); // This is the application's user.
            container.RegisterInstance(methodQ, new ContainerControlledLifetimeManager());
            container.RegisterType<ISettings, SettingsJson>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            container.RegisterType<IViewModelFactory, ViewModelFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IWindowFactory, WindowFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IConvoPasswordProvider, ConvoPasswordProvider>(new ContainerControlledLifetimeManager());
            container.RegisterInstance<IRepository<Convo, string>>(new ConvoRepositorySQLite($"Data Source={Path.Combine(Paths.CONVOS_DIRECTORY, "_metadata.db")};Version=3;"), new ContainerControlledLifetimeManager());

            // Open the main app's window.
            var mainView = container.Resolve<MainView>();
            mainView.DataContext = container.Resolve<MainViewModel>();
            Current.MainWindow = mainView;
            Current.MainWindow?.Show();

            var settings = container.Resolve<ISettings>();
            ChangeTheme(settings["Theme", Themes.DARK_THEME]);
        }

        /// <summary>
        /// Resolves a <see langword="class"/> instance through the IoC container. Only call this method from factories!
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
