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
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class UserCreationViewModel : ViewModel
    {
        #region Constants
        private readonly ILogger logger;
        private readonly IAppSettings appSettings;
        private readonly ILocalization localization;
        private readonly IUserSettings userSettings;
        private readonly IEventAggregator eventAggregator;
        private readonly IRegistrationService registrationService;
        #endregion

        #region Commands
        public ICommand EditServerUrlCommand { get; }
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

        private Visibility userCreationSecretFieldVis = Visibility.Visible;
        public Visibility UserCreationSecretFieldVis
        {
            get => userCreationSecretFieldVis;
            set => Set(ref userCreationSecretFieldVis, value);
        }
        #endregion

        private volatile bool pendingAttempt;
        private volatile string password1, password2;

        public UserCreationViewModel(IUserSettings userSettings, ILocalization localization, IAppSettings appSettings, IEventAggregator eventAggregator, ILogger logger, IRegistrationService registrationService)
        {
            this.logger = logger;
            this.appSettings = appSettings;
            this.userSettings = userSettings;
            this.localization = localization;
            this.eventAggregator = eventAggregator;
            this.registrationService = registrationService;

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
            EditServerUrlCommand = new DelegateCommand(_ =>
            {
                eventAggregator.GetEvent<ClickedConfigureServerUrlButtonEvent>().Publish();
            });

            bool onOfficialServer = appSettings.ServerUrl.Contains("epistle.glitchedpolygons.com");

            UserCreationSecretFieldVis = onOfficialServer ? Visibility.Collapsed : Visibility.Visible;

            if (onOfficialServer)
            {
                UserCreationSecret = "Freedom";
            }
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
            if (pendingAttempt || Username.NullOrEmpty() || password1.NullOrEmpty() || password2.NullOrEmpty())
            {
                return;
            }

            if (password1 != password2)
            {
                ErrorMessage = localization["PasswordsDontMatch"];
                return;
            }

            if (password1.Length <= 7)
            {
                ErrorMessage = localization["PasswordTooWeakErrorMessage"];
                return;
            }

            pendingAttempt = true;
            UIEnabled = false;

            var loadingScreen = new GeneratingKeyView { Topmost = true };
            loadingScreen.Show();

            Task.Run(async () =>
            {
                Tuple<int, UserCreationResponseDto> result = await registrationService.CreateUser(password1, UserCreationSecret);

                switch (result.Item1)
                {
                    case 0: // Success!
                            // TODO: reload user settings before setting username!
                        userSettings.Username = Username;
                        logger?.LogMessage($"Created user {result.Item2.Id}.");
                        // Handle this event back in the main view model,
                        // since it's there where the backup codes + 2FA secret (QR) will be shown.
                        ExecUI(() =>
                        {
                            loadingScreen?.Close();
                            password1 = password2 = null;
                            eventAggregator.GetEvent<UserCreationSucceededEvent>().Publish(result.Item2);
                        });
                        return;

                    case 1: // Epistle backend connectivity issues
                        string errorMsg = localization["CouldNotConnectToServerErrorMessageUserCreationViewModel"];
                        logger?.LogError(errorMsg);
                        ErrorMessage = errorMsg;
                        break;

                    case 2: // RSA failure
                        errorMsg = localization["UnexpectedErrorWhileGeneratingRSAKeyPair"];
                        logger?.LogError(errorMsg);
                        ErrorMessage = errorMsg;
                        break;

                    case 3: // Server-side failure
                        errorMsg = localization["UserCreationFailedServerSide"];
                        logger?.LogError(errorMsg);
                        ErrorMessage = errorMsg;
                        break;

                    case 4: // Client-side failure
                        errorMsg = localization["UserCreationFailedClientSide"];
                        logger?.LogError(errorMsg);
                        ErrorMessage = errorMsg;
                        break;
                }

                UIEnabled = true;
                pendingAttempt = false;
                password1 = password2 = null;
                ExecUI(() => loadingScreen?.Close());
            });
        }

        private void ValidateForm()
        {
            FormValid = Username.NotNullNotEmpty()  &&
                        password1.NotNullNotEmpty() &&
                        password2.NotNullNotEmpty() &&
                        password1 == password2      &&
                        password1.Length > 7;
        }
    }
}
