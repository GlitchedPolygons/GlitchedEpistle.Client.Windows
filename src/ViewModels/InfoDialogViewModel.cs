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
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class InfoDialogViewModel : ViewModel, ICloseable
    {
        #region Constants
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region UI Bindings
        private string title = "Information";
        public string Title { get => title; set => Set(ref title, value); }

        private string text = string.Empty;
        public string Text { get => text; set => Set(ref text, value); }

        private string okButtonText = "Okay";
        public string OkButtonText { get => okButtonText; set => Set(ref okButtonText, value); }

        private double maxWidth = 420;
        public double MaxWidth { get => maxWidth; set => Set(ref maxWidth, value); }
        #endregion

        #region Commands
        public ICommand OkButtonCommand { get; }
        #endregion

        public InfoDialogViewModel()
        {
            OkButtonCommand = new DelegateCommand(OnClickedOkay);
        }

        private void OnClickedOkay(object commandParam)
        {
            RequestedClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
