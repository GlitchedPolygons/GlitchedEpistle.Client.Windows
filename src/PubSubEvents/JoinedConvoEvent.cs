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

using GlitchedPolygons.GlitchedEpistle.Client.Models;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This <see cref="Prism.Events.PubSubEvent{TPayload}"/> (where <c>TPayload</c> is of type <see cref="Convo"/>)
    /// is raised whenever the user has submitted the convo credentials form successfully,
    /// and has now joined the conversation successfully.<para> </para>
    /// The subscriber should be adding the convo data to the device in a persistent way (e.g. file IO).
    /// </summary>
    public class JoinedConvoEvent : PubSubEvent<Convo>
    {
    }
}
