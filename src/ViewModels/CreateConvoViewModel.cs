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
using System.IO;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.RepositoryPattern;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.Services.Cryptography.Asymmetric;

using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class CreateConvoViewModel : ViewModel, ICloseable
    {
        #region Constants
        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly ILocalization localization;
        private readonly IConvoService convoService;
        private readonly IEventAggregator eventAggregator;
        private readonly IAsymmetricCryptographyRSA crypto;
        private readonly ICompressionUtility compressionUtility;
        #endregion

        private readonly IRepository<Convo, string> convoProvider;

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand ClosedCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand PasswordChangedCommand { get; }
        public ICommand Password2ChangedCommand { get; }
        #endregion

        #region UI Bindings
        private string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        private string description;
        public string Description
        {
            get => description;
            set => Set(ref description, value);
        }

        private DateTime minExpirationUTC = DateTime.UtcNow.AddDays(2);
        public DateTime MinExpirationUTC
        {
            get => minExpirationUTC;
            set => Set(ref minExpirationUTC, value);
        }
        
        private DateTime maxExpirationUTC = DateTime.UtcNow.AddDays(20);
        public DateTime MaxExpirationUTC
        {
            get => maxExpirationUTC;
            set => Set(ref maxExpirationUTC, value);
        }
        
        private DateTime expirationUTC = DateTime.UtcNow.AddDays(14);
        public DateTime ExpirationUTC
        {
            get => expirationUTC;
            set => Set(ref expirationUTC, value);
        }

        private bool canSubmit = true;
        public bool CanSubmit
        {
            get => canSubmit;
            set => Set(ref canSubmit, value);
        }
        #endregion

        private string pw = string.Empty;
        private string pw2 = string.Empty;

        public CreateConvoViewModel(User user, ILogger logger, ILocalization localization, IEventAggregator eventAggregator, IConvoService convoService, ICompressionUtility compressionUtility, IAsymmetricCryptographyRSA crypto)
        {
            this.user = user;
            this.logger = logger;
            this.crypto = crypto;
            this.localization = localization;
            this.convoService = convoService;
            this.eventAggregator = eventAggregator;
            this.compressionUtility = compressionUtility;

            convoProvider = new ConvoRepositorySQLite($"Data Source={Path.Combine(Paths.GetConvosDirectory(user.Id), "_metadata.db")};Version=3;");

            SubmitCommand = new DelegateCommand(OnSubmit);
            CancelCommand = new DelegateCommand(OnClickedCancel);
            PasswordChangedCommand = new DelegateCommand(pwBox => pw = (pwBox as PasswordBox)?.Password);
            Password2ChangedCommand = new DelegateCommand(pwBox => pw2 = (pwBox as PasswordBox)?.Password);

            Task.Run(async () =>
            {
                int maxDays = await convoService.GetMaximumConvoDurationDays().ConfigureAwait(false);
                MaxExpirationUTC = maxDays > 0 ? DateTime.UtcNow.AddDays(maxDays) : DateTime.MaxValue;
            });
        }

        private void OnClosed(object commandParam)
        {
            //nop
        }

        private void OnClickedCancel(object commandParam)
        {
            RequestedClose?.Invoke(null, EventArgs.Empty);
        }

        private void OnSubmit(object commandParam)
        {
            string totp = commandParam as string;

            if (totp.NullOrEmpty())
            {
                ErrorMessage = localization["NoTwoFactorAuthTokenProvided"];
                return;
            }

            CanSubmit = false;

            if (pw != pw2)
            {
                ErrorMessage = localization["ConvoCreationPasswordsDontMatch"];
                CanSubmit = true;
                return;
            }

            if (pw.Length < 5)
            {
                ErrorMessage = localization["ConvoCreationPasswordTooWeak"];
                CanSubmit = true;
                return;
            }

            Task.Run(async () =>
            {
                var convoCreationDto = new ConvoCreationRequestDto
                {
                    Totp = totp,
                    Name = Name,
                    Description = Description,
                    ExpirationUTC = ExpirationUTC.ToUnixTimeSeconds(),
                    PasswordSHA512 = pw.SHA512()
                };

                var body = new EpistleRequestBody
                {
                    UserId = user.Id,
                    Auth = user.Token.Item2,
                    Body = compressionUtility.Compress(JsonSerializer.Serialize(convoCreationDto))
                };

                string id = await convoService.CreateConvo(body.Sign(crypto, user.PrivateKeyPem));

                if (id.NotNullNotEmpty())
                {
                    // Create the convo model object and feed it into the convo provider.
                    var convo = new Convo
                    {
                        Id = id,
                        Name = Name,
                        CreatorId = user.Id,
                        CreationUTC = DateTime.UtcNow.ToUnixTimeSeconds(),
                        ExpirationUTC = ExpirationUTC.ToUnixTimeSeconds(),
                        Description = Description,
                        Participants = new List<string> { user.Id }
                    };

                    // Add the created convo to the convo 
                    // provider instance and write it out to disk.
                    await convoProvider.Add(convo);

                    // Raise the convo created event application-wide 
                    // (the main view will subscribe to this to update its list).
                    ExecUI(() =>
                    {
                        eventAggregator.GetEvent<ConvoCreationSucceededEvent>().Publish(id);

                        // Display success message and keep UI disabled.
                        CanSubmit = false;
                        SuccessMessage = $"The convo \"{Name}\" has been created successfully under the id \"{id}\". You can close this window now.";
                    });

                    // Record the convo creation into the user log.
                    logger.LogMessage($"Convo \"{Name}\" created successfully under the id {id}.");
                }
                else
                {
                    // If convo creation failed for some reason, the returned
                    // id string is null and the user is notified accordingly.
                    CanSubmit = true;
                    ErrorMessage = string.Format(localization["ConvoCreationFailedServerSide"], Name);
                    logger.LogError($"Convo \"{Name}\" couldn't be created. Probably 2FA token wrong or expired.");
                }
            });
        }
    }
}
