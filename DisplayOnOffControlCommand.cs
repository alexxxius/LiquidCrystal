namespace LiquidCrystal
{
    public class DisplayOnOffControlCommand
    {
        private const uint DisplayOn = 0x04;
        private const uint DisplayOff = 0x00;
        private const uint CursorOn = 0x02;
        private const uint CursorOff = 0x00;
        private const uint BlinkOn = 0x01;
        private const uint BlinkOff = 0x00;
        private const uint DisplayControl = 0x08;
        private uint _value;

        public DisplayOnOffControlCommand()
        {
            // turn the display on with no cursor or blinking default
            _value = DisplayOn | CursorOff | BlinkOff;
        }

        public uint NoDisplay => DisplayControl | (_value &= ~DisplayOn);
        public uint Display => DisplayControl | (_value |= DisplayOn);
        public uint Blink => DisplayControl | (_value |= BlinkOn);
        public uint NoBlink => DisplayControl | (_value &= ~BlinkOn);

        public uint Cursor => DisplayControl | (_value |= CursorOn);
        public uint NoCursor => DisplayControl | (_value &= ~CursorOn);
    }
}