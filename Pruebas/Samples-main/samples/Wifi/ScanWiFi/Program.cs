﻿//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Wifi;

namespace ScanWifi
{
    public class Program
    {
        // Set the SSID & Password to your local Wifi network
        const string MYSSID = "La Gorda";
        const string MYPASSWORD = "comandante123";

        public static void Main()
        {
            try
            {
                // Get the first WiFI Adapter
                WifiAdapter wifi = WifiAdapter.FindAllAdapters()[0];

                // Set up the AvailableNetworksChanged event to pick up when scan has completed
                wifi.AvailableNetworksChanged += Wifi_AvailableNetworksChanged;

                // give it some time to perform the initial "connect"
                // trying to scan while the device is still in the connect procedure will throw an exception
                Thread.Sleep(10_000);

                // Loop forever scanning every 30 seconds
                while (true)
                {
                    try
                    {
                        Debug.WriteLine("starting Wi-Fi scan");
                        wifi.ScanAsync();
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($"Failure starting a scan operation: {ex}");
                    }

                    Thread.Sleep(30000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("message:" + ex.Message);
                Debug.WriteLine("stack:" + ex.StackTrace);
            }

            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// Event handler for when Wifi scan completes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Wifi_AvailableNetworksChanged(WifiAdapter sender, object e)
        {
            Debug.WriteLine("Wifi_AvailableNetworksChanged - get report");

            // Get Report of all scanned Wifi networks
            WifiNetworkReport report = sender.NetworkReport;

            // Enumerate though networks looking for our network
            foreach (WifiAvailableNetwork net in report.AvailableNetworks)
            {
                // Show all networks found
                Debug.WriteLine( $"Net SSID :{net.Ssid},  BSSID : {net.Bsid},  rssi : {net.NetworkRssiInDecibelMilliwatts.ToString()},  signal : {net.SignalBars.ToString()}");

                // If its our Network then try to connect
                if (net.Ssid == MYSSID)
                {
                    // Disconnect in case we are already connected
                    sender.Disconnect();

                    // Connect to network
                    WifiConnectionResult result = sender.Connect(net, WifiReconnectionKind.Automatic, MYPASSWORD);

                    // Display status
                    if (result.ConnectionStatus == WifiConnectionStatus.Success)
                    {
                        Debug.WriteLine("Connected to Wifi network");
                    }
                    else
                    {
                        Debug.WriteLine($"Error {result.ConnectionStatus.ToString()} connecting o Wifi network");
                    }
                }
            }
        }
    }
}

