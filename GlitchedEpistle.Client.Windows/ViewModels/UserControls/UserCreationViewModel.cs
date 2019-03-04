using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.ExtensionMethods.RSAXmlPemStringConverter;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Asymmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;

using Prism.Events;
using Microsoft.Win32;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class UserCreationViewModel : ViewModel
    {
        #region Constants
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        private readonly Timer errorMessageTimer = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };
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
        public bool UIEnabled { get => uiEnabled; private set => Set(ref uiEnabled, value); }

        public bool formValid;
        public bool FormValid { get => formValid; private set => Set(ref formValid, value); }

        private string username = string.Empty;
        public string Username { get => username; set => Set(ref username, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }
        #endregion

        private bool pendingAttempt = false;
        private string password1, password2;

        public UserCreationViewModel(IUserService userService, ISettings settings, IEventAggregator eventAggregator, ICompressionUtility gzip, ILogger logger, IAsymmetricKeygen keygen, IViewModelFactory viewModelFactory, IWindowFactory windowFactory)
        {
            PasswordChangedCommand1 = new DelegateCommand(commandParam =>
            {
                if (commandParam is PasswordBox passwordBox)
                    password1 = passwordBox.Password;
                ValidateForm();
            });

            PasswordChangedCommand2 = new DelegateCommand(commandParam =>
            {
                if (commandParam is PasswordBox passwordBox)
                    password2 = passwordBox.Password;
                ValidateForm();
            });

            ImportCommand = new DelegateCommand(_ =>
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
                    if (sender is SaveFileDialog _dialog && !string.IsNullOrEmpty(_dialog.FileName))
                    {
                        var view = windowFactory.Create<ImportUserFromBackupView>(true);
                        if (view.DataContext is null)
                        {
                            var viewModel = viewModelFactory.Create<ImportUserFromBackupViewModel>();
                            viewModel.BackupFilePath = _dialog.FileName;
                            view.DataContext = viewModel;
                        }
                        view.ShowDialog();
                    }
                };

                dialog.ShowDialog();
            });

            RegisterCommand = new DelegateCommand(async commandParam =>
            {
                if (pendingAttempt
                    || string.IsNullOrEmpty(Username)
                    || string.IsNullOrEmpty(password1)
                    || string.IsNullOrEmpty(password2)
                    || password1 != password2 || password1.Length < 7)
                {
                    return;
                }

                pendingAttempt = true;
                UIEnabled = false;

                var loadingScreen = new GeneratingKeyView { Topmost = true };
                loadingScreen.Show();

                bool keyGenerationSuccessful = await keygen.GenerateKeyPair(Paths.KEYS_DIRECTORY);
                if (keyGenerationSuccessful)
                {
                    var userCreationResponse = await userService.CreateUser(
                        passwordHash: password1.SHA512(),
                        publicKeyXml: File.ReadAllText(Paths.PUBLIC_KEY_PATH).PemToXml(),
                        creationSecret: Encoding.UTF8.GetString(gzip.Decompress(File.ReadAllBytes("UserCreator.dat"), new CompressionSettings())));

                    // Handle this event back in the main view model,
                    // since it's there where the backup codes + 2FA secret (QR) will be shown.
                    eventAggregator.GetEvent<UserCreationSucceededEvent>().Publish(userCreationResponse);
                    logger.LogMessage($"Created user {userCreationResponse.Id}.");

                    settings[nameof(Username)] = Username;
                    settings.Save();
                }
                else
                {
                    string errorMsg = "There was an unexpected error whilst generating the RSA key pair (during user creation process).";
                    logger.LogError(errorMsg);
                    ErrorMessage = errorMsg;
                }

                UIEnabled = true;
                loadingScreen.Close();
                pendingAttempt = false;
                password1 = password2 = null;
            });

            QuitCommand = new DelegateCommand(_ => { Application.Current.Shutdown(); });

            errorMessageTimer.Elapsed += (_, __) => ErrorMessage = null;
            errorMessageTimer.Start();
        }

        ~UserCreationViewModel()
        {
            password1 = null;
            password2 = null;
        }

        private void ValidateForm() => FormValid = !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(password1) && !string.IsNullOrEmpty(password2) && password1 == password2 && password1.Length > 7;
    }
}
