namespace TestLiquidCrystal
{
    public class DisplayEntryMode
    {
        private uint _value; // flags for display entry mode     
        private const uint EntryRight = 0x00;
        private const uint EntryLeft = 0x02;
        private const uint EntryShiftIncrement = 0x01;
        private const uint EntryShiftDecrement = 0x00;
        private const uint EntryMode = 0x04;

        public DisplayEntryMode() => _value = EntryLeft | EntryShiftDecrement;  // Initialize to default text direction (for romance languages)
        public uint EntryModeSet => EntryMode | _value;
        public uint LeftToRight
        {
            get
            {
                _value |= EntryLeft;
                return EntryModeSet;
            }
        }
        public uint RightToLeft
        {
            get
            {
                _value &= ~EntryRight;
                return EntryModeSet;
            }
        }
        public uint AutoScroll
        {
            get
            {
                _value |= EntryShiftIncrement;
                return EntryModeSet;
            }
        }
        public uint NoAutoScroll
        {
            get
            {
                _value &= ~EntryShiftIncrement;
                return EntryModeSet;
            }
        }
    }
}