namespace LiquidCrystal
{
    public class DisplayCursorShiftCommand
    {
        // flags for display/cursor shift   ;
        private const uint DisplayMove = 0x08;
        private const uint CursorMove = 0x00;
        private const uint MoveRight = 0x04;
        private const uint MoveLeft = 0x00;
        private static uint CursorShift => 0x10;
        public uint ScrollDisplayLeft => CursorShift | DisplayMove | MoveLeft;
        public uint ScrollDisplayRight => CursorShift | DisplayMove | MoveRight;
    }
}