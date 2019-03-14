using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Models
{
    /// <summary>
    /// Thread-safe <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements that the collection should hold.</typeparam>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{T}" />
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        /// <inheritdoc />
        public AsyncObservableCollection()
        {
        }

        /// <inheritdoc />
        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        {
        }

        /// <inheritdoc />
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SynchronizationContext.Current == _synchronizationContext)
            {
                // Execute the CollectionChanged event on the current thread
                RaiseCollectionChanged(e);
            }
            else
            {
                // Raises the CollectionChanged event on the creator thread
                _synchronizationContext.Send(RaiseCollectionChanged, e);
            }
        }

        private void RaiseCollectionChanged(object param)
        {
            // We are in the creator thread, call the base implementation directly
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (SynchronizationContext.Current == _synchronizationContext)
            {
                // Execute the PropertyChanged event on the current thread
                RaisePropertyChanged(e);
            }
            else
            {
                // Raises the PropertyChanged event on the creator thread
                _synchronizationContext.Send(RaisePropertyChanged, e);
            }
        }

        private void RaisePropertyChanged(object param)
        {
            // We are in the creator thread, call the base implementation directly
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }
    }
}
