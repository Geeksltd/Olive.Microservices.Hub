﻿namespace Olive.Microservices.Hub
{
    using Olive.Entities;

    /// <summary>Represents an instance of Authrozied feature info entity type.</summary>
    [TransientEntity]

    public partial class AuthroziedFeatureInfo : GuidEntity
    {
        /// <summary>Gets or sets a value indicating whether this Authrozied feature info instance Is disabled.</summary>
        public bool IsDisabled { get; set; }

        /// <summary>Gets or sets the value of Feature on this Authrozied feature info instance.</summary>
        public Feature Feature { get; set; }

        /// <summary>Returns a textual representation of this Authrozied feature info.</summary>
        public override string ToString() => $"Authrozied feature info ({ID})";
    }
}