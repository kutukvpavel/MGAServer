using System;
using System.Collections.Generic;
using System.Text;
using RJCP.IO.Ports;

namespace MGAServer
{
    public class MGAServer
    {
        /*
         * Configuration == EEPROM image load??
         * Structure:
         * 
         * 0x4i - ith sensor page load start
         * 0x00 x 3
         * 0x01
         * 0x00 x 3
         * 0x01
         * 0x00
         * Three bytes, last nibbles represent target heater resistance with 2 decimal places, e.g. 0x0C 0x0F 0x00 = 33.12 Ohm
         * 0x00 x 3
         * 0x0A
         * 0x00 x 112
         * 0x8i - ith sensor page load end
         * .
         * .
         * .
         * 0xC0 - start measurement cycle?
         * 
         */

        public SerialPortStream Port { get; set; }
        public MGAParser Parser { get; set; }
    }
}
