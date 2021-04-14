namespace Olive.Microservices.Hub
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
    
    /// <summary>Represents an instance of Service entity type.</summary>
    [TransientEntity]
    
    public partial class Service : GuidEntity
    {
        /// <summary>Initializes a new instance of the Service class.</summary>
        public Service()
        {
            Saving += (ev) => ClearCachedInstances();
            Saved += (ev) => ClearCachedInstances();
        }
        
        /// <summary>Gets or sets the value of BaseUrl on this Service instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string BaseUrl { get; set; }
        
        /// <summary>Gets or sets the value of Icon on this Service instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Icon { get; set; }
        
        /// <summary>Gets or sets a value indicating whether this Service instance Inject single signon.</summary>
        public bool InjectSingleSignon { get; set; }
        
        /// <summary>Gets or sets the value of Name on this Service instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Name { get; set; }
        
        /// <summary>Gets or sets a value indicating whether this Service instance Use Iframe.</summary>
        [System.ComponentModel.DisplayName("Use Iframe")]
        public bool UseIframe { get; set; }
        
        /// <summary>A repository of all Services.</summary>
        public static Dictionary<string, Service> Repository = new Dictionary<string, Service>();
        
        /// <summary>
        /// Returns the Service instance that is textually represented with a specified string value, or null if no such object is found.<para/>
        /// </summary>
        /// <param name="text">The text representing the Service to be retrieved from the database.</param>
        /// <returns>The Service object whose string representation matches the specified text.</returns>
        public static Task<Service> Parse(string text)
        {
            if (text.IsEmpty())
                throw new ArgumentNullException(nameof(text));
            
            return Task.FromResult(Repository[text]);
        }
        
        /// <summary>Returns a textual representation of this Service.</summary>
        public override string ToString() => Name.OrEmpty();
        
        /// <summary>
        /// Validates the data for the properties of this Service and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override Task ValidateProperties()
        {
            var result = new List<string>();
            
            if (BaseUrl?.Length > 200)
                result.Add("The provided Base url is too long. A maximum of 200 characters is acceptable.");
            
            if (Icon?.Length > 200)
                result.Add("The provided Icon is too long. A maximum of 200 characters is acceptable.");
            
            if (Name?.Length > 200)
                result.Add("The provided Name is too long. A maximum of 200 characters is acceptable.");
            
            if (result.Any())
                throw new ValidationException(result.ToLinesString());
            
            return Task.CompletedTask;
        }
        
        private static void ClearCachedInstances()
        {
        }
    }
}