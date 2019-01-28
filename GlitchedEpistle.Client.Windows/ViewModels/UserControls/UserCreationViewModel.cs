using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.ExtensionMethods.RSAXmlPemStringConverter;

using Prism.Events;
using Microsoft.Win32;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class UserCreationViewModel : ViewModel
    {
        #region Constants
        private readonly ILogger logger;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly ICompressionUtility gzip;
        private readonly IEventAggregator eventAggregator;
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        #endregion

        #region Commands
        public ICommand ImportCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand QuitCommand { get; }
        #endregion

        #region UI Bindings
        private string username = string.Empty;
        public string Username { get => username; set => Set(ref username, value); }

        private string password = string.Empty;
        public string Password { get => password; set => Set(ref password, value); }

        private string password2 = string.Empty;
        public string Password2 { get => password2; set => Set(ref password2, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }
        #endregion

        private bool pendingAttempt = false;
        private Timer ErrorMessageTimer { get; } = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };

        public UserCreationViewModel(IUserService userService, ISettings settings, IEventAggregator eventAggregator, ICompressionUtility gzip, ILogger logger)
        {
            this.gzip = gzip;
            this.logger = logger;
            this.settings = settings;
            this.userService = userService;
            this.eventAggregator = eventAggregator;

            ImportCommand = new DelegateCommand(OnClickedImport);
            RegisterCommand = new DelegateCommand(OnClickedRegister);
            QuitCommand = new DelegateCommand(OnClickedQuit);

            ErrorMessageTimer.Elapsed += ErrorMessageTimer_Elapsed;
            ErrorMessageTimer.Start();
        }

        private void ErrorMessageTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ErrorMessage != null)
                ErrorMessage = null;
        }

        private void OnClickedImport(object commandParam)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Epistle backup file path",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                DefaultExt = ".dat",
                AddExtension = true,
                Filter = "Epistle Backup File|*.dat;*.dat.gz"
            };
            dialog.FileOk += FileDialog_FileOk;
            dialog.ShowDialog();
        }

        private void FileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is SaveFileDialog dialog)
            {
                dialog.FileOk -= FileDialog_FileOk;
                if (string.IsNullOrEmpty(dialog.FileName))
                {
                    return;
                }

                // TODO: see comment on new line
                // Import user data from backup file here and then reboot the program.

                Application.Current.Shutdown();
                Process.Start(Application.ResourceAssembly.Location);
            }
        }

        private async void OnClickedRegister(object commandParam)
        {
            if (pendingAttempt
                || string.IsNullOrEmpty(Username)
                || string.IsNullOrEmpty(Password)
                || Password != Password2)
            {
                return;
            }

            pendingAttempt = true;

            Directory.CreateDirectory(Paths.KEYS_DIRECTORY);

            var keygen = new RsaKeyPairGenerator();
            keygen.Init(new KeyGenerationParameters(new SecureRandom(), 4096));
            var keys = keygen.GenerateKeyPair();

            using (var sw = new StringWriter())
            {
                var pem = new PemWriter(sw);
                pem.WriteObject(keys.Private);
                pem.Writer.Flush();

                File.WriteAllText(Paths.PRIVATE_KEY_PATH, sw.ToString());
            }

            string pubKeyXml;
            using (var sw = new StringWriter())
            {
                var pem = new PemWriter(sw);
                pem.WriteObject(keys.Public);
                pem.Writer.Flush();

                string pubKeyPem = sw.ToString();
                pubKeyXml = pubKeyPem.PemToXml();

                File.WriteAllText(Paths.PUBLIC_KEY_PATH, pubKeyPem);
            }

            string userCreationSecret = Encoding.UTF8.GetString(gzip.Decompress(File.ReadAllBytes("UserCreator.dat"), new CompressionSettings()));

            var userCreationResponse = await userService.CreateUser(Password, pubKeyXml, userCreationSecret);
            eventAggregator.GetEvent<UserCreationSucceededEvent>().Publish(userCreationResponse);
            logger.LogMessage($"Created user {userCreationResponse.Id}.");
            // Handle this event back in the main view model, since it's there where the backup codes + 2FA secret (QR) will be shown.

            pendingAttempt = false;
        }

        private void OnClickedQuit(object commandParam)
        {
            Application.Current.Shutdown();
        }
    }
}
