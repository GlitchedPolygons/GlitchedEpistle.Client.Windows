using Unity;
using Unity.Lifetime;
using Prism.Events;
using System.Windows;
using GlitchedPolygons.Services.JwtService;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Asymmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string VERSION = "1.0.0";

        private readonly IUnityContainer container = new UnityContainer();

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            container.RegisterType<JwtService>();
            container.RegisterType<ILogger, Logger>();
            container.RegisterType<ICompressionUtility, GZipUtility>();
            container.RegisterType<IAsymmetricCryptographyRSA, AsymmetricCryptographyRSA>();
            container.RegisterType<ISettings, SettingsJson>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            container.RegisterType<IWindowFactory, WindowFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IViewModelFactory, ViewModelFactory>(new ContainerControlledLifetimeManager());

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
