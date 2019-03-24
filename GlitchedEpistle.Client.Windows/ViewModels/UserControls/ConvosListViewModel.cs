using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ConvosListViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly IWindowFactory windowFactory;
        private readonly IViewModelFactory viewModelFactory;
        private readonly IConvoProvider convoProvider;
        private readonly IEventAggregator eventAggregator;
        #endregion

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
        #endregion

        public ConvosListViewModel(IConvoProvider convoProvider, IEventAggregator eventAggregator, IWindowFactory windowFactory, IViewModelFactory viewModelFactory)
        {
            this.windowFactory = windowFactory;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;
            this.viewModelFactory = viewModelFactory;

            UpdateList();

            OpenConvoCommand = new DelegateCommand(OnClickedOnConvo);
            EditConvoCommand = new DelegateCommand(OnClickedEditConvo);
            CopyConvoIdCommand = new DelegateCommand(OnClickedCopyConvoIdToClipboard);

            eventAggregator.GetEvent<JoinedConvoEvent>().Subscribe(_ => UpdateList());
            eventAggregator.GetEvent<DeletedConvoEvent>().Subscribe(_ => UpdateList());
            eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Subscribe(_ => UpdateList());
            eventAggregator.GetEvent<ConvoCreationSucceededEvent>().Subscribe(_ => UpdateList());
        }

        private void UpdateList()
        {
            convoProvider.Load();
            Convos = convoProvider.Convos != null ? new ObservableCollection<Convo>(convoProvider.Convos.OrderBy(c => c.Name)) : new ObservableCollection<Convo>();
        }

        private void OnClickedOnConvo(object commandParam)
        {
            var convo = commandParam as Convo;
            if (convo is null)
            {
                return;
            }

            var view = windowFactory.Create<JoinConvoDialogView>(true);
            var viewModel = viewModelFactory.Create<JoinConvoDialogViewModel>();
            viewModel.ConvoId = convo.Id;

            if (view.DataContext is null)
            {
                view.DataContext = viewModel;
            }

            view.ShowDialog();
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