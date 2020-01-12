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
    /// Interface for the <see cref="ViewModel"/> factory.
    /// </summary>
    public interface IViewModelFactory
    {
        /// <summary>
        /// Gets a <see cref="ViewModel"/> with resolved dependencies through the <see cref="App"/>'s <see cref="UnityContainer"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ViewModel"/> you want to get.</typeparam>
        /// <returns>The retrieved <see cref="ViewModel"/> instance, ready to be assigned to a <see cref="Window.DataContext"/>.</returns>
        T Create<T>() where T : ViewModel;
    }
}
