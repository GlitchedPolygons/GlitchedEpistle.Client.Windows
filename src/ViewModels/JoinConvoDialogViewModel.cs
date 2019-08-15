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
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.Services.Cryptography.Asymmetric;

using Newtonsoft.Json;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class JoinConvoDialogViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly Timer messageTimer = new Timer(7000) { AutoReset = true };

        // Injections:
        private readonly User user;
        private readonly ICompressionUtility gzip;
        private readonly IConvoService convoService;
        private readonly IConvoPasswordProvider convoPasswordProvider;
        private readonly IAsymmetricCryptographyRSA crypto;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand JoinCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region UI Bindings
        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }

        private string convoId = string.Empty;
        public string ConvoId
        {
            get => convoId;
            set => Set(ref convoId, value);
        }

        private bool uiEnabled = true;
        public bool UIEnabled
        {
            get => uiEnabled;
            set => Set(ref uiEnabled, value);
        }
        #endregion

        public JoinConvoDialogViewModel(IConvoService convoService, IEventAggregator eventAggregator, User user, IConvoPasswordProvider convoPasswordProvider, ICompressionUtility gzip, IAsymmetricCryptographyRSA crypto)
        {
            this.user = user;
            this.gzip = gzip;
            this.crypto = crypto;
            this.convoService = convoService;
            this.eventAggregator = eventAggregator;
            this.convoPasswordProvider = convoPasswordProvider;

            JoinCommand = new DelegateCommand(OnClickedJoinConvo);
            CancelCommand = new DelegateCommand(_ => RequestedClose?.Invoke(this, EventArgs.Empty));

            messageTimer.Elapsed += (_, __) => ErrorMessage = null;
            messageTimer.Start();
        }

        private void ResetMessages()
        {
            messageTimer.Stop();
            messageTimer.Start();
            ErrorMessage = null;
        }

        private void OnClickedJoinConvo(object commandParam)
        {
            var passwordBox = commandParam as PasswordBox;
            if (passwordBox == null)
            {
                return;
            }

            string passwordSHA512 = passwordBox.Password?.SHA512();
            if (passwordSHA512.NullOrEmpty())
            {
                return;
            }

            UIEnabled = false;

            Task.Run(async () =>
            {
                var dto = new ConvoJoinRequestDto
                {
                    ConvoId = this.ConvoId,
                    ConvoPasswordSHA512 = passwordSHA512
                };

                var body = new EpistleRequestBody
                {
                    UserId = user.Id,
                    Auth = user.Token.Item2,
                    Body = JsonConvert.SerializeObject(dto)
                };

                if (!await convoService.JoinConvo(body.Sign(crypto, user.PrivateKeyPem)))
                {
                    convoPasswordProvider.RemovePasswordSHA512(ConvoId);

                    ExecUI(() =>
                    {
                        ResetMessages();
                        ErrorMessage = "ERROR: Couldn't join convo. Please double check the credentials and try again. If that's not the problem, then the convo might have expired, deleted or you've been kicked out of it. Sorry :/";
                        UIEnabled = true;
                    });

                    return;
                }

                ConvoMetadataDto metadata = await convoService.GetConvoMetadata(ConvoId, passwordSHA512, user.Id, user.Token.Item2);
                convoPasswordProvider.SetPasswordSHA512(ConvoId, passwordSHA512);

                ExecUI(() =>
                {
                    UIEnabled = true;
                    eventAggregator.GetEvent<JoinedConvoEvent>().Publish(metadata);
                    RequestedClose?.Invoke(this, EventArgs.Empty);
                });
            });
        }
    }
}
