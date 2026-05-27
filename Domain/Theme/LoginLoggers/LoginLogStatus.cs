namespace Olive.Microservices.Hub.Domain.Theme.LoginLoggers
{
    public enum LoginLogStatus
    {
        FailedExternalLogin,
        SuccessfulExternalLogin,

        RequestManualLogin,
        FailedManualLogin,
        SuccessfulManualLogin,
        WrongOtp,

        Logout,

        RequestAutoLogin,
        FailedAutoLogin,
        SuccessfulAutoLogin,

        RequestShellLogin,
        FailedShellLogin,
        SuccessfulShellLogin,
    }
}