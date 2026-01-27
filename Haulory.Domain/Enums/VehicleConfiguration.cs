using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Domain.Enums
{

    public enum VehicleConfiguration
    {
        SingleAxle,
        TandemAxle,
        Rigid,
        RigidCold,
        TractorUint,
        SemiFlatDeck,
        SemiSkeleton,
        SemiRefrigerator,
        RefrigeratorTrailer,
        CurtainSiderTrailer,
        BTrainCurtainSider
    }

    public enum Class4PowerUnitType
    { 
        Truck = 0,
        Tractor = 1
    }


    public enum ComplianceCertificateType
    {
        None = 0,
        Wof = 1,
        Cof = 2
    }
}
