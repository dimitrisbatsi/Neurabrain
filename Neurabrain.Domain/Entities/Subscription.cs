using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Το Φροντιστήριο/Οργανισμός που έχει τη συνδρομή
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public string PlanName { get; set; } = string.Empty; // π.χ. "Free Trial", "Pro Educator"
        public string Status { get; set; } = "Active";       // π.χ. "Active", "Canceled", "PastDue"
        public string StripeSubscriptionId { get; set; } = string.Empty; // Για μελλοντική σύνδεση με το Stripe

        public DateTime CurrentPeriodEnd { get; set; }       // Πότε λήγει
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
