using System;
using System.Text.RegularExpressions;
using Bogus.DataSets;
using Fabrica.Utilities.Types;
using NUnit.Framework;

namespace Fabrica.Tests.Utilities;


[TestFixture]
public class RegExTests
{


    [Test]
    public void Test0110_RegEx_should_Work_On_Various_Urls()
    {

        var raw = "https://prod.fortiumpartners.com";


        var re = new Regex(@"^(?<scheme>[a-z][a-z0-9+.-]+):(?<authority>\/\/(?<user>[^@]+@)?(?<host>[a-z0-9.\-_~]+)(?<port>:\d+)?)?(?<path>(?:[a-z0-9-._~]|%[a-f0-9]|[!$&'()*+,;=:@])+(?:\/(?:[a-z0-9-._~]|%[a-f0-9]|[!$&'()*+,;=:@])*)*|(?:\/(?:[a-z0-9-._~]|%[a-f0-9]|[!$&'()*+,;=:@])+)*)?(?<query>\?(?:[a-z0-9-._~]|%[a-f0-9]|[!$&'()*+,;=:@]|[/?])+)?(?<fragment>\#(?:[a-z0-9-._~]|%[a-f0-9]|[!$&'()*+,;=:@]|[/?])+)?$");

        var z = re.GetGroupNames();




        var x = re.IsMatch(raw);
        if (x)
        {
            var uri = new Uri(raw);
            var host = uri.Host;
            var args = host.Split(".");
            var domain = host;
            if (args.Length == 3)
                domain = $"{args[1]}.{args[2]}";
        }


    }


}