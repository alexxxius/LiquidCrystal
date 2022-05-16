using LCD1602;

namespace TestLiquidCrystal
{
    public class Command
    {
        private readonly uint[] _rowOffsets = new uint[4];
        private readonly uint _numLines;
        private readonly DisplayOnOffControl _displayOnOffControl;
        private readonly DisplayFunction _displayFunction;
        private readonly DisplayEntryMode _displayEntryMode;
        private readonly DisplayCursorShift _displayCursorShift;

        public Command(DisplayOnOffControl displayOnOffControl,
            DisplayFunction displayFunction,
            DisplayEntryMode displayEntryMode,
            DisplayCursorShift displayCursorShift,
            uint numLines)
        {
            _displayOnOffControl = displayOnOffControl;
            _displayFunction = displayFunction;
            _displayEntryMode = displayEntryMode;
            _displayCursorShift = displayCursorShift;
            _numLines = numLines;
            SetRowOffsets(0x00, 0x40);
        }

        public uint ClearDisplay => 0x01;
        public uint NoDisplay => _displayOnOffControl.NoDisplay;
        public uint Display => _displayOnOffControl.Display;
        public uint NoBlink => _displayOnOffControl.NoBlink;
        public uint Blink => _displayOnOffControl.Blink;

        public uint FunctionSet => _displayFunction.FunctionSet;
        public uint EntryModeSet => _displayEntryMode.EntryModeSet;
        public uint ReturnHome => 0x02;
        public uint SetCGramAddr => 0x40;
        public uint ScrollDisplayLeft => _displayCursorShift.ScrollDisplayLeft;
        public uint ScrollDisplayRight => _displayCursorShift.ScrollDisplayRight;
        public uint RightToLeft => _displayEntryMode.RightToLeft;
        public uint LeftToRight => _displayEntryMode.LeftToRight;
        public uint AutoScroll => _displayEntryMode.AutoScroll;
        public uint NoAutoScroll => _displayEntryMode.NoAutoScroll;
        public uint NoCursor => _displayOnOffControl.NoCursor;
        public uint Cursor => _displayOnOffControl.Cursor;

        public uint SetDDramAddr(uint col, uint row)
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
            return 0x80 | (col + _rowOffsets[row]);
        }
        private void SetRowOffsets(uint row0, uint row1)
        {
            const uint cols = 16;
            _rowOffsets[0] = row0;
            _rowOffsets[1] = row1;
            _rowOffsets[2] = row0 + cols;
            _rowOffsets[3] = row1 + cols;
        }
        public uint CreateChar(uint location)
        {
            location &= 0x7; // we only have 8 locations 0-7
            return SetCGramAddr | (location << 3);
        }
    }
}