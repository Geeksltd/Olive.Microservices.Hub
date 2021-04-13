namespace AppStart
{
    using System.Threading.Tasks;
    using Olive;
    using Olive.Microservices.Hub;


    /// <summary>Executes the scheduled tasks in independent threads automatically.</summary>
    [EscapeGCop("Auto generated code.")]
#pragma warning disable
    public partial class TaskManager : BackgroundJobsPlan
    {
        /// <summary>Registers the scheduled activities.</summary>
        public override void Initialize()
        {
            Register(new BackgroundJob("Pull all", () => PullAll(), Hangfire.Cron.MinuteInterval(1)));
        }

        /// <summary>Pull all</summary>
        public static async Task PullAll()
        {
            await new PeopleService.HubEndPoint(typeof(PeopleService.UserInfo).Assembly).PullAll();
        }
    }
}