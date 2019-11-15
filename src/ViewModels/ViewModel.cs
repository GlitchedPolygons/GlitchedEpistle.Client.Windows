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
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

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
            ExecUI(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            return true;
        }

        /// <summary>
        /// Shorthand for <c>Application.Current?.Dispatcher?.Invoke(Action, DispatcherPriority);</c>
        /// </summary>
        /// <param name="action">What you want to execute on the UI thread.</param>
        /// <param name="priority">The <see cref="DispatcherPriority"/> with which to execute the <see cref="Action"/> on the UI thread.</param>
        protected static void ExecUI(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            Application.Current?.Dispatcher?.Invoke(action, priority);
        }
    }
}
