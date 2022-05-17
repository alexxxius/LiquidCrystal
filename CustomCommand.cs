namespace LiquidCrystal
{
    public class CustomCommand
    {
        private const uint SetCGramAddr = 0x40;
        public uint CreateChar(uint location)
        {
            location &= 0x7; // we only have 8 locations 0-7
            return SetCGramAddr | (location << 3);
        }

    }
}