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
