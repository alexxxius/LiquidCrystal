using System.Threading;
using LiquidCrystal;

namespace Scroll
{
    public class Program
    {
        public static void Main()
        {
            const int rs = 4, en = 2, d4 = 32, d5 = 33, d6 = 25, d7 = 26;
            var lcd = new LiquidCrystal.LiquidCrystal(rs, en, d4, d5, d6, d7, lines: 2, cols: 16);
            lcd.Write("Hello nanoFwk!");
            Thread.Sleep(3000);
            while (true)
            {
                for (int positionCounter = 0; positionCounter < 13; positionCounter++)
                {
                    // scroll one position left:
                    lcd.ScrollDisplayLeft();
                    // wait a bit:
                    Thread.Sleep(250);
                }

                // scroll 29 positions (string length + display length) to the right
                // to move it offscreen right:
                for (int positionCounter = 0; positionCounter < 29; positionCounter++)
                {
                    // scroll one position right:
                    lcd.ScrollDisplayRight();
                    // wait a bit:
                    Thread.Sleep(250);
                }

                // scroll 16 positions (display length + string length) to the left
                // to move it back to center:
                for (int positionCounter = 0; positionCounter < 16; positionCounter++)
                {
                    // scroll one position left:
                    lcd.ScrollDisplayLeft();
                    // wait a bit:
                    Thread.Sleep(250);
                }

                // Thread.Sleep at the end of the full loop:
                Thread.Sleep(1000);
            }
        }
    }
}
