﻿using System.Windows.Input;
using System.Globalization;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        #region Constants
        public const string DEFAULT_USERNAME = "user";
        public const double DEFAULT_UPDATE_FREQUENCY = 500D;
        private readonly ISettings settings;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Variables
        private bool cancelled = false;
        #endregion

        #region Commands
        public ICommand LoadedCommand { get; }
        public ICommand ClosedCommand { get; }
        public ICommand CancelButtonCommand { get; }
        public ICommand RevertButtonCommand { get; }
        #endregion

        #region UI Bindings
        private string username = DEFAULT_USERNAME;
        public string Username { get => username; set => Set(ref username, value); }

        private double updateFrequency = DEFAULT_UPDATE_FREQUENCY;
        public double UpdateFrequency { get => updateFrequency; set => Set(ref updateFrequency, value); }
        #endregion

        public SettingsViewModel(ISettings settings, IEventAggregator eventAggregator)
        {
            this.settings = settings;
            this.eventAggregator = eventAggregator;

            LoadedCommand = new DelegateCommand(OnLoaded);
            ClosedCommand = new DelegateCommand(OnClosed);
            CancelButtonCommand = new DelegateCommand(OnClickedCancel);
            RevertButtonCommand = new DelegateCommand(OnClickedRevert);
        }

        private void OnLoaded(object commandParam)
        {
            if (!settings.Load())
            {
                return;
            }

            Username = settings[nameof(Username), DEFAULT_USERNAME];
            UpdateFrequency = settings[nameof(UpdateFrequency), DEFAULT_UPDATE_FREQUENCY];
        }

        private void OnClosed(object commandParam)
        {
            if (!cancelled)
            {
                settings[nameof(Username)] = Username;
                settings[nameof(UpdateFrequency)] = UpdateFrequency.ToString(CultureInfo.InvariantCulture);
                settings.Save();

                eventAggregator.GetEvent<UsernameChangedEvent>().Publish(Username);
            }
        }

        private void OnClickedCancel(object commandParam)
        {
            cancelled = true;
        }

        private void OnClickedRevert(object commandParam)
        {
            Username = DEFAULT_USERNAME;
            UpdateFrequency = DEFAULT_UPDATE_FREQUENCY;
        }
    }
}
