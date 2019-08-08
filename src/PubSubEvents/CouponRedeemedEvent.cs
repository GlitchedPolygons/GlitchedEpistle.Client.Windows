using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents
{
    /// <summary>
    /// This parameterless <see cref="PubSubEvent"/> is raised
    /// whenever the user has successfully redeemed a coupon code to extend his account.
    /// </summary>
    /// <seealso cref="Prism.Events.PubSubEvent" />
    public class CouponRedeemedEvent : PubSubEvent
    {
    }
}
