using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;

namespace EjemploSleep
{
    public class Program
    {
        public static void Main()
        {
            // How to set Alternate pins for Devices ( COM1/2/3, SPI1/2, I2C, PWM ) then open device as normal
            Configuration.SetPinFunction(33, DeviceFunction.COM2_RX);
            Configuration.SetPinFunction(32, DeviceFunction.COM2_TX);

            // Sleep Test
            Debug.WriteLine("ESP32 Sleep test started");

            Debug.WriteLine("Check wakeup cause");
            Sleep.WakeupCause cause = Sleep.GetWakeupCause();
            Debug.WriteLine("Wakeup cause:" + cause.ToString());
            switch (cause)
            {
                // System was woken up by timer
                case Sleep.WakeupCause.Timer:
                    Debug.WriteLine("Wakup by timer");
                    break;

                // System was woken up in normal running mode
                case Sleep.WakeupCause.Undefined:

                    Debug.WriteLine("Set wakeup by timer for 10 seconds");
                    Sleep.EnableWakeupByTimer(new TimeSpan(0, 0, 0, 10));
                    Debug.WriteLine("Go to Deep sleep in 5 secs");
                    Debug.WriteLine("When coming out of deep sleep the system will reboot");
                    Thread.Sleep(5000);
                    Debug.WriteLine("Deep sleep now");
                    Sleep.StartDeepSleep();
                    break;

                default:
                    break;

            }

            try
            {
                // Other examples of Sleep in Hardware.Esp32

                //string message = "";

                //try
                //{
                //    // Wake up after a timer expires ( 60 seconds )
                //    Sleep.EnableWakeupByTimer(new TimeSpan(0, 0, 0, 60));
                //}
                //catch (Exception ex) { message = ex.Message; }

                //try
                //{
                //    // Wakeup when Pin12 is high
                //    Sleep.EnableWakeupByPin(Sleep.WakeupGpioPin.Pin12, 1);
                //}
                //catch (Exception ex) { message = ex.Message; }

                //try
                //{
                //    // Wakeup if either Pin12 or Pin13 are high
                //    Sleep.EnableWakeupByMultiPins(
                //    Sleep.WakeupGpioPin.Pin12 |
                //    Sleep.WakeupGpioPin.Pin13, 
                //    Sleep.WakeupMode.AnyHigh );
                //}
                //catch (Exception ex) { message = ex.Message; }
                //Debug.WriteLine(message);


                //try
                //{
                //    // Get the Wakeup cause
                //    cause = Sleep.GetWakeupCause();
                //}
                //catch (Exception ex) { message = ex.Message; }


                //try
                //{
                //    // Get the Wakeup Pin
                //    Sleep.WakeupGpioPin pin = Sleep.GetWakeupGpioPin();
                //}
                //catch (Exception ex) { message = ex.Message; }

                //try
                //{
                //    // Wake up when a touch pad is touched
                //    Sleep.EnableWakeupByTouchPad();
                //}
                //catch (Exception ex) { message = ex.Message; }

                //try
                //{
                //    // Get which touch pad woke up Esp32, 
                //    // when cause is Sleep.EnableWakeupByTouchPad
                //    Sleep.TouchPad tp = Sleep.GetWakeupTouchpad();
                //}
                //catch (Exception ex) { message = ex.Message; }


                //try
                //{
                //    // Start Light Sleep
                //    EspNativeError err = Sleep.StartLightSleep();
                //}
                //catch (Exception ex) { message = ex.Message; }


                //try
                //{
                //    // Start Deep Sleep
                //    EspNativeError err = Sleep.StartDeepSleep();
                //}
                //catch (Exception ex) { message = ex.Message; }
            }
            catch (Exception)
            {
                // Do whatever please you with the exception caught
            }
            finally    // Enter the infinite loop in all cases
            {
                while (true)
                {
                    Thread.Sleep(200);
                }
            }
        }

    }
}
