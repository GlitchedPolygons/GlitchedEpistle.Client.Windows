﻿using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class LoginViewModel : ViewModel
    {
        #region Constants
        private readonly User user;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        #endregion

        #region Commands
        public ICommand LoginCommand { get; }
        public ICommand QuitCommand { get; }
        public ICommand PasswordChangedCommand { get; }
        #endregion

        #region UI Bindings
        private string userId = string.Empty;
        public string UserId { get => userId; set => Set(ref userId, value); }

        private string totp = string.Empty;
        public string Totp { get => totp; set => Set(ref totp, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }
        #endregion

        private string password;
        private bool pendingAttempt = false;
        private Timer ErrorMessageTimer { get; } = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };

        public LoginViewModel(IUserService userService, ISettings settings, IEventAggregator eventAggregator, User user)
        {
            this.user = user;
            this.settings = settings;
            this.userService = userService;
            this.eventAggregator = eventAggregator;

            QuitCommand = new DelegateCommand(OnClickedQuit);
            LoginCommand = new DelegateCommand(OnClickedLogin);
            PasswordChangedCommand = new DelegateCommand(OnChangedPassword);

            ErrorMessageTimer.Elapsed += ErrorMessageTimer_Elapsed;
            ErrorMessageTimer.Start();
        }

        private void ErrorMessageTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ErrorMessage != null)
                ErrorMessage = null;
        }

        private void OnChangedPassword(object commandParam)
        {
            if (commandParam is PasswordBox passwordBox)
            {
                password = passwordBox.Password;
            }
        }

        private async void OnClickedLogin(object commandParam)
        {
            if (pendingAttempt
                || string.IsNullOrEmpty(UserId)
                || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(Totp))
            {
                return;
            }

            pendingAttempt = true;

            string jwt = await userService.Login(UserId, password.SHA512(), Totp);
            if (!string.IsNullOrEmpty(jwt))
            {
                user.Token = new Tuple<DateTime, string>(DateTime.UtcNow, jwt);
                eventAggregator.GetEvent<LoginSucceededEvent>().Publish();
            }
            else
            {
                ErrorMessageTimer.Stop();
                ErrorMessageTimer.Start();
                ErrorMessage = "Error! Invalid user id, password or 2FA."; // TODO: display lockout message (when login failed >5 times).
            }

            pendingAttempt = false;
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
