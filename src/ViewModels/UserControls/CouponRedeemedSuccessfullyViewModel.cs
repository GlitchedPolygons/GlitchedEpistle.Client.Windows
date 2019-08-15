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

using RestSharp;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class CouponRedeemedSuccessfullyViewModel : ViewModel
    {
        #region UI Bindings
        private string thankYouNote;
        public string ThankYouNote { get => thankYouNote; set => Set(ref thankYouNote, value); }

        private string quote;
        public string Quote { get => quote; set => Set(ref quote, value); }
        #endregion

        public CouponRedeemedSuccessfullyViewModel()
        {
            ThankYouNote =

$@"You are awesome!

By redeeming your access code you have successfully extended your Glitched Epistle membership by 30 days.

To celebrate your decision of valuing privacy, here's a completely random quote:";

            Quote = "(retrieving quote from server)";

            Task.Run(() =>
            {
                RestRequest request = new RestRequest(Method.GET);
                RestClient restClient = new RestClient("http://quotes.rest/qod.json");

                dynamic qodJson = JsonConvert.DeserializeObject(restClient.Get(request)?.Content);
                if (qodJson != null)
                {
                    dynamic qod = qodJson.contents.quotes[0];
                    ExecUI(() => Quote = $"{qod.quote}\n\n- {qod.author}");
                }
            });
        }
    }
}
