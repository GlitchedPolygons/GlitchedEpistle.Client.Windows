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

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    /// <summary>
    /// Interface for requesting a <see cref="System.Windows.Window"/>'s closure through its <see cref="ViewModel"/>.
    /// </summary>
    public interface ICloseable
    {
        /// <summary>
        /// Occurs when some <see cref="ViewModel"/> requested a view's closure
        /// (code-behind should react to this <c>event</c> and close itself when it's raised).
        /// </summary>
        event EventHandler<EventArgs> RequestedClose;
    }
}
