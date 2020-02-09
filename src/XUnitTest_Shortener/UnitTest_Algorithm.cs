using dotnetcore.urlshortener.generator;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace XUnitTest_Shortener
{
    public class UnitTest_Algorithm
    {


        [Fact]
        public void Test1()
        {
            var guidUrlShortener = new GuidUrlShortenerAlgorithm();
            var keys = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                var key = guidUrlShortener.GenerateUniqueId();
                keys.Add(key);
            }
            var d = keys[0];

        }
        [Fact]
        public void Test2()
        {
            List<string> keys = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                var key = GuidUrlShortenerAlgorithm.ToBase62(i); 
                keys.Add(key);
            }
            var d = keys[0];

           
        }
    }
}
