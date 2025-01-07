using Flurl.Http;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace InitialLogin;

class Program
{
    static void Main(string[] args)
    {
        var cookieFilePath = args.Length > 0 && IsValidPath(args[0]) ? args[0] : "cookies.txt";
        var cookies = GetCookies(cookieFilePath);
        foreach (var cookie in cookies)
        {
            Console.WriteLine($"{cookie.Name}: {cookie.Value}");
        }
        
        Console.WriteLine("Cookies saved to file: " + cookieFilePath);
    }

    private static bool IsValidPath(string path)
    {
        return Path.IsPathFullyQualified(path) && path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
    }
    
    private static CookieJar GetCookies(string file = "")
    {
        var options = new ChromeOptions();
        // options.AddArgument("--headless"); // Run Chrome in headless mode
        using var driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl("https://oma.datahub.fi/");
        var cookieJar = new CookieJar();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
        wait.Until(d => d.Url == "https://oma.datahub.fi/#/");

        foreach (var cookie in driver.Manage().Cookies.AllCookies)
        {
            cookieJar.AddOrReplace(new FlurlCookie(
                cookie.Name,
                cookie.Value,
                driver.Url,
                cookie.Expiry));
        }

        if (cookieJar.Count == 0)
        {
            throw new Exception("Could not get cookies");
        }

        File.WriteAllText(file, cookieJar.ToString());
        return cookieJar;
    }
}