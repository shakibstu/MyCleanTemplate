namespace MyCleanTemplate.Domain.Common.Logs
{
    public enum LogLevel
    {
        RequestInApp = 1,
        RequestInApi,
        ResponseApp,
        ResponseApi,
        DbQuery,
        Repo,
        //TODO: Use These Levels
        Info,
        Warn,
        Err,
    }
}
