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
using System.Diagnostics;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ExpirationReminderViewModel : ViewModel
    {
        #region Commands
        public ICommand RedeemButtonCommand { get; }
        public ICommand BuyButtonCommand { get; }
        #endregion

        #region UI Bindings
        private string reminderText;
        public string ReminderText { get => reminderText; set => Set(ref reminderText, value); }
        #endregion

        public ExpirationReminderViewModel(User user, IWindowFactory windowFactory, IViewModelFactory viewModelFactory)
        {
            BuyButtonCommand = new DelegateCommand(_ => Process.Start("https://www.glitchedpolygons.com/extend-epistle-sub"));
            RedeemButtonCommand = new DelegateCommand(_ => windowFactory.OpenWindow<SettingsView, SettingsViewModel>(true, true));

            ReminderText = $"Your Epistle membership ends {(user.ExpirationUTC - DateTime.UtcNow).TotalDays} day(s) from now; the {user.ExpirationUTC.Day}. of {GetMonthName(user.ExpirationUTC.Month)}, {user.ExpirationUTC.Year} at {user.ExpirationUTC:HH:mm} (UTC).";
        }

        private string GetMonthName(int month)
        {
            switch (Math.Abs(month))
            {
                default:
                    return null;
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
            }
        }
    }
}
