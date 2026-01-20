using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Moblie.ViewModels
{
    public class VehicleOption <T>
    {

        public T Value { get; }
        public string Display { get; }

        public VehicleOption(T value, string display)
        {
            Value = value;
            Display = display;
        }

        public override string ToString() => Display;
    }
}
