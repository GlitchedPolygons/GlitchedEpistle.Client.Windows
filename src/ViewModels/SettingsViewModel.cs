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

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class SettingsViewModel : ViewModel, ICloseable
    {
        #region Constants
        // Injections:
        private readonly User user;
        private readonly IAppSettings appSettings;
        private readonly IUserSettings userSettings;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Variables
        private bool cancelled;
        #endregion

        #region Events        
        /// <summary>
        /// Occurs when the <see cref="SettingsView"/> is requested to be closed
        /// (raise this <c>event</c> in this <c>class</c> here to request the related <see cref="Window"/>'s closure).
        /// </summary>
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand ClosedCommand { get; }
        public ICommand ChangeThemeCommand { get; }
        public ICommand DeleteButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }
        public ICommand RevertButtonCommand { get; }
        #endregion

        #region UI Bindings
        private string username;
        public string Username
        {
            get => username;
            set => Set(ref username, value);
        }
        #endregion

        private string oldTheme, newTheme, oldUsername;

        public SettingsViewModel(IAppSettings appSettings, IUserSettings userSettings, IEventAggregator eventAggregator, User user)
        {
            this.user = user;
            this.appSettings = appSettings;
            this.userSettings = userSettings;
            this.eventAggregator = eventAggregator;

            ClosedCommand = new DelegateCommand(OnClosed);
            ChangeThemeCommand = new DelegateCommand(OnChangedTheme);
            DeleteButtonCommand = new DelegateCommand(OnClickedDelete);
            CancelButtonCommand = new DelegateCommand(OnClickedCancel);
            RevertButtonCommand = new DelegateCommand(OnClickedRevert);

            oldUsername = this.userSettings.Username;

            // Load up the current settings into the UI on load.
            Username = userSettings.Username;
            oldTheme = newTheme = appSettings["Theme", Themes.DARK_THEME];
            OnChangedTheme(oldTheme);
        }

        private void OnClosed(object commandParam)
        {
            if (cancelled)
            {
                userSettings.Username = oldUsername;
                OnChangedTheme(oldTheme);
            }
            else
            {
                userSettings.Username = Username;
                appSettings["Theme"] = newTheme;
            }
        }

        private void OnClickedCancel(object commandParam)
        {
            cancelled = true;
            RequestedClose?.Invoke(this, EventArgs.Empty);
        }

        private void OnClickedRevert(object commandParam)
        {
            Username = user?.Id ?? "user";
            OnChangedTheme(Themes.DARK_THEME);
        }

        private void OnChangedTheme(object commandParam)
        {
            string theme = commandParam as string;
            if (theme.NullOrEmpty())
            {
                return;
            }

            var app = Application.Current as App;
            if (app is null)
            {
                return;
            }

            if (app.ChangeTheme(theme))
            {
                newTheme = theme;
            }
        }

        private void OnClickedDelete(object commandParam)
        {
            var dialog = new ConfirmResetView();
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult is true)
            {
                cancelled = true;

                // Handle this event inside the MainViewModel to prevent
                // user settings being saved out on application shutdown.
                eventAggregator.GetEvent<ResetConfirmedEvent>().Publish();
            }
        }
    }
}
