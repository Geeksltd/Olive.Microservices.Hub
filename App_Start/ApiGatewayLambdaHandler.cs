namespace Olive.Microservices.Hub
{
    public class ApiGatewayLambdaHandler<TTaskManager> : FS.Shared.Website.ApiGatewayLambdaHandler<HubStartup<TTaskManager>> where TTaskManager : BackgroundJobsPlan, new()
    {
    }
}
