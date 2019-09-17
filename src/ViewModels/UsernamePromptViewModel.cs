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
using System.Windows.Input;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    /// <summary>
    /// View model for the missing username prompt (dialog window).
    /// </summary>
    public class UsernamePromptViewModel : ViewModel, ICloseable
    {
        #region Constants
        // Injections:
        private readonly IUserSettings userSettings;
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand AcceptCommand { get; }
        #endregion

        #region UI Bindings
        private string username = string.Empty;
        public string Username { get => username; set => Set(ref username, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }

        private bool uiEnabled = true;
        public bool UIEnabled { get => uiEnabled; set => Set(ref uiEnabled, value); }
        #endregion

        public UsernamePromptViewModel(IUserSettings userSettings)
        {
            this.userSettings = userSettings;

            AcceptCommand = new DelegateCommand(OnAccept);
        }

        private void OnAccept(object commandParam)
        {
            if (Username.NullOrEmpty())
            {
                return;
            }

            userSettings.Username = Username;
            RequestedClose?.Invoke(this,EventArgs.Empty);
        }
    }
}
