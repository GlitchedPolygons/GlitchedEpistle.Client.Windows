using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;

using Newtonsoft.Json;

using RestSharp;

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

            RestRequest request = new RestRequest(Method.GET);
            RestClient restClient = new RestClient("http://quotes.rest/qod.json");

            dynamic qodJson = JsonConvert.DeserializeObject(restClient.Get(request)?.Content);
            if (qodJson != null)
            {
                dynamic qod = qodJson.contents.quotes[0];
                Quote = $"{qod.quote}\n\n- {qod.author}";
            }
        }
    }
}
