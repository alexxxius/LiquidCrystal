using LCD1602;

namespace TestLiquidCrystal
{
    public class DisplayFunction
    {
        // flags for function set           ;
        private const uint Bit8Mode = 0x10;
        private const uint Bit4Mode = 0x00;
        private const uint TwoLine = 0x08;
        private const uint OneLine = 0x00;
        private const uint Dots5X10 = 0x04;
        private const uint Dots5X8 = 0x00;
        private readonly uint _value;

        public DisplayFunction(uint lines, uint dotSize, DataPinMode dataPinMode)
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

        public uint FunctionSet => 0x20 | _value;
    }
}