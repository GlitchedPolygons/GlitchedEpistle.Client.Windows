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
using System.Timers;
using System.Windows;
using System.Windows.Threading;
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
        /// The interval (in milliseconds) between error message resets.
        /// </summary>
        public int ErrorMessageResetInterval { get; set; } = 7000;

        /// <summary>
        /// The interval (in milliseconds) between success message resets.
        /// </summary>
        public int SuccessMessageResetInterval { get; set; } = 7000;

        private Timer errorMsgResetTimer = null;
        private Timer successMsgResetTimer = null;

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMsgResetTimer is null)
                {
                    errorMsgResetTimer = new Timer { Interval = ErrorMessageResetInterval, AutoReset = true };
                    errorMsgResetTimer.Elapsed += (sender, e) => ErrorMessage = null;
                }

                errorMsgResetTimer.Stop();
                errorMsgResetTimer.Start();

                Set(ref errorMessage, value);
            }
        }

        private string successMessage = string.Empty;
        public string SuccessMessage
        {
            get => successMessage;
            set
            {
                if (successMsgResetTimer is null)
                {
                    successMsgResetTimer = new Timer { Interval = SuccessMessageResetInterval, AutoReset = true };
                    successMsgResetTimer.Elapsed += (sender, e) => SuccessMessage = null;
                }

                successMsgResetTimer.Stop();
                successMsgResetTimer.Start();

                Set(ref successMessage, value);
            }
        }

        /// <summary>
        /// Sets the specified field to a new value.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="field">The field to update.</param>
        /// <param name="newValue">The new value to give to the field.</param>
        /// <param name="propertyName">Name of the property (use the nameof operator when possible).</param>
        /// <param name="onChanged">Optional <paramref name="onChanged"/> callback to invoke if the field was changed.</param>
        /// <param name="force">Force the update even if it wouldn't be needed.</param>
        /// <returns><c>true</c> if the property needed to be updated (and thus received a new value), <c>false</c> otherwise.</returns>
        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "", Action onChanged = null, bool force = false)
        {
            if (!force && EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            // Update field.
            field = newValue;

            // Raise events.
            ExecUI(() =>
            {
                onChanged?.Invoke();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });

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
