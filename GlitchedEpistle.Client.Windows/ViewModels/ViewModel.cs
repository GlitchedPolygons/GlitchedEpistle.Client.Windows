using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    /// <summary>
    /// ViewModel base class.
    /// Implements <see cref="System.ComponentModel.INotifyPropertyChanged" />.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Raised when a viewmodel's property value changes (via <see cref="Set{T}"/>).
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the specified field to a new value.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="field">The field to update.</param>
        /// <param name="newValue">The new value to give to the field.</param>
        /// <param name="propertyName">Name of the property (use the nameof operator when possible).</param>
        /// <returns><see langword="true"/> if the property needed to be updated (and thus received a new value), <see langword="false"/> otherwise.</returns>
        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
