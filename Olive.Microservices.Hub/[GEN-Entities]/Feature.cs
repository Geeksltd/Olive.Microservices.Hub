namespace Domain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Olive;
    using Olive.Entities;
    using Olive.Entities.Data;
    
    /// <summary>Represents an instance of Feature entity type.</summary>
    [TransientEntity]
    [SkipAutoSort]
    [EscapeGCop("Auto generated code.")]
    public partial class Feature : GuidEntity, IComparable<Feature>, IHierarchy, ISortable
    {
        /// <summary>Stores the associated Features for Children property.</summary>
        private IList<Feature> children = new List<Feature>();
        
        /// <summary>Initializes a new instance of the Feature class.</summary>
        public Feature() => Saving += (ev) => ev.Do(Feature_Saving);
        
        /// <summary>Gets or sets the value of BadgeOptionalFor on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string BadgeOptionalFor { get; set; }
        
        /// <summary>Gets or sets the value of BadgeUrl on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string BadgeUrl { get; set; }
        
        /// <summary>Gets or sets the value of Description on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this Feature instance is hide.<para/>
        /// Hide this feature on the left menu and it&apos;s only visible on the top menu<para/>
        /// </summary>
        [System.ComponentModel.DisplayName("hide")]
        public bool Hide { get; set; }
        
        /// <summary>Gets or sets the value of Icon on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Icon { get; set; }
        
        /// <summary>Gets or sets the value of ImplementationUrl on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string ImplementationUrl { get; set; }
        
        /// <summary>Gets or sets the value of LoadUrl on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string LoadUrl { get; set; }
        
        /// <summary>Gets or sets the value of NotPermissions on this Feature instance.</summary>
        public string[] NotPermissions { get; set; }
        
        /// <summary>Gets or sets the value of Order on this Feature instance.</summary>
        public int Order { get; set; }
        
        /// <summary>Gets or sets the value of Pass on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Pass { get; set; }
        
        /// <summary>Gets or sets the value of Permissions on this Feature instance.</summary>
        public string[] Permissions { get; set; }
        
        /// <summary>Gets or sets the value of Ref on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Ref { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this Feature instance Show on right.<para/>
        /// Show this feature on the right menu when navigating to the board pages<para/>
        /// </summary>
        public bool ShowOnRight { get; set; }
        
        /// <summary>Gets or sets the value of Title on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Title { get; set; }
        
        /// <summary>Gets or sets a value indicating whether this Feature instance Use iframe.</summary>
        public bool UseIframe { get; set; }
        
        /// <summary>Gets or sets the Children of this Feature.</summary>
        public IEnumerable<Feature> Children
        {
            get => children;
            
            set
            {
                children = value?.ToList() ?? new List<Feature>();
            }
        }
        
        /// <summary>Gets the value of GrandParent on this Feature instance.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.ComponentModel.DisplayName("GrandParent")]
        public Feature GrandParent
        {
            get => Parent?.Parent;
        }
        
        /// <summary>Gets or sets the value of Parent on this Feature instance.</summary>
        public Feature Parent { get; set; }
        
        /// <summary>Gets or sets the value of Service on this Feature instance.</summary>
        public Service Service { get; set; }
        
        /// <summary>Returns a textual representation of this Feature.</summary>
        public override string ToString() => this.WithAllParents().Select(s=>s.Title).ToString(" > ");
        
        /// <summary>Compares this Feature with another specified Feature instance.</summary>
        /// <param name="other">The other Feature to compare this instance to.</param>
        /// <returns>
        /// An integer value indicating whether this instance precedes, follows, or appears in the same position as the other Feature in sort orders.<para/>
        /// </returns>
        public int CompareTo(Feature other)
        {
            if (other is null)
                return 1;
            else
            {
                return this.Order.CompareTo(other.Order);
            }
        }
        
        /// <summary>Compares this Feature with another object of a compatible type.</summary>
        public override int CompareTo(object other)
        {
            if (other is Feature) return CompareTo(other as Feature);
            else return base.CompareTo(other);
        }
        
        /// <summary>Adds a single Feature  to the Children of this Feature.</summary>
        /// <param name="item">The instance to add to Children of this Feature.</param>
        public virtual void AddToChildren(Feature item)
        {
            if (item != null && !children.Contains(item))
            {
                children.Add(item);
            }
        }
        
        /// <summary>Removes a specified Feature object from the Children of this Feature.</summary>
        /// <param name="item">The instance to remove from Children of this Feature.</param>
        public virtual void RemoveFromChildren(Feature item)
        {
            if (item != null && children.Contains(item))
            {
                children.Remove(item);
            }
        }
        
        /// <summary>
        /// Validates the data for the properties of this Feature and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override Task ValidateProperties()
        {
            var result = new List<string>();
            
            if (BadgeOptionalFor?.Length > 200)
                result.Add("The provided Badge optional for is too long. A maximum of 200 characters is acceptable.");
            
            if (BadgeUrl?.Length > 200)
                result.Add("The provided Badge url is too long. A maximum of 200 characters is acceptable.");
            
            if (Description?.Length > 200)
                result.Add("The provided Description is too long. A maximum of 200 characters is acceptable.");
            
            if (Icon?.Length > 200)
                result.Add("The provided Icon is too long. A maximum of 200 characters is acceptable.");
            
            if (ImplementationUrl?.Length > 200)
                result.Add("The provided Implementation url is too long. A maximum of 200 characters is acceptable.");
            
            if (LoadUrl.IsEmpty())
                result.Add("Load url cannot be empty.");
            
            if (LoadUrl?.Length > 200)
                result.Add("The provided Load url is too long. A maximum of 200 characters is acceptable.");
            
            if (Order < 0)
                result.Add("The value of Order must be 0 or more.");
            
            if (Pass?.Length > 200)
                result.Add("The provided Pass is too long. A maximum of 200 characters is acceptable.");
            
            if (Ref?.Length > 200)
                result.Add("The provided Ref is too long. A maximum of 200 characters is acceptable.");
            
            if (Service == null)
                result.Add("Please provide a value for Service.");
            
            if (Title.IsEmpty())
                result.Add("Title cannot be empty.");
            
            if (Title?.Length > 200)
                result.Add("The provided Title is too long. A maximum of 200 characters is acceptable.");
            
            if (this.GetParent() != null)
            {
                if (this.WithAllChildren().Contains(this.GetParent()))
                    result.Add(string.Format("Invalid parent selected for this Feature. Setting {0} as the parent node of {1} will create an infinite loop.", this.GetParent(), this));
            }
            
            if (result.Any())
                throw new ValidationException(result.ToLinesString());
            
            return Task.CompletedTask;
        }
        
        /// <summary>Handles the Saving event of the Feature instance.</summary>
        /// <param name="e">The CancelEventArgs instance containing the event data.</param>
        async Task Feature_Saving(System.ComponentModel.CancelEventArgs e)
        {
            if (IsNew && Order == 0)
            {
                // This is a new Feature with unset Order value.
                // So set the Order property so that this Feature goes to the end of the list:
                Order = await Sorter.GetNewOrder(this);
            }
        }
    }
}