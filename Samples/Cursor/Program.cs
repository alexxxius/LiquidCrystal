using LiquidCrystal;

namespace Cursor
{
    public class Program
    {
        public static void Main()
        {
            const int rs = 4, en = 2, d4 = 32, d5 = 33, d6 = 25, d7 = 26;
            var lcd = new LiquidCrystal.LiquidCrystal(rs, en, d4, d5, d6, d7);

            lcd.Write("Hello world!");
            while (true)
            {
                lcd.DisplayCursor();
            }
        }
    }
}
