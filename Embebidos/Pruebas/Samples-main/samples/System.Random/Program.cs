﻿//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.Threading;

namespace SystemRandom.Sample
{
    public class Program
    {
        public static void Main()
        {
            // instantiate random generator
            Random randomGenerator = new Random();

            for (; ; )
            {
                int counter = 0;

                // generate block of 10 random integers between 0 and 100
                Debug.WriteLine("");
                Debug.WriteLine("-- 10 random integers between 0 and 100 --");
                while (counter++ < 10)
                {
                    var value = randomGenerator.Next(100);
                    Debug.WriteLine(value.ToString());
                }

                counter = 0;

                // generate block of 10 random integers between 0 and 999999
                Debug.WriteLine("");
                Debug.WriteLine("-- 10 random integers between 0 and 999999 --");
                while (counter++ < 10)
                {
                    var value = randomGenerator.Next(999999);
                    Debug.WriteLine(value.ToString());
                }

                counter = 0;

                // generate block of 10 random doubles
                Debug.WriteLine("");
                Debug.WriteLine("-- 10 random doubles --");
                while (counter++ < 10)
                {
                    var value = randomGenerator.NextDouble();
                    Debug.WriteLine(value.ToString());
                }

                // fill byte array with 10 random numbers and output
                Debug.WriteLine("");
                Debug.WriteLine("-- 10 random numbers from array --");

                byte[] buffer = new byte[10];
                randomGenerator.NextBytes(buffer);

                counter = 0;
                while (counter < 10)
                {
                    var value = randomGenerator.NextDouble();
                    Debug.WriteLine(buffer[counter++].ToString());
                }

                Thread.Sleep(500);
            }
        }
    }
}
