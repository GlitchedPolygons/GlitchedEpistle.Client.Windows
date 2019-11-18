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
using System.Threading;
using System.Globalization;

using Resources = GlitchedPolygons.GlitchedEpistle.Client.Windows.Properties.Resources;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization
{
    public class Localization : ILocalization
    {
        /// <summary>
        /// Translates the specified <c>string</c> identifier into the target <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="key">The localization lookup key (key <c>string</c> found in the <c>.resx</c> file).</param>
        /// <param name="ci">The <see cref="CultureInfo"/> to localize into (if left out <c>null</c>, <see cref="GetCurrentCultureInfo"/> is used).</param>
        /// <returns>Hopefully, the localized <c>string</c>.</returns>
        public string this[string key, CultureInfo ci = null]
        {
            get
            {
                var culture = ci ?? GetCurrentCultureInfo();

                try
                {
                    string translation = Resources.ResourceManager.GetString(key, ci);

                    if (translation == null)
                    {
                        translation = key;
#if DEBUG
                        Console.WriteLine(string.Format("Key '{0}' was not found in resources for culture '{1}'.", key, culture.Name));
#endif
                    }

                    return translation;
                }
                catch { return key; }
            }
        }

        /// <summary>
        /// Sets the <see cref="CultureInfo"/> for this app.
        /// </summary>
        /// <param name="ci">The target <see cref="CultureInfo"/> to apply.</param>
        public void SetCurrentCultureInfo(CultureInfo ci)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = ci;
            // Needs a System.Windows.Forms.Application.Restart(); in order to take effect.
        }

        /// <summary>
        /// Gets the current <see cref="CultureInfo"/>.
        /// </summary>
        public CultureInfo GetCurrentCultureInfo()
        {
            return Thread.CurrentThread.CurrentUICulture;
        }
    }
}
