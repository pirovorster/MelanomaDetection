//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Research.Model
{
    using System;
    using System.Collections.Generic;

	[Serializable]
    public partial class Feature
    {
        public System.Guid Id { get; set; }
        public string FeatureName { get; set; }
        public double Value { get; set; }
        public string Path { get; set; }
    }
}
