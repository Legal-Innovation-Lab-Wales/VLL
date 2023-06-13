﻿namespace VLL.Web;

public class AppConfiguration
{
    public string ConnectionString { get; }
    public string PostmarkServerToken { get; }
    public string TusFileStorePath { get; }
    public string OsrFileStorePath { get; }
    public string CookieKeyPath { get; }
    public string GmailPassword { get; }

    private AppConfiguration(string connectionString, string postmarkServerToken, string cookieKeyPath)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        PostmarkServerToken = postmarkServerToken ?? throw new ArgumentNullException(nameof(postmarkServerToken));
        CookieKeyPath = cookieKeyPath;
    }

    public static AppConfiguration LoadFromEnvironment()
    {
        // This reads the ASPNETCORE_ENVIRONMENT flag from the system

        // set on production server via the dot net run command
        // set on development via the launchSettings.json file
        // set on Unit test projects via the TestBase
        var aspnetcore = "ASPNETCORE_ENVIRONMENT";
        var env = Environment.GetEnvironmentVariable(aspnetcore);


        string connectionString;
        string postmarkServerToken = "";
        string cookieKeyPath;

        switch (env)
        {
            case "Development":
            case "Test":
                connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json")
                                                             .Build()
                                                             .GetConnectionString("Default");
                try
                {
                    postmarkServerToken = File.ReadAllText("../../secrets/postmark-osr4rightstools.txt");
                }
                catch
                {
                    // swallow for integration tests
                }
                //tusFileStorePath = @"c:\tusFileStore";
                //osrFileStorePath = @"c:\osrFileStore";

                var foo = @"c:\dev\vll\src\VLL.Web\cookie-keys";

                // Create a directory for keys if it doesn't exist
                if (!Directory.Exists(foo)) Directory.CreateDirectory(foo);
                cookieKeyPath = foo;
                break;

            case "Production":
                connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                   ?? throw new ApplicationException(
                                       "Can't read DB_CONNECTION_STRING environment variable. It should be set when building the webserver specifically when creating the kestrel service which is in /etc/systemd/system/kestrel-osr.service");

                postmarkServerToken = Environment.GetEnvironmentVariable("POSTMARK_OSR4RIGHTSTOOLS")
                                      ?? throw new ApplicationException(
                                          "Can't read POSTMARK_OSR4RIGHTSTOOLS environment variable. It should be set when building the webserver specifically when creating the kestrel service which is in /etc/systemd/system/kestrel-osr.service");


                cookieKeyPath = @"/mnt/vllshare/cookie-keys";

                break;

            default:
                throw new ArgumentException($"Expected {nameof(aspnetcore)} to be Development, Test or Production and it is {env}");
        }


        //// https://postmarkapp.com/support/article/1213-best-practices-for-testing-your-emails-through-postmark
        //// this is a black hole but tests if everything should work
        //if (env is "Development" or "Test") postmarkServerToken = "POSTMARK_API_TEST";


        // A way of getting secrets before I set environment variables on Dev and Prod

        //var filepath = Directory.GetCurrentDirectory();
        //if (1 == 2) { }
        //else if (filepath == "/var/www/web")
        //{
        //    Log.Information("Linux looking for apikey for postmark");
        //    //postmarkServerToken = File.ReadAllText(filepath + "/secrets/postmark-passwordpostgres.txt");
        //    //postmarkServerToken = File.ReadAllText(filepath + "/secrets/postmark-osr4rightstools-sandbox.txt");
        //    postmarkServerToken = File.ReadAllText(filepath + "/secrets/postmark-osr4rightstools.txt");
        //}
        //else
        //{
        //    Log.Information("Windows looking for apikey for postmark");
        //    //postmarkServerToken = File.ReadAllText("../../secrets/postmark-passwordpostgres.txt");
        //    postmarkServerToken = File.ReadAllText("../../secrets/postmark-osr4rightstools.txt");
        //}

        return new AppConfiguration(connectionString, postmarkServerToken, cookieKeyPath);

    }
}
