using System.Threading;
using LiquidCrystal;
namespace HelloWorld
{
    public class Program
    {
        public static void Main()
        {
            const int rs = 4, en = 2, d4 = 32, d5 = 33, d6 = 25, d7 = 26;
            var lcd = new LiquidCrystal.LiquidCrystal(rs, en, d4, d5, d6, d7, lines: 1);
            // set up the LCD's number of columns and rows:
            // Print a message to the LCD.
            lcd.Write("hello world!");
            Thread.Sleep(Timeout.Infinite);

        }
    }
}
