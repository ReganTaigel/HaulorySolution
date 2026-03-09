using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Mobile.Models
{
    public class TrailerLookupResponse
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Rego { get; set; } = string.Empty;
        public int? OdometerKm { get; set; }
    }
}
