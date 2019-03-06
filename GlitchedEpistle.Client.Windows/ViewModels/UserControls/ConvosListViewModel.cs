using System.Windows.Input;
using System.Collections.ObjectModel;

using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;

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
        #endregion

        #region UI Bindings
        private ObservableCollection<Convo> convos;
        public ObservableCollection<Convo> Convos { get => convos; set => Set(ref convos, value); }
        #endregion

        public ConvosListViewModel(IConvoProvider convoProvider, IEventAggregator eventAggregator, IWindowFactory windowFactory, IViewModelFactory viewModelFactory)
        {
            this.windowFactory = windowFactory;
            this.viewModelFactory = viewModelFactory;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;

            Convos = new ObservableCollection<Convo>(convoProvider.Convos);

            OpenConvoCommand = new DelegateCommand(OnOpenConvo);

            eventAggregator.GetEvent<JoinedConvoEvent>().Subscribe(OnJoinedConvo);
            eventAggregator.GetEvent<ConvoCreationSucceededEvent>().Subscribe(OnCreatedConvoSuccessfully);
        }

        private void UpdateList()
        {
            Convos = new ObservableCollection<Convo>(convoProvider.Convos);
        }

        private void OnJoinedConvo(Convo convo)
        {
            UpdateList();
        }

        private void OnCreatedConvoSuccessfully(string convoId)
        {
            UpdateList();
        }

        private void OnOpenConvo(object commandParam)//TODO: raise this via binding inside convolist view xaml
        {
            var convo = commandParam as Convo;
            if (convo is null)
            {
                return;
            }

            var view = windowFactory.Create<JoinConvoDialogView>(true);
            var viewModel= viewModelFactory.Create<JoinConvoDialogViewModel>();
            viewModel.ConvoId = convo.Id;

            if (view.DataContext is null)
                view.DataContext = viewModel;

            view.ShowDialog();
        }
    }
}
