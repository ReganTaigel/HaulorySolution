using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Domain.Enums
{

    public enum VehicleConfiguration
    {
        // Light Trailers axle
        SingleAxle,
        TandemAxle,

        // Rigid Truck
        RigidTruckCurtainsider,
        RigidTruckRefrigerator,
        RigidTruckTanker,
        RigidTruckFlatDeck,

        // Tractor Units 
        TractorUint,

        // Simi Trailers
        SemiCurtainsider,
        SemiFlatDeck,
        SemiSkeleton,
        SemiRefrigerator,
        SemiTanker,

        // Rigid Trailers
        RefrigeratorTrailer,
        CurtainSiderTrailer,
        TankerTrailer,
        FlatDeckTrailer,

        // B Train Trailers
        BCurtainSider,
        BFlatDeck,
        BRefigerator,
        BTanker
    }

    public enum Class4PowerUnitType
    {
        TruckCurtainsider,
        TruckRefrigerator,
        TruckTanker,
        TruckFlatDeck,
        Tractor
    }


    public enum ComplianceCertificateType
    {
        None = 0,
        Wof = 1,
        Cof = 2
    }
}
