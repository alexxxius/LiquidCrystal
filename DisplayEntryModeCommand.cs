namespace LiquidCrystal
{
    public class DisplayEntryModeCommand
    {
        private uint _value; // flags for display entry mode     
        private const uint EntryRight = 0x00;
        private const uint EntryLeft = 0x02;
        private const uint EntryShiftIncrement = 0x01;
        private const uint EntryShiftDecrement = 0x00;
        private const uint EntryMode = 0x04;
        private readonly uint[] _rowOffsets = new uint[4];
        private readonly uint _numLines;
        private const uint SetDDramAddr = 0x80;

        public DisplayEntryModeCommand(uint numLines)
        {
            _numLines = numLines;
            SetRowOffsets(0x00, 0x40);
       
            _value = EntryLeft | EntryShiftDecrement;
            // Initialize to default text direction (for romance languages)
        }

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

        public uint SetCursor(uint col, uint row)
        {
            var maxlines = (uint)_rowOffsets.Length;

            if (row >= maxlines)
            {
                row = maxlines - 1;    // we count rows starting w/ 0
            }
            if (row >= _numLines)
            {
                row = _numLines - 1;    // we count rows starting w/ 0
            }

            return SetDDramAddr | (col + _rowOffsets[row]);
        }
        private void SetRowOffsets(uint row0, uint row1)
        {
            const uint cols = 16;
            _rowOffsets[0] = row0;
            _rowOffsets[1] = row1;
            _rowOffsets[2] = row0 + cols;
            _rowOffsets[3] = row1 + cols;
        }
    }
}