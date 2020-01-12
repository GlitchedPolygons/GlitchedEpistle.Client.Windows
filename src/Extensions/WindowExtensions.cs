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

using System;
using System.Windows;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions
{
    /// <summary>
    /// Extension methods for all <see cref="Window"/>s.
    /// </summary>
    public static class WindowExtensions
    {
        /// <summary>
        /// Makes the <see cref="Window"/> react to the <see cref="ICloseable.RequestedClose"/> <c>event</c> by subscribing <see cref="Window.Close"/> to it.
        /// Only call this ONCE and only FROM THE <see cref="Window"/>'S CONSTRUCTOR! 
        /// </summary>
        /// <param name="window">The <see cref="Window"/> that needs to be closeable through its <see cref="ViewModel"/>.</param>
        public static void MakeCloseable(this Window window)
        {
            if (window.DataContext is ICloseable dataContext)
            {
                void OnRequestedClosure(object sender, EventArgs args)
                {
                    window.Close();
                }

                dataContext.RequestedClose += OnRequestedClosure;
                window.Closing += (sender, args) => dataContext.RequestedClose -= OnRequestedClosure;
            }
        }
    }
}
