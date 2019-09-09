/*
    Glitched Epistle - Windows Client
    Copyright (C) 2019 Raphael Beck

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

using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.Services.Cryptography.Symmetric;
using GlitchedPolygons.Services.Cryptography.Asymmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Utilities;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;

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
        private readonly IAsymmetricKeygenRSA keygen;
        private readonly ISymmetricCryptography crypto;
        private readonly ICompressionUtility gzip;
        private readonly IWindowFactory windowFactory;
        private readonly IViewModelFactory viewModelFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly Timer errorMessageTimer = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };
        private static readonly RSAKeySize RSA_KEY_SIZE = new RSA4096();
        private static readonly CompressionSettings COMPRESSION_SETTINGS = new CompressionSettings();
        #endregion

        #region Commands
        public ICommand PasswordChangedCommand1 { get; }
        public ICommand PasswordChangedCommand2 { get; }
        public ICommand LoginCommand { get; }
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

        private string userCreationSecret = string.Empty;
        public string UserCreationSecret
        {
            get => userCreationSecret;
            set => Set(ref userCreationSecret, value);
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

        public UserCreationViewModel(IUserService userService, ISettings settings, IEventAggregator eventAggregator, ICompressionUtility gzip, ILogger logger, IAsymmetricKeygenRSA keygen, IViewModelFactory viewModelFactory, IWindowFactory windowFactory, ISymmetricCryptography crypto)
        {
            this.gzip = gzip;
            this.logger = logger;
            this.keygen = keygen;
            this.settings = settings;
            this.userService = userService;
            this.windowFactory = windowFactory;
            this.crypto = crypto;
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

            RegisterCommand = new DelegateCommand(OnClickedRegister);
            LoginCommand = new DelegateCommand(OnClickedAlreadyHaveAnAccount);
            QuitCommand = new DelegateCommand(_ => Application.Current.Shutdown());

            errorMessageTimer.Elapsed += (_, __) => ErrorMessage = null;
            errorMessageTimer.Start();
        }

        ~UserCreationViewModel()
        {
            password1 = null;
            password2 = null;
        }

        private void OnClickedAlreadyHaveAnAccount(object commandParam)
        {
            eventAggregator.GetEvent<LogoutEvent>().Publish();
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
                Tuple<string, string> keyPair = await keygen.GenerateKeyPair(RSA_KEY_SIZE);
                
                if (keyPair != null)
                {
                    try
                    {
                        string publicKeyPem = keyPair.Item1;
                        string privateKeyPem = keyPair.Item2;

                        var userCreationResponse = await userService.CreateUser(new UserCreationRequestDto
                        {
                            PasswordSHA512 = password1.SHA512(),
                            PublicKey = KeyExchangeUtility.CompressPublicKey(publicKeyPem),
                            PrivateKey = KeyExchangeUtility.EncryptAndCompressPrivateKey(privateKeyPem, password1),
                            CreationSecret = UserCreationSecret
                        });

                        if (userCreationResponse is null)
                        {
                            logger.LogError("The user creation process failed server-side. Reason unknown; please make an admin check out the server's log files!");
                            ExecUI(() =>
                            {
                                UIEnabled = true;
                                pendingAttempt = false;
                                loadingScreen?.Close();
                            });
                            return;
                        }

                        // Handle this event back in the main view model,
                        // since it's there where the backup codes + 2FA secret (QR) will be shown.
                        ExecUI(() => eventAggregator.GetEvent<UserCreationSucceededEvent>().Publish(userCreationResponse));
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

                ExecUI(() =>
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