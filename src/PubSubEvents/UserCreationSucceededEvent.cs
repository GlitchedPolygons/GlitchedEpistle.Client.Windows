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

using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="Prism.Events.PubSubEvent{UserCreationResponse}"/>
    /// is raised whenever the user has submitted the registration form successfully, 
    /// and is now awaiting to receive his 2FA TOTP secret + backup codes to show on a QR and unordered list, respectively.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent{UserCreationResponse}" />
    public class UserCreationSucceededEvent : PubSubEvent<UserCreationResponseDto>
    {
    }
}
