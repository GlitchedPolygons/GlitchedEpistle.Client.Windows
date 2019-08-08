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
