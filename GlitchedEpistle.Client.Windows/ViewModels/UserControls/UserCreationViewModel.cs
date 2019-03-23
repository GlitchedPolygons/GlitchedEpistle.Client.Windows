using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GlitchedPolygons.ExtensionMethods.RSAXmlPemStringConverter;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Asymmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.Services.CompressionUtility;

using Microsoft.Win32;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class UserCreationViewModel : ViewModel
    {
        #region Constants
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        private readonly ILogger logger;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IAsymmetricKeygen keygen;
        private readonly ICompressionUtility gzip;
        private readonly IWindowFactory windowFactory;
        private readonly IViewModelFactory viewModelFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly Timer errorMessageTimer = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };
        private static readonly CompressionSettings COMPRESSION_SETTINGS = new CompressionSettings();
        #endregion

        #region Commands
        public ICommand PasswordChangedCommand1 { get; }
        public ICommand PasswordChangedCommand2 { get; }
        public ICommand ImportCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand QuitCommand { get; }
        #endregion

        #region UI Bindings
        public bool uiEnabled = true;
        public bool UIEnabled
        {
            get => uiEnabled;
            private set => Set(ref uiEnabled, value);
        }

        public bool formValid;
        public bool FormValid
        {
            get => formValid;
            private set => Set(ref formValid, value);
        }

        private string username = string.Empty;
        public string Username
        {
            get => username;
            set => Set(ref username, value);
        }

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }
        #endregion

        private bool pendingAttempt;
        private string password1, password2;

        public UserCreationViewModel(IUserService userService, ISettings settings, IEventAggregator eventAggregator, ICompressionUtility gzip, ILogger logger, IAsymmetricKeygen keygen, IViewModelFactory viewModelFactory, IWindowFactory windowFactory)
        {
            this.gzip = gzip;
            this.logger = logger;
            this.keygen = keygen;
            this.settings = settings;
            this.userService = userService;
            this.windowFactory = windowFactory;
            this.viewModelFactory = viewModelFactory;
            this.eventAggregator = eventAggregator;

            PasswordChangedCommand1 = new DelegateCommand(commandParam =>
            {
                if (commandParam is PasswordBox passwordBox)
                {
                    password1 = passwordBox.Password;
                }
                ValidateForm();
            });

            PasswordChangedCommand2 = new DelegateCommand(commandParam =>
            {
                if (commandParam is PasswordBox passwordBox)
                {
                    password2 = passwordBox.Password;
                }
                ValidateForm();
            });

            ImportCommand = new DelegateCommand(OnClickedImport);
            RegisterCommand = new DelegateCommand(OnClickedRegister);
            QuitCommand = new DelegateCommand(_ => { Application.Current.Shutdown(); });

            errorMessageTimer.Elapsed += (_, __) => ErrorMessage = null;
            errorMessageTimer.Start();
        }

        ~UserCreationViewModel()
        {
            password1 = null;
            password2 = null;
        }

        private void OnClickedImport(object commandParam)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Epistle backup file path",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                DefaultExt = ".dat",
                AddExtension = true,
                Filter = "Epistle Backup File|*.dat"
            };

            dialog.FileOk += (sender, e) =>
            {
                if (sender is OpenFileDialog _dialog && _dialog.FileName.NotNullNotEmpty())
                {
                    var view = windowFactory.Create<ImportUserFromBackupView>(true);
                    if (view.DataContext is null)
                    {
                        var viewModel = viewModelFactory.Create<ImportUserFromBackupViewModel>();
                        viewModel.BackupFilePath = _dialog.FileName;
                        view.DataContext = viewModel;
                    }
                    view.ShowDialog();
                    view.Activate();
                }
            };

            dialog.ShowDialog();
        }

        private void OnClickedRegister(object commandParam)
        {
            if (pendingAttempt
                || Username.NullOrEmpty()
                || password1.NullOrEmpty()
                || password2.NullOrEmpty()
                || password1 != password2 || password1.Length < 7)
            {
                return;
            }

            pendingAttempt = true;
            UIEnabled = false;

            var loadingScreen = new GeneratingKeyView { Topmost = true };
            loadingScreen.Show();

            Task.Run(async () =>
            {
                bool keyGenerationSuccessful = await keygen.GenerateKeyPair(Paths.KEYS_DIRECTORY);
                if (keyGenerationSuccessful)
                {
                    try
                    {
                        var userCreationResponse = await userService.CreateUser(
                            new UserCreationDto
                            {
                                PasswordSHA512 = password1.SHA512(),
                                PublicKeyXml = File.ReadAllText(Paths.PUBLIC_KEY_PATH).PemToXml(),
                                CreationSecret = Encoding.UTF8.GetString(
                                    gzip.Decompress(
                                        File.ReadAllBytes("UserCreator.dat"), COMPRESSION_SETTINGS
                                    )
                                )
                            }
                        );

                        if (userCreationResponse is null)
                        {
                            logger.LogError("The user creation process failed server-side. Reason unknown; please make an admin check out the server's log files!");
                            Application.Current?.Dispatcher?.Invoke(() =>
                            {
                                UIEnabled = true;
                                pendingAttempt = false;
                                loadingScreen?.Close();
                            });
                            return;
                        }

                        // Handle this event back in the main view model,
                        // since it's there where the backup codes + 2FA secret (QR) will be shown.
                        eventAggregator.GetEvent<UserCreationSucceededEvent>().Publish(userCreationResponse);
                        logger.LogMessage($"Created user {userCreationResponse.Id}.");

                        settings[nameof(Username)] = Username;
                        settings.Save();
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"The user creation process failed. Thrown exception: {e}");
                    }
                }
                else
                {
                    var errorMsg = "There was an unexpected error whilst generating the RSA key pair (during user creation process).";
                    logger.LogError(errorMsg);
                    ErrorMessage = errorMsg;
                }

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    UIEnabled = true;
                    pendingAttempt = false;
                    loadingScreen?.Close();
                });
                
                password1 = password2 = null;
            });
        }

        private void ValidateForm()
        {
            FormValid = Username.NotNullNotEmpty() &&
                        password1.NotNullNotEmpty() &&
                        password2.NotNullNotEmpty() &&
                        password1 == password2 &&
                        password1.Length > 7;
        }
    }
}