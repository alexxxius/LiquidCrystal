using System.Threading;

namespace TextDirection
{
    public class Program
    {
        public static void Main()
        {
            const int rs = 4, en = 2, d4 = 32, d5 = 33, d6 = 25, d7 = 26;
            var lcd = new LiquidCrystal.LiquidCrystal(rs, en, d4, d5, d6, d7);
            lcd.Cursor();

            uint thisChar = 'a';
            while (true)
            {
                // reverse directions at 'm':
                if (thisChar == 'm')
                {
                    // go right for the next letter
                    lcd.RightToLeft();
                }
                // reverse again at 's':
                if (thisChar == 's')
                {
                    // go left for the next letter
                    lcd.LeftToRight();
                }
                // reset at 'z':
                if (thisChar > 'z')
                {
                    // go to (0,0):
                    lcd.Home();
                    // start again at 0
                    thisChar = 'a';
                }
                // print the character
                lcd.Write(thisChar);
                // wait a second:
                Thread.Sleep(1000);
                // increment the letter:
                thisChar++;
            }
        }
    }
}
