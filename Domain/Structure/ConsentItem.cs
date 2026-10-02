namespace Olive.Microservices.Hub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Olive;
    using Olive.Entities;

    /// <summary>
    /// One cookie consent choice made by a logged in person. Rows are never updated: a new choice
    /// adds a new row, so the table is the audit trail and the latest row is the person's answer.
    /// </summary>
    public partial class ConsentItem : GuidEntity
    {

        CachedReference<PeopleService.UserInfo> cachedPerson = new CachedReference<PeopleService.UserInfo>();

        public ConsentItem() => RegisteredAt = LocalTime.Now;

        /// <summary>The PeopleService id of the person who chose.</summary>
        public Guid PersonId { get; set; }

        /// <summary>ConsentService.PolicyVersion at the time of the choice. Bumping it re-prompts everyone.</summary>
        public int PolicyVersion { get; set; }

        public DateTime RegisteredAt { get; set; }

        public bool Necessary { get; set; }

        public bool Functional { get; set; }

        public bool Analytics { get; set; }

        // There is no Marketing flag: the hub has no advertising, so ad consent is never asked
        // and is always denied. See ConsentService.

        /// <summary>How consent was given: banner-accept | banner-reject | modal-accept | modal-reject | modal-save.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(20)]
        public string Source { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(45)]
        public string IP { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string UserAgent { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(2000)]
        public string PageUrl { get; set; }

        /// <summary>Gets or sets the value of Category on this FAQ instance.</summary>
        public PeopleService.UserInfo Person
        {
            get => cachedPerson.Get(PersonId);
            set => PersonId = value.ID;
        }

        public override string ToString() => $"{PersonId} v{PolicyVersion} {Source}";

        public new ConsentItem Clone() => (ConsentItem)base.Clone();

        protected override Task ValidateProperties()
        {
            var result = new List<string>();

            if (PersonId == Guid.Empty)
                result.Add("Person id cannot be empty.");

            if (Source.IsEmpty())
                result.Add("Source cannot be empty.");

            if (Source?.Length > 20)
                result.Add("The provided Source is too long. A maximum of 20 characters is acceptable.");

            if (IP?.Length > 45)
                result.Add("The provided IP is too long. A maximum of 45 characters is acceptable.");

            if (UserAgent?.Length > 500)
                result.Add("The provided User agent is too long. A maximum of 500 characters is acceptable.");

            if (PageUrl?.Length > 2000)
                result.Add("The provided Page url is too long. A maximum of 2000 characters is acceptable.");

            if (result.Any())
                throw new ValidationException(result.ToLinesString());

            return Task.CompletedTask;
        }
    }
}
