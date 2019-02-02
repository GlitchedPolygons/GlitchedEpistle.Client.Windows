using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Globalization;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Coupons;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class SettingsViewModel : ViewModel, ICloseable
    {
        #region Constants
        public const string DEFAULT_USERNAME = "user";
        public const double DEFAULT_UPDATE_FREQUENCY = 500D;

        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly ISettings settings;
        private readonly ICouponService couponService;
        private readonly IEventAggregator eventAggregator;
        private readonly IViewModelFactory viewModelFactory;
        #endregion

        #region Variables
        private bool cancelled = false;
        #endregion

        #region Events        
        /// <summary>
        /// Occurs when the <see cref="SettingsView"/> is requested to be closed
        /// (raise this <see langword="event"/> in this <see langword="class"/> here to request the related <see cref="Window"/>'s closure).
        /// </summary>
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand LoadedCommand { get; }
        public ICommand ClosedCommand { get; }
        public ICommand DeleteButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }
        public ICommand RevertButtonCommand { get; }
        public ICommand RedeemButtonCommand { get; }
        #endregion

        #region UI Bindings
        private string username = DEFAULT_USERNAME;
        public string Username { get => username; set => Set(ref username, value); }

        private string couponCode = string.Empty;
        public string CouponCode { get => couponCode; set => Set(ref couponCode, value); }

        private double updateFrequency = DEFAULT_UPDATE_FREQUENCY;
        public double UpdateFrequency { get => updateFrequency; set => Set(ref updateFrequency, value); }
        #endregion

        public SettingsViewModel(ISettings settings, IEventAggregator eventAggregator, ICouponService couponService, User user, IViewModelFactory viewModelFactory, ILogger logger)
        {
            this.user = user;
            this.logger = logger;
            this.settings = settings;
            this.couponService = couponService;
            this.eventAggregator = eventAggregator;
            this.viewModelFactory = viewModelFactory;

            LoadedCommand = new DelegateCommand(OnLoaded);
            ClosedCommand = new DelegateCommand(OnClosed);
            DeleteButtonCommand = new DelegateCommand(OnClickedDelete);
            CancelButtonCommand = new DelegateCommand(OnClickedCancel);
            RevertButtonCommand = new DelegateCommand(OnClickedRevert);
            RedeemButtonCommand = new DelegateCommand(OnClickedRedeem);
        }

        private void OnLoaded(object commandParam)
        {
            if (!settings.Load())
            {
                return;
            }

            // Load up the current settings into the UI on load.
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
            RequestedClose?.Invoke(this, EventArgs.Empty);
        }

        private void OnClickedRevert(object commandParam)
        {
            Username = DEFAULT_USERNAME;
            UpdateFrequency = DEFAULT_UPDATE_FREQUENCY;
        }

        private async void OnClickedRedeem(object commandParam)
        {
            if (string.IsNullOrEmpty(CouponCode))
            {
                return;
            }

            bool success = await couponService.UseCoupon(CouponCode, user.Id, user.Token.Item2);

            var dialogViewModel = viewModelFactory.Create<InfoDialogViewModel>();

            if (success)
            {
                dialogViewModel.Title = "Success!";
                dialogViewModel.Text = $"Your coupon \"{CouponCode}\" has been redeemed successfully; your Glitched Epistle membership has thus been extended. Thanks for choosing this service!";
                eventAggregator.GetEvent<CouponRedeemedEvent>().Publish();
                logger.LogMessage($"Successfully redeemed Glitched Epistle coupon {CouponCode}");
            }
            else
            {
                dialogViewModel.Title = "Couldn't redeem coupon...";
                dialogViewModel.Text= $"The coupon \"{CouponCode}\" couldn't be redeemed, sorry... Please make sure that you have no typos in it, and keep in mind that its validity is case-sensitive!";
                logger.LogError($"Unsuccessful coupon redeem attempt ({CouponCode})");
            }

            var dialogView = new InfoDialogView { DataContext = dialogViewModel };
            dialogView.ShowDialog();
        }

        private void OnClickedDelete(object commandParam)
        {
            var dialog = new ConfirmResetView();
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult is true)
            {
                // Handle this event inside the MainViewModel to prevent
                // user settings being saved out on application shutdown.
                eventAggregator.GetEvent<ResetConfirmedEvent>().Publish();
            }
        }
    }
}
