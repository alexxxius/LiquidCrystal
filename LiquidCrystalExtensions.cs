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
    }
}