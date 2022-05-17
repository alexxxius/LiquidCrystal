using System;
using System.Threading;
using LiquidCrystal;


namespace CustomCharacter
{
    public class Program
    {
        public static void Main()
        {
            const int rs = 4, en = 2, d4 = 32, d5 = 33, d6 = 25, d7 = 26;
            var lcd = new LiquidCrystal.LiquidCrystal(rs, en, d4, d5, d6, d7);
            
            // make some custom characters:
            var heart = new uint[]{
                0b00000,
                0b01010,
                0b11111,
                0b11111,
                0b11111,
                0b01110,
                0b00100,
                0b00000
            };

            var smiley = new uint[]{
                0b00000,
                0b00000,
                0b01010,
                0b00000,
                0b00000,
                0b10001,
                0b01110,
                0b00000
            };

            var frownie = new uint[]{
                0b00000,
                0b00000,
                0b01010,
                0b00000,
                0b00000,
                0b00000,
                0b01110,
                0b10001
            };

            var armsDown = new uint[]{
                0b00100,
                0b01010,
                0b00100,
                0b00100,
                0b01110,
                0b10101,
                0b00100,
                0b01010
            };

            var armsUp = new uint[]{
                0b00100,
                0b01010,
                0b00100,
                0b10101,
                0b01110,
                0b00100,
                0b00100,
                0b01010
            };

            lcd.CreateChar(0, heart);
            // create a new character
            lcd.CreateChar(1, smiley);
            // create a new character
            lcd.CreateChar(2, frownie);
            // create a new character
            lcd.CreateChar(3, armsDown);
            // create a new character
            lcd.CreateChar(4, armsUp);

            // set the cursor to the top left
            lcd.SetCursor(0, 0);

            // Print a message to the LCD.
            lcd.Write("I ");
            lcd.Write((byte)0); // when calling lcd.write() '0' must be cast as a byte
            lcd.Write(" nanoFrmWrk! ");
            lcd.Write((byte)1);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
