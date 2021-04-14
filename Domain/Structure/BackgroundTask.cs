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

    /// <summary>Represents an instance of Background task entity type.</summary>
    
    public partial class BackgroundTask : GuidEntity, Olive.PassiveBackgroundTasks.IBackgourndTask
    {
        /// <summary>Gets or sets the value of ExecutingInstance on this Background task instance.</summary>
        public Guid? ExecutingInstance { get; set; }

        /// <summary>Gets or sets the value of Heartbeat on this Background task instance.</summary>
        public DateTime? Heartbeat { get; set; }

        /// <summary>Gets or sets the value of IntervalInMinutes on this Background task instance.</summary>
        public int IntervalInMinutes { get; set; }

        /// <summary>Gets or sets the value of LastExecuted on this Background task instance.</summary>
        public DateTime? LastExecuted { get; set; }

        /// <summary>Gets or sets the value of Name on this Background task instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Name { get; set; }

        /// <summary>Gets or sets the value of TimeoutInMinutes on this Background task instance.</summary>
        public int TimeoutInMinutes { get; set; }

        /// <summary>
        /// Find and returns an instance of Background task from the database by its Name.<para/>
        ///                               If no matching Background task is found, it returns Null.<para/>
        /// </summary>
        /// <param name="name">The Name of the requested Background task.</param>
        /// <returns>
        /// The Background task instance with the specified Name or null if there is no Background task with that Name in the database.<para/>
        /// </returns>
        public static Task<BackgroundTask> FindByName(string name)
        {
            return Database.FirstOrDefault<BackgroundTask>(b => b.Name == name);
        }

        /// <summary>Returns a textual representation of this Background task.</summary>
        public override string ToString() => Name;

        /// <summary>Returns a clone of this Background task.</summary>
        /// <returns>
        /// A new Background task object with the same ID of this instance and identical property values.<para/>
        ///  The difference is that this instance will be unlocked, and thus can be used for updating in database.<para/>
        /// </returns>
        public new BackgroundTask Clone() => (BackgroundTask)base.Clone();

        /// <summary>
        /// Validates the data for the properties of this Background task and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override async Task ValidateProperties()
        {
            var result = new List<string>();

            if (IntervalInMinutes < 0)
                result.Add("The value of Interval in minutes must be 0 or more.");

            if (Name.IsEmpty())
                result.Add("Name cannot be empty.");

            if (Name?.Length > 200)
                result.Add("The provided Name is too long. A maximum of 200 characters is acceptable.");

            // Ensure uniqueness of Name.

            if (await Database.Any<BackgroundTask>(b => b.Name == Name && b != this))
                result.Add("Name must be unique. There is an existing Background task record with the provided Name.");

            if (TimeoutInMinutes < 0)
                result.Add("The value of Timeout in minutes must be 0 or more.");

            if (result.Any())
                throw new ValidationException(result.ToLinesString());
        }
    }
}