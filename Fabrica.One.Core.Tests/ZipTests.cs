
using System.IO;
using System.IO.Compression;
using NUnit.Framework;

namespace Fabrica.One.Core.Tests;

public class ZipTests
{

    public void Test1000_ZipDirectory()
    {


        var src = new DirectoryInfo( @"E:\fabrica-one\installations\fabrica-monitor-1\791312e6-0d07-4d44-8f6b-53d2672b86a7" );
        var dst = new FileInfo(@"c:\temp\tester.zip");

        if( dst.Exists)
            dst.Delete();

        ZipFile.CreateFromDirectory(src.FullName, dst.FullName, CompressionLevel.Fastest, false );
   

        Assert.IsTrue(true);


    }        


}