using LCD1602;

namespace LiquidCrystal
{
    public class DisplayFunctionCommand
    {
        // flags for function set           ;
        private const uint Bit8Mode = 0x10;
        private const uint Bit4Mode = 0x00;
        private const uint TwoLine = 0x08;
        private const uint OneLine = 0x00;
        private const uint Dots5X10 = 0x04;
        private const uint Dots5X8 = 0x00;
        private readonly uint _value;

        public DisplayFunctionCommand(uint lines, uint dotSize, DataPinMode dataPinMode)
        {
            if (dataPinMode == DataPinMode.Four)
                _value = Bit4Mode | OneLine | Dots5X8;
            else
                _value = Bit8Mode | OneLine | Dots5X8;

            if ((dotSize != Dots5X8) && (lines == 1))
                _value |= Dots5X10;

            if (lines > 1)
                _value |= TwoLine;
        }
        public uint ClearDisplay => 0x01;
        public uint ReturnHome => 0x02;
        public uint FunctionSet => 0x20 | _value;
    }
}