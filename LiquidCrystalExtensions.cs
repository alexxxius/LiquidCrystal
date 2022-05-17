using System.Threading;

namespace LiquidCrystal
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

        public static void AutoScrollText(this LiquidCrystal lcd, int millisecondsTimeout = 500)
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
        public static void BlinkDisplay(this LiquidCrystal lcd, int millisecondsTimeout = 500)
        {
            lcd.NoBlink();
            Thread.Sleep(millisecondsTimeout);
            // Turn on the blinking cursor:
            lcd.Blink();
            Thread.Sleep(millisecondsTimeout);
        }

        public static void IntermittentDisplay(this LiquidCrystal lcd, int millisecondsTimeout = 500)
        {
            lcd.NoDisplay();
            Thread.Sleep(millisecondsTimeout);
            lcd.Display();
            Thread.Sleep(millisecondsTimeout);
        }
        
        public static void Write(this LiquidCrystal lcd,  string message)
        {
            foreach (var c in message) lcd.Write(c);
        }
    }
}