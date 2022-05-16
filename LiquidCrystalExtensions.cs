using System.Threading;

namespace TestLiquidCrystal
{
    public static class LiquidCrystalExtensions
    {

        public static void DisplayCursor(this LiquidCrystal lcd, int millisecondsTimeout = 500)
        {
            lcd.NoCursor();
            Thread.Sleep(millisecondsTimeout);
            lcd.Cursor();
            Thread.Sleep(millisecondsTimeout);
        }

        public static void AutoScroll(this LiquidCrystal lcd, int millisecondsTimeout = 500)
        {
            lcd.SetCursor(0, 0);
            // print from 0 to 9:
            for (uint thisChar = 0; thisChar < 10; thisChar++)
            {
                lcd.Write(thisChar);
                Thread.Sleep(millisecondsTimeout);
            }

            // set the cursor to (16,1):
            lcd.SetCursor(16, 1);
            // set the display to automatically scroll:
            lcd.AutoScroll();
            // print from 0 to 9:
            for (uint thisChar = 0; thisChar < 10; thisChar++)
            {
                lcd.Write(thisChar);
                Thread.Sleep(millisecondsTimeout);
            }
            // turn off automatic scrolling
            lcd.NoAutoScroll();

            // clear screen for the next loop:
            lcd.Clear();
        }
        public static void Blink(this LiquidCrystal lcd, int millisecondsTimeout = 500)
        {
            lcd.NoBlink();
            Thread.Sleep(millisecondsTimeout);
            // Turn on the blinking cursor:
            lcd.Blink();
            Thread.Sleep(millisecondsTimeout);
        }
       
    }
}