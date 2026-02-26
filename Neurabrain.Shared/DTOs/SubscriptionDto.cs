using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Shared.DTOs
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }

        // Αυτό το πεδίο θα μας έρχεται από το Backend μόνο για να το βλέπουμε στον πίνακα
        public string OrganizationName { get; set; } = string.Empty;

        public string PlanName { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string StripeSubscriptionId { get; set; } = string.Empty;

        // Βάζουμε μια default τιμή για τον επόμενο μήνα
        public DateTime CurrentPeriodEnd { get; set; } = DateTime.UtcNow.AddMonths(1);
    }
}
