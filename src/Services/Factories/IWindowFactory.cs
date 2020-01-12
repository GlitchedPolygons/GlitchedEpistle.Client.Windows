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

using System.Windows;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;

using Unity;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories
{
    /// <summary>
    /// Interface for the <see cref="Window"/> factory.
    /// </summary>
    public interface IWindowFactory
    {
        /// <summary>
        /// Gets a <see cref="Window"/> with resolved dependencies through the <see cref="App"/>'s <see cref="UnityContainer"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Window"/> you want to get.</typeparam>
        /// <param name="ensureSingleWindow">If set to <see langword="true"/>, only one active <see cref="Window"/> of the provided type <c>T</c> can exist at one time.</param>
        /// <returns>The retrieved <see cref="Window"/> instance, ready to be shown (via <see cref="Window.Show"/>).</returns>
        T Create<T>(bool ensureSingleWindow) where T : Window;

        /// <summary>
        /// Gets or creates a window and opens/activates it immediately after.
        /// </summary>
        /// <typeparam name="TView">The type of the <see cref="Window"/> (view).</typeparam>
        /// <typeparam name="TViewModel">The type of the associated <see cref="ViewModel"/> DataContext.</typeparam>
        /// <param name="dialog">if set to <c>true</c> the <see cref="Window"/> will be opened as a dialog.</param>
        /// <param name="ensureSingleInstance">if set to <c>true</c> only one instance of the specified <see cref="Window"/> can ever exist at once.</param>
        TViewModel OpenWindow<TView, TViewModel>(bool dialog, bool ensureSingleInstance) where TView : Window where TViewModel : ViewModel;
    }
}
