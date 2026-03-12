using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Mobile.Contracts.Vehicles
{
    public sealed class OdometerAssetDto
    {
        public Guid Id { get; set; }
        public int UnitNumber { get; set; }
        public string Rego { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int? CurrentOdometerKm { get; set; }
    }
}
