// System namespaces
using System;
using System.IO;
using System.Windows;
using System.Threading;
using System.Reflection;

// Shared code namespaces
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Coupons;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.ServerHealth;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Messages;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Symmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Asymmetric;

// Glitched Polygons NuGet packages
using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.Services.JwtService;
using GlitchedPolygons.Services.CompressionUtility;

// Windows client namespaces
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;

// Third party namespaces
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
        /// The IoC container.
        /// </summary>
        private readonly IUnityContainer container = new UnityContainer();

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mutex = new Mutex(true, Assembly.GetCallingAssembly().GetName().Name, out bool newInstance);  
            if (!newInstance)
            {
                MessageBox.Show("There is already one instance of Glitched Epistle running!");
                Current.Shutdown();  
            }
            
            Directory.CreateDirectory(Paths.ROOT_DIRECTORY);

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
            container.RegisterType<IMethodQ, MethodQ>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISettings, SettingsJson>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            container.RegisterType<IViewModelFactory, ViewModelFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IWindowFactory, WindowFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IConvoProvider, ConvoProvider>(new ContainerControlledLifetimeManager());
            
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
