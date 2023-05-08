using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Poseidon;

public class AppDictionary
{
    private static AppDictionary _appDictionary = null;
    private App app;

    private AppDictionary()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var fileName = $"app.{environmentName}.yaml";
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        using var reader = new StreamReader(fileName);
        app = deserializer.Deserialize<App>(reader);
    }

    public static AppDictionary GetAppDictionary()
    {
        if (_appDictionary == null)
        {
            _appDictionary = new AppDictionary();
        }

        return _appDictionary;
    }
    
    public App GetApp()
    {
        return app;
    }
}


