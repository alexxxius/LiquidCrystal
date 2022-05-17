using System.Threading;
using LiquidCrystal;

namespace AutoScroll
{
    public class Program
    {
        public static void Main()
        {
            const int rs = 4, en = 2, d4 = 32, d5 = 33, d6 = 25, d7 = 26;
            var lcd = new LiquidCrystal.LiquidCrystal(rs, en, d4, d5, d6, d7, lines: 2);

            var millisecondsTimeout = 500;
            while (true)
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



        }
    }
}
