﻿/*
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

using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="PubSubEvent"/> is raised whenever a the user has successfully modified a <see cref="Convo"/>'s metadata server-side.<para> </para>
    /// The passed <c>string</c> parameter is the <see cref="Convo.Id"/>.<para> </para>
    /// Can also be raised when you're a non-admin participant of a (currently open and active)
    /// <see cref="Convo"/> and the admin changed the metadata server-side.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent" />
    public class ChangedConvoMetadataEvent : PubSubEvent<string>
    {
    }
}
