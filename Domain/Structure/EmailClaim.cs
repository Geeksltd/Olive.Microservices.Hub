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
    
    /// <summary>Represents an instance of Email claim entity type.</summary>
    
    public partial class EmailClaim : GuidEntity
    {
        /// <summary>Initializes a new instance of the EmailClaim class.</summary>
        public EmailClaim() => Created = LocalTime.Now;
        
        /// <summary>Gets or sets the value of Created on this Email claim instance.</summary>
        public DateTime Created { get; set; }
        
        /// <summary>Gets or sets the value of Email on this Email claim instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Email { get; set; }
        
        /// <summary>Gets or sets the value of Token on this Email claim instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Token { get; set; }
        
        /// <summary>
        /// Find and returns an instance of Email claim from the database by its Token.<para/>
        ///                               If no matching Email claim is found, it returns Null.<para/>
        /// </summary>
        /// <param name="token">The Token of the requested Email claim.</param>
        /// <returns>
        /// The Email claim instance with the specified Token or null if there is no Email claim with that Token in the database.<para/>
        /// </returns>
        public static Task<EmailClaim> FindByToken(string token)
        {
            return Database.FirstOrDefault<EmailClaim>(e => e.Token == token);
        }
        
        /// <summary>Returns a textual representation of this Email claim.</summary>
        public override string ToString() => Email;
        
        /// <summary>Returns a clone of this Email claim.</summary>
        /// <returns>
        /// A new Email claim object with the same ID of this instance and identical property values.<para/>
        ///  The difference is that this instance will be unlocked, and thus can be used for updating in database.<para/>
        /// </returns>
        public new EmailClaim Clone() => (EmailClaim)base.Clone();
        
        /// <summary>
        /// Validates the data for the properties of this Email claim and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override async Task ValidateProperties()
        {
            var result = new List<string>();
            
            if (Email.IsEmpty())
                result.Add("Email cannot be empty.");
            
            if (Email?.Length > 200)
                result.Add("The provided Email is too long. A maximum of 200 characters is acceptable.");
            
            if (Token.IsEmpty())
                result.Add("Token cannot be empty.");
            
            if (Token?.Length > 200)
                result.Add("The provided Token is too long. A maximum of 200 characters is acceptable.");
            
            // Ensure uniqueness of Token.
            
            if (await Database.Any<EmailClaim>(e => e.Token == Token && e != this))
                result.Add("Token must be unique. There is an existing Email claim record with the provided Token.");
            
            if (result.Any())
                throw new ValidationException(result.ToLinesString());
        }
    }
}