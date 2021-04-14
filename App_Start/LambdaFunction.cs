namespace Olive.Microservices.Hub
{
    public class LambdaFunction<TTaskManager> : FS.Shared.Website.LambdaFunction<HubStartup<TTaskManager>> where TTaskManager : BackgroundJobsPlan, new()
    {

    }
}
