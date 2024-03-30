﻿//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.Device.Dac;
using System.Threading;

namespace DacSample
{
    public class Scenario1ConfigureDac
    {
        static internal int dacResolution;

        /// <summary>
        /// Time resolution to update the DAC output in milliseconds.
        /// </summary>
        /// <remarks>
        ///  Default is 10ms.
        /// </remarks>
        internal const int s_timeResolution = 5;

        public static void Main()
        {
            string devices = DacController.GetDeviceSelector();
            Debug.WriteLine("DAC controllers: " + devices);

            // get default controller
            DacController dac = DacController.GetDefault();

            // open channel 0
            DacChannel dacChannel = dac.OpenChannel(0);

            // get DAC resolution
            dacResolution = dac.ResolutionInBits;

            // uncomment the scenario to test 
            /////////////////////////////////////////
            // !!note that none of these returns!! //
            /////////////////////////////////////////

            Scenario2TriangleWave.Execute(ref dacChannel);

            //Scenario3SquareWave.Execute(ref dacChannel);

            //Scenario4SineWave.Execute(ref dacChannel);


            Thread.Sleep(Timeout.Infinite);
        }
    }
}
