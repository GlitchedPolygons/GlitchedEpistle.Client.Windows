/*
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

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization;

using Prism.Events;
using System.Diagnostics;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.KeyExchange;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.ServerHealth;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class LoginViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly User user;
        private readonly IKeyExchange keyExchange;
        private readonly IAppSettings appSettings;
        private readonly IUserService userService;
        private readonly ILocalization localization;
        private readonly IEventAggregator eventAggregator;
        private readonly IServerConnectionTest connectionTest;
        #endregion

        #region Commands
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand QuitCommand { get; }
        public ICommand PasswordChangedCommand { get; }
        public ICommand EditServerUrlCommand { get; }
        #endregion

        #region UI Bindings
        private string userId = string.Empty;
        public string UserId { get => userId; set => Set(ref userId, value); }

        private bool uiEnabled = true;
        public bool UIEnabled
        {
            get => uiEnabled;
            set
            {
                Set(ref uiEnabled, value);
                LoggingInVisibility = value ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private bool englishChecked = true;
        public bool EnglishChecked
        {
            get => englishChecked;
            set
            {
                Set(ref englishChecked, value);
                if (initialized && value)
                {
                    localization.SetCurrentCultureInfo(new System.Globalization.CultureInfo("en"));
                }
            }
        }

        private bool germanChecked = false;
        public bool GermanChecked
        {
            get => germanChecked;
            set
            {
                Set(ref germanChecked, value);
                if (initialized && value)
                {
                    localization.SetCurrentCultureInfo(new System.Globalization.CultureInfo("de"));
                }
            }
        }

        private bool swissGermanChecked = false;
        public bool SwissGermanChecked
        {
            get => swissGermanChecked;
            set
            {
                Set(ref swissGermanChecked, value);
                if (initialized && value)
                {
                    localization.SetCurrentCultureInfo(new System.Globalization.CultureInfo("gsw"));
                }
            }
        }

        private bool italianChecked = false;
        public bool ItalianChecked
        {
            get => italianChecked;
            set
            {
                Set(ref italianChecked, value);
                if (initialized && value)
                {
                    localization.SetCurrentCultureInfo(new System.Globalization.CultureInfo("it"));
                }
            }
        }

        private Visibility loggingInVisibility = Visibility.Hidden;
        public Visibility LoggingInVisibility { get => loggingInVisibility; set => Set(ref loggingInVisibility, value); }
        #endregion

        private volatile string password;
        private volatile int failedAttempts;
        private volatile bool pendingAttempt, initialized;

        public LoginViewModel(IAppSettings settings, IEventAggregator eventAggregator, ILocalization localization, IUserService userService, IServerConnectionTest connectionTest, User user, IKeyExchange keyExchange, IAppSettings appSettings)
        {
            this.user = user;
            this.userService = userService;
            this.keyExchange = keyExchange;
            this.appSettings = appSettings;
            this.localization = localization;
            this.connectionTest = connectionTest;
            this.eventAggregator = eventAggregator;

            QuitCommand = new DelegateCommand(OnClickedQuit);
            LoginCommand = new DelegateCommand(OnClickedLogin);

            RegisterCommand = new DelegateCommand(_ => eventAggregator.GetEvent<ClickedRegisterButtonEvent>().Publish());
            EditServerUrlCommand = new DelegateCommand(_ => eventAggregator.GetEvent<ClickedConfigureServerUrlButtonEvent>().Publish());

            UserId = settings.LastUserId;

            // Bind the password box to the password field.
            PasswordChangedCommand = new DelegateCommand(o => password = (o as PasswordBox)?.Password);

            var currentCI = localization.GetCurrentCultureInfo().Name;
            if (currentCI.Contains("de"))
            {
                EnglishChecked = GermanChecked = SwissGermanChecked = ItalianChecked = false;
                GermanChecked = true;
            }
            else if (currentCI.Contains("gsw"))
            {
                EnglishChecked = GermanChecked = SwissGermanChecked = ItalianChecked = false;
                SwissGermanChecked = true;
            }
            else if (currentCI.Contains("it"))
            {
                EnglishChecked = GermanChecked = SwissGermanChecked = ItalianChecked = false;
                ItalianChecked = true;
            }
            else
            {
                EnglishChecked = GermanChecked = SwissGermanChecked = ItalianChecked = false;
                EnglishChecked = true;
            }

            initialized = true;
        }

        private void OnClickedLogin(object commandParam)
        {
            string totp = commandParam as string;

            if (pendingAttempt || UserId.NullOrEmpty() || password.NullOrEmpty() || totp.NullOrEmpty())
            {
                return;
            }

            pendingAttempt = true;
            UIEnabled = false;

            Task.Run(async () =>
            {
                if (!await connectionTest.TestConnection().ConfigureAwait(false))
                {
                    ExecUI(() =>
                    {
                        new InfoDialogView
                        {
                            DataContext = new InfoDialogViewModel
                            {
                                OkButtonText = "Okay :/",
                                Title = localization["EpistleServerUnresponsive"],
                                Text = localization["EpistleServerUnresponsiveErrorMessage"]
                            }
                        }.ShowDialog();
                    });
                    goto end;
                }
                
                var request = new UserLoginRequestDto
                {
                    UserId = UserId,
                    PasswordSHA512 = password.SHA512(),
                    Totp = totp
                };
            
                UserLoginSuccessResponseDto response = await userService.Login(request).ConfigureAwait(false);

                if (response is null || response.Auth.NullOrEmpty() || response.PrivateKey.NullOrEmpty())
                {
                    ErrorMessage = localization["InvalidUserIdPasswordOr2FA"];
                    if (++failedAttempts > 2)
                    {
                        ErrorMessage += "\n" + localization["InvalidUserIdPasswordOr2FA_Detailed"];
                    }
                    goto end;
                }

                try
                {
                    user.Id = appSettings.LastUserId = userId;
                    user.PublicKeyPem = await keyExchange.DecompressPublicKeyAsync(response.PublicKey).ConfigureAwait(false);
                    user.PrivateKeyPem = await keyExchange.DecompressAndDecryptPrivateKeyAsync(response.PrivateKey, password).ConfigureAwait(false);
                    user.Token = new Tuple<DateTime, string>(DateTime.UtcNow, response.Auth);

                    if (response.Message.NotNullNotEmpty())
                    {
                        ExecUI(() =>
                        {
                            var dialog = new InfoDialogView
                            {
                                DataContext = new InfoDialogViewModel
                                {
                                    OkButtonText = localization["Dismiss"],
                                    Title = localization["EpistleServerBroadcastMessage"],
                                    Text = response.Message
                                }, MinWidth = 250, MinHeight = 175
                            };
                            dialog.Show();
                            dialog.Focus();
                        });
                    }
                    
                    failedAttempts = 0;
                    ExecUI(() => eventAggregator.GetEvent<LoginSucceededEvent>().Publish());
                }
                catch
                {
                    failedAttempts++;
                    ErrorMessage = localization["LoginSucceededServerSideButResponseCouldNotBeHandledClientSide"];
                }

                end:
                pendingAttempt = false;
                UIEnabled = true;
            });
        }

        private void OnClickedQuit(object commandParam)
        {
            Application.Current.Shutdown();
        }

        ~LoginViewModel()
        {
            password = null;
        }
    }
}
