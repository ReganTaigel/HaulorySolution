using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Contracts.Jobs
{
    public sealed class TrailerLookupDto
    {
        public Guid Id { get; set; }
        public string Rego { get; set; } = string.Empty;
        public int? OdometerKm { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
