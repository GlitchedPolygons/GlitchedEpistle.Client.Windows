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
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.Services.Cryptography.Asymmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Convos;

using Newtonsoft.Json;

using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class JoinConvoDialogViewModel : ViewModel, ICloseable
    {
        #region Constants
        // Injections:
        private readonly User user;
        private readonly IConvoService convoService;
        private readonly ILocalization localization;
        private readonly IEventAggregator eventAggregator;
        private readonly IAsymmetricCryptographyRSA crypto;
        private readonly IConvoPasswordProvider convoPasswordProvider;
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand JoinCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region UI Bindings
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

        public JoinConvoDialogViewModel(User user, IConvoService convoService, IEventAggregator eventAggregator, ILocalization localization, IConvoPasswordProvider convoPasswordProvider, IAsymmetricCryptographyRSA crypto)
        {
            this.user = user;
            this.crypto = crypto;
            this.localization = localization;
            this.convoService = convoService;
            this.eventAggregator = eventAggregator;
            this.convoPasswordProvider = convoPasswordProvider;

            JoinCommand = new DelegateCommand(OnClickedJoinConvo);
            CancelCommand = new DelegateCommand(_ => RequestedClose?.Invoke(this, EventArgs.Empty));
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
                    ErrorMessage = localization["CouldNotJoinConvoPleaseDoubleCheckCredentials"];
                    UIEnabled = true;
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
