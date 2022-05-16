using System.Threading;
using LCD1602;

namespace TestLiquidCrystal
{
    public class Program
    {
        public static void Main()
        {

            Thread.Sleep(1000);
            const int rs = 4, en = 2, d4 = 32, d5 = 33, d6 = 25, d7 = 26;
            var lcd = new LiquidCrystal(rs, en, d4, d5, d6, d7, 1);
            // set up the LCD's number of columns and rows:
            // Print a message to the LCD.
            lcd.Write("hello world!");
            while (true)
            {

                lcd.Blink();
                Thread.Sleep(3000);
                lcd.NoBlink();
                Thread.Sleep(3000);
            }
            
        }



    }
}
