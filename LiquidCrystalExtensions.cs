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

        public static void BlinkCursor(this LiquidCrystal lcd, int millisecondsTimeout = 500)
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

        public static void Write(this LiquidCrystal lcd, string message)
        {
            foreach (char c in message) lcd.Write(c);
        }
    }
}