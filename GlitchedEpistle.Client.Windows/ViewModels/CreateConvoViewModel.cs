using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.RepositoryPattern;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.Services.Cryptography.Asymmetric;

using Newtonsoft.Json;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class CreateConvoViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly Timer messageTimer = new Timer(7000) { AutoReset = true };

        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly IUserService userService;
        private readonly IConvoService convoService;
        private readonly IEventAggregator eventAggregator;
        private readonly ICompressionUtility gzip;
        private readonly IAsymmetricCryptographyRSA crypto;
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

        private DateTime expirationUTC = DateTime.UtcNow.AddDays(14);
        public DateTime ExpirationUTC
        {
            get => expirationUTC;
            set => Set(ref expirationUTC, value);
        }

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }

        private string successMessage = string.Empty;
        public string SuccessMessage
        {
            get => successMessage;
            set => Set(ref successMessage, value);
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

        public CreateConvoViewModel(User user, ILogger logger, IEventAggregator eventAggregator, IUserService userService, IConvoService convoService, ICompressionUtility gzip, IAsymmetricCryptographyRSA crypto)
        {
            this.user = user;
            this.gzip = gzip;
            this.logger = logger;
            this.crypto = crypto;
            this.userService = userService;
            this.convoService = convoService;
            this.eventAggregator = eventAggregator;

            convoProvider = new ConvoRepositorySQLite($"Data Source={Path.Combine(Paths.GetConvosDirectory(user.Id), "_metadata.db")};Version=3;");

            SubmitCommand = new DelegateCommand(OnSubmit);
            CancelCommand = new DelegateCommand(OnClickedCancel);
            PasswordChangedCommand = new DelegateCommand(pwBox => pw = (pwBox as PasswordBox)?.Password);
            Password2ChangedCommand = new DelegateCommand(pwBox => pw2 = (pwBox as PasswordBox)?.Password);
            ClosedCommand = new DelegateCommand(OnClosed);

            messageTimer.Elapsed += (_, __) => ResetMessages();
            messageTimer.Start();
        }

        private void ResetMessages()
        {
            messageTimer.Stop();
            messageTimer.Start();
            ErrorMessage = SuccessMessage = null;
        }

        private void PrintMessage(string message, bool error)
        {
            ResetMessages();

            if (error)
            {
                ErrorMessage = message;
            }
            else
            {
                SuccessMessage = message;
            }
        }

        private void OnClosed(object commandParam)
        {
            // nop
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
                PrintMessage("No 2FA token provided - please take security seriously and authenticate your request!", true);
                return;
            }

            CanSubmit = false;

            if (pw != pw2)
            {
                PrintMessage("The password does not match its confirmation; please make sure that you re-type your password correctly!", true);
                CanSubmit = true;
                return;
            }

            if (pw.Length < 5)
            {
                PrintMessage("Your password is too weak; make sure that it has at least >5 characters!", true);
                CanSubmit = true;
                return;
            }

            Task.Run(async() =>
            {
                var convoCreationDto = new ConvoCreationRequestDto
                {
                    Totp = totp,
                    Name = Name,
                    Description = Description,
                    ExpirationUTC = ExpirationUTC,
                    PasswordSHA512 = pw.SHA512()
                };

                var body = new EpistleRequestBody
                {
                    UserId = user.Id,
                    Auth = user.Token.Item2,
                    Body = gzip.Compress(JsonConvert.SerializeObject(convoCreationDto))
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
                        CreationTimestampUTC = DateTime.UtcNow,
                        Description = Description,
                        ExpirationUTC = ExpirationUTC,
                        Participants = new List<string> { user.Id }
                    };

                    // Add the created convo to the convo 
                    // provider instance and write it out to disk.
                    await convoProvider.Add(convo);

                    // Raise the convo created event application-wide (the main view will subscribe to this to update its list).
                    ExecUI(() =>
                    {
                        eventAggregator.GetEvent<ConvoCreationSucceededEvent>().Publish(id);

                        // Display success message and keep UI disabled.
                        CanSubmit = false;
                        PrintMessage($"The convo {Name} has been created successfully under the id {id}. You can close this window now.", false);
                    });

                    // Record the convo creation into the user log.
                    logger.LogMessage($"Convo {Name} created successfully under the id {id}.");
                }
                else
                {
                    ExecUI(() =>
                    {
                        // If convo creation failed for some reason, the returned
                        // id string is null and the user is notified accordingly.
                        CanSubmit = true;
                        PrintMessage($"Convo {Name} couldn't be created. Perhaps due to unsuccessful Two-Factor Authentication: please double check the provided 2FA token and try again.", true);
                        logger.LogError($"Convo {Name} couldn't be created. Probably 2FA token (\"{totp}\") wrong or expired.");
                    });
                }
            });
        }
    }
}