using System;
using System.Net.Http;

using Newtonsoft.Json;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;

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

        public CouponRedeemedSuccessfullyViewModel(User user, IUserService userService)
        {
            ThankYouNote =
$@"You are awesome!

By redeeming your access code you have successfully extended your Glitched Epistle membership by 30 days, which means it will now expire {userService.GetUserExpirationUTC(user.Id).Result:F} (UTC).

To celebrate your decision of valuing privacy, here's a completely random quote:";

            GetQuoteOfTheDay();
        }

        private async void GetQuoteOfTheDay()
        {
            dynamic qod;
            using (var httpClient = new HttpClient())
            {
                dynamic qodJson = JsonConvert.DeserializeObject(await httpClient.GetStringAsync(new Uri("http://quotes.rest/qod.json")));
                qod = qodJson.contents.quotes[0];
            }
            Quote = $"{qod.quote}\n\n- {qod.author}";
        }
    }
}
