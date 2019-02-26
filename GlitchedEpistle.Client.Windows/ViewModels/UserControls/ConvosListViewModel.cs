using System.IO;
using System.Windows.Input;
using System.Collections.Generic;

using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ConvosListViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly IConvoProvider convoProvider;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Commands
        public ICommand OpenConvoCommand { get; }
        public ICommand DeleteConvoCommand { get; }
        #endregion

        #region UI Bindings
        private ICollection<Convo> convos;
        public ICollection<Convo> Convos { get => convos; set => Set(ref convos, value); }
        #endregion

        public ConvosListViewModel(IConvoProvider convoProvider, IEventAggregator eventAggregator)
        {
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;

            Convos = convoProvider.Convos;

            OpenConvoCommand = new DelegateCommand(OnOpenConvo);
            DeleteConvoCommand = new DelegateCommand(OnDeleteConvo);

            eventAggregator.GetEvent<JoinedConvoEvent>().Subscribe(OnJoinedConvo);
            eventAggregator.GetEvent<ConvoCreationSucceededEvent>().Subscribe(OnCreatedConvoSuccessfully);
        }

        private void OnJoinedConvo(Convo convo)
        {
            if (convo is null 
                || string.IsNullOrEmpty(convo.Id) 
                || convoProvider[convo.Id] != null)
            {
                return;
            }

            convoProvider.Convos.Add(convo);
            Convos = convoProvider.Convos;
        }

        private void OnOpenConvo(object commandParam)
        {
            var convo = commandParam as Convo;
            if (convo is null)
            {
                return;
            }

            // TODO: ask for password and then dispatch OnOpenedConvo event via aggregator.
        }

        // This deletes the convo data from the device but does NOT make the user leave the convo!
        private void OnDeleteConvo(object commandParam)
        {
            var convo = commandParam as Convo;
            if (convo is null)
            {
                return;
            }

            // TODO:
            // show confirmation dialog here ("are you sure?").
            // Remember to explain that it will delete the convo data
            // from device but not make user leave!
            // (That needs to be done manually inside the open convo).
            
            convoProvider.Convos.Remove(convo);
            Convos = convoProvider.Convos;

            string path = Path.Combine(Paths.CONVOS_DIRECTORY, convo.Id);

            if (File.Exists(path))
                File.Delete(path);

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        private void OnCreatedConvoSuccessfully(string convoId)
        {
            var convo = convoProvider[convoId];
            if (convo != null)
            {
                // TODO: add convo entry to list
            }
        }
    }
}
