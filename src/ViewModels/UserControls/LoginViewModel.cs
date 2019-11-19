﻿/*
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

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class LoginViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly ILoginService loginService;
        private readonly ILocalization localization;
        private readonly IEventAggregator eventAggregator;
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
        public bool UIEnabled { get => uiEnabled; set => Set(ref uiEnabled, value); }
        #endregion

        private volatile string password;
        private volatile int failedAttempts;
        private volatile bool pendingAttempt;

        public LoginViewModel(IAppSettings settings, IEventAggregator eventAggregator, ILoginService loginService, ILocalization localization)
        {
            this.loginService = loginService;
            this.localization = localization;
            this.eventAggregator = eventAggregator;

            QuitCommand = new DelegateCommand(OnClickedQuit);
            LoginCommand = new DelegateCommand(OnClickedLogin);

            RegisterCommand = new DelegateCommand(_ => eventAggregator.GetEvent<ClickedRegisterButtonEvent>().Publish());
            EditServerUrlCommand = new DelegateCommand(_ => eventAggregator.GetEvent<ClickedConfigureServerUrlButtonEvent>().Publish());

            UserId = settings.LastUserId;

            // Bind the password box to the password field.
            PasswordChangedCommand = new DelegateCommand(o => password = (o as PasswordBox)?.Password);
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
                int result = await loginService.Login(UserId, password, totp);

                switch (result)
                {
                    case 0: // Login succeeded.
                        failedAttempts = 0;
                        ExecUI(() => eventAggregator.GetEvent<LoginSucceededEvent>().Publish());
                        break;

                    case 1: // Connection to server failed.
                        ExecUI(() =>
                        {
                            pendingAttempt = false;
                            UIEnabled = true;
                            new InfoDialogView
                            {
                                DataContext = new InfoDialogViewModel()
                                {
                                    OkButtonText = "Okay :/",
                                    Title = localization["EpistleServerUnresponsive"],
                                    Text = localization["EpistleServerUnresponsiveErrorMessage"]
                                }
                            }.ShowDialog();
                        });
                        break;

                    case 2: // Login failed server-side.
                        ErrorMessage = localization["InvalidUserIdPasswordOr2FA"];
                        if (++failedAttempts > 2)
                        {
                            ErrorMessage += "\n" + localization["InvalidUserIdPasswordOr2FA_Detailed"];
                        }
                        break;

                    case 3: // Login failed client-side.
                        failedAttempts++;
                        ErrorMessage = localization["LoginSucceededServerSideButResponseCouldNotBeHandledClientSide"];
                        break;
                }

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
