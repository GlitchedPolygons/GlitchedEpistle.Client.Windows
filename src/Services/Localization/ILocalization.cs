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

using System.Globalization;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization
{
    /// <summary>
    /// Localization provider.
    /// </summary>
    public interface ILocalization
    {
        /// <summary>
        /// Gets the current device's preferred <see cref="CultureInfo"/>.
        /// </summary>
        CultureInfo GetCurrentCultureInfo();

        /// <summary>
        /// Sets the <see cref="CultureInfo"/> for this app.
        /// </summary>
        /// <param name="ci">The target <see cref="CultureInfo"/> to apply.</param>
        void SetCurrentCultureInfo(CultureInfo ci);

        /// <summary>
        /// Translates the specified <c>string</c> identifier into the target <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="key">The localization lookup key.</param>
        /// <param name="ci">The <see cref="CultureInfo"/> to localize into (if left out null, <see cref="GetCurrentCultureInfo"/> is used).</param>
        /// <returns>Hopefully, the localized <c>string</c>.</returns>
        string this[string key, CultureInfo ci = null] { get; }
    }
}
