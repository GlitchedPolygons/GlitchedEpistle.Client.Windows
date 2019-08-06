﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.RepositoryPattern;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ConvosListViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly IWindowFactory windowFactory;
        private readonly IViewModelFactory viewModelFactory;
        private readonly IConvoService convoService;
        private readonly IConvoPasswordProvider convoPasswordProvider;
        private readonly IEventAggregator eventAggregator;
        private readonly User user;
        #endregion

        private IRepository<Convo, string> convoProvider;

        #region Commands
        public ICommand OpenConvoCommand { get; }
        public ICommand EditConvoCommand { get; }
        public ICommand CopyConvoIdCommand { get; }
        #endregion

        #region UI Bindings
        private ObservableCollection<Convo> convos;
        public ObservableCollection<Convo> Convos
        {
            get => convos;
            set => Set(ref convos, value);
        }

        private bool canJoin = true;
        public bool CanJoin
        {
            get => canJoin;
            set => Set(ref canJoin, value);
        }
        #endregion

        public ConvosListViewModel(IEventAggregator eventAggregator, IWindowFactory windowFactory, IViewModelFactory viewModelFactory, IConvoPasswordProvider convoPasswordProvider, User user, IConvoService convoService)
        {
            #region Injections
            this.user = user;
            this.convoService = convoService;
            this.windowFactory = windowFactory;
            this.eventAggregator = eventAggregator;
            this.viewModelFactory = viewModelFactory;
            this.convoPasswordProvider = convoPasswordProvider;
            #endregion

            if (user.Id.NotNullNotEmpty())
            {
                convoProvider = new ConvoRepositorySQLite($"Data Source={Path.Combine(Paths.GetConvosDirectory(user.Id), "_metadata.db")};Version=3;");
                UpdateList();
            }

            OpenConvoCommand = new DelegateCommand(OnClickedOnConvo);
            EditConvoCommand = new DelegateCommand(OnClickedEditConvo);
            CopyConvoIdCommand = new DelegateCommand(OnClickedCopyConvoIdToClipboard);

            eventAggregator.GetEvent<LoginSucceededEvent>().Subscribe(UpdateList);
            eventAggregator.GetEvent<UpdatedUserConvosEvent>().Subscribe(UpdateList);
            eventAggregator.GetEvent<JoinedConvoEvent>().Subscribe(_ => UpdateList());
            eventAggregator.GetEvent<DeletedConvoEvent>().Subscribe(_ => UpdateList());
            eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Subscribe(_ => UpdateList());
            eventAggregator.GetEvent<ConvoCreationSucceededEvent>().Subscribe(_ => UpdateList());
        }

        private void UpdateList()
        {
            convoProvider = new ConvoRepositorySQLite($"Data Source={Path.Combine(Paths.GetConvosDirectory(user.Id), "_metadata.db")};Version=3;");
            var convos = convoProvider.GetAll().GetAwaiter().GetResult();
            Convos = convos != null ? new ObservableCollection<Convo>(convos.OrderBy(c => c.IsExpired()).ThenBy(c => c.Name.ToUpper())) : new ObservableCollection<Convo>();
        }

        private void OnClickedOnConvo(object commandParam)
        {
            var _convo = commandParam as Convo;
            if (_convo is null || !CanJoin)
            {
                return;
            }

            CanJoin = false;
            string cachedPwSHA512 = convoPasswordProvider.GetPasswordSHA512(_convo.Id);

            if (cachedPwSHA512.NotNullNotEmpty())
            {
                Task.Run(async () =>
                {
                    if (!await convoService.JoinConvo(_convo.Id, cachedPwSHA512, user.Id, user.Token.Item2))
                    {
                        convoPasswordProvider.RemovePasswordSHA512(_convo.Id);
                        Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            CanJoin = true;
                            var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Couldn't join convo. Please double check the credentials and try again. If that's not the problem, then the convo might have expired, deleted or you've been kicked out of it. Sorry :/", Title = "Message upload failed" } };
                            errorView.ShowDialog();
                        });
                        return;
                    }

                    ConvoMetadataDto metadata = await convoService.GetConvoMetadata(_convo.Id, cachedPwSHA512, user.Id, user.Token.Item2);
                    if (metadata is null)
                    {
                        return;
                    }

                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        CanJoin = true;
                        eventAggregator.GetEvent<JoinedConvoEvent>().Publish(metadata);
                    });
                });
            }
            else // Password not yet stored in session's IConvoPasswordProvider
            {
                var view = windowFactory.Create<JoinConvoDialogView>(true);
                var viewModel = viewModelFactory.Create<JoinConvoDialogViewModel>();
                viewModel.ConvoId = _convo.Id;

                if (view.DataContext is null)
                {
                    view.DataContext = viewModel;
                }

                view.ShowDialog();
                CanJoin = true;
            }
        }

        private void OnClickedEditConvo(object commandParam)
        {
            var convo = commandParam as Convo;
            if (convo is null)
            {
                return;
            }

            var view = windowFactory.Create<EditConvoMetadataView>(true);
            var viewModel = viewModelFactory.Create<EditConvoMetadataViewModel>();
            viewModel.Convo = convo;

            if (view.DataContext is null)
            {
                view.DataContext = viewModel;
            }

            view.Show();
            view.Focus();
        }

        private void OnClickedCopyConvoIdToClipboard(object commandParam)
        {
            if (commandParam is Convo convo)
            {
                Clipboard.SetText(convo.Id);
            }
        }
    }
}