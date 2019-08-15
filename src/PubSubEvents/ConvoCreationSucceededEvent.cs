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
using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Models;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="Prism.Events.PubSubEvent{String}"/> (where <c>TPayload</c> is of type <see cref="String"/>)
    /// is raised whenever the user has successfully created a new <see cref="Convo"/>.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent{String}" />
    public class ConvoCreationSucceededEvent : PubSubEvent<string>
    {
    }
}
