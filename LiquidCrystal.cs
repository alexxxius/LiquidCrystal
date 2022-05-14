using System.Device.Gpio;
using System.Threading;
using LCD1602;
using static System.Device.DelayHelper;

namespace LCD1602
{
    public enum DataPinMode
    {
        Eight = 8,
        Four = 4
    }
    public class DisplayOnOffControl
    {
        private const uint DisplayOn = 0x04;
        private const uint DisplayOff = 0x00;
        private const uint CursorOn = 0x02;
        private const uint CursorOff = 0x00;
        private const uint BlinkOn = 0x01;
        private const uint BlinkOff = 0x00;

        private uint _value;

        public DisplayOnOffControl()
        {
            // turn the display on with no cursor or blinking default
            _value = DisplayOn | CursorOff | BlinkOff;
        }

        public uint NoDisplay => _value &= ~DisplayOn;
        public uint Display => _value |= DisplayOn;
        public uint Blink => _value |= BlinkOn;
        public uint NoBlink => _value &= ~BlinkOn;
    }

    public class DisplayEntryMode
    {
        private readonly uint _value; // flags for display entry mode     
        private const uint EntryRight = 0x00;
        private const uint EntryLeft = 0x02;
        private const uint EntryShiftIncrement = 0x01;
        private const uint EntryShiftDecrement = 0x00;

        public DisplayEntryMode() => _value = EntryLeft | EntryShiftDecrement;  // Initialize to default text direction (for romance languages)

        public static implicit operator uint(DisplayEntryMode d) => d._value;
    }

    public class DisplayCursorShift
    {
        // flags for display/cursor shift   ;
        private const uint DisplayMove = 0x08;
        private const uint CursorMove = 0x00;
        private const uint MoveRight = 0x04;
        private const uint MoveLeft = 0x00;
    }
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

        public static implicit operator uint(DisplayFunction d) => d._value;
    }
}
public class Command
{
    private readonly DisplayOnOffControl _displayOnOffControl;
    private readonly DisplayFunction _displayFunction;
    private readonly DisplayEntryMode _displayEntryMode;
    private readonly DisplayCursorShift _displayCursorShift;

    private const uint ReturnHome = 0x02;
    private const uint EntryModeSet = 0x04;
    private const uint DisplayControl = 0x08;
    private const uint CursorShift = 0x10;
    private const uint LCD_FUNCTIONSET = 0x20;
    private const uint SetCGramAddr = 0x40;
    private const uint SetDDramAddr = 0x80;

    public Command(DisplayOnOffControl displayOnOffControl,
        DisplayFunction displayFunction,
        DisplayEntryMode displayEntryMode,
        DisplayCursorShift displayCursorShift)
    {
        _displayOnOffControl = displayOnOffControl;
        _displayFunction = displayFunction;
        _displayEntryMode = displayEntryMode;
        _displayCursorShift = displayCursorShift;
    }

    public uint ClearDisplay => 0x01;

    public uint NoDisplay => DisplayControl | _displayOnOffControl.NoDisplay;
    public uint Display => DisplayControl | _displayOnOffControl.Display;
    public uint NoBlink => DisplayControl | _displayOnOffControl.NoBlink;
    public uint Blink => DisplayControl | _displayOnOffControl.Blink;

    public uint FunctionSet => LCD_FUNCTIONSET | _displayFunction;
    public uint EntryMode => EntryModeSet | _displayEntryMode;

}

public class LiquidCrystal
{
    private readonly int _rsPin; // LOW: command. HIGH: character.
    private readonly int _rwPin; // LOW: write to LCD. HIGH: read from LCD.
    private readonly int _enablePin; // activated by a HIGH pulse.
    private readonly int[] _dataPins = new int[8];

    private readonly uint[] _rowOffsets = new uint[4];
    private readonly GpioController _gpio = new();
    private readonly Command _command;
    private readonly DataPinMode _dataPinMode;

    public LiquidCrystal(int rs, int enable,
             int d0, int d1, int d2, int d3,
             uint lines = 1, uint dotSize = 0x00)
    {
        _rsPin = rs;
        _rwPin = 255;
        _enablePin = enable;
        _dataPins[0] = d0;
        _dataPins[1] = d1;
        _dataPins[2] = d2;
        _dataPins[3] = d3;
        _dataPins[4] = 0;
        _dataPins[5] = 0;
        _dataPins[6] = 0;
        _dataPins[7] = 0;
        _dataPinMode = DataPinMode.Four;
        _command = new Command(new DisplayOnOffControl(), 
            new DisplayFunction(lines, dotSize, _dataPinMode), 
            new DisplayEntryMode(), 
            new DisplayCursorShift());

        Initialize();
    }

    private void Initialize()
    {
        const uint cols = 16;
        SetRowOffsets(0x00, 0x40, 0x00 + cols, 0x40 + cols);

        _gpio.OpenPin(_rsPin, PinMode.Output);
        if (_rwPin != 255)
            _gpio.OpenPin(_rwPin, PinMode.Output);
        _gpio.OpenPin(_enablePin, PinMode.Output);

        // Do these once, instead of every time a character is drawn for speed reasons.
        for (var i = 0; i < (int)_dataPinMode; ++i)
            _gpio.OpenPin(_dataPins[i], PinMode.Output);

        // SEE PAGE 45/46 FOR INITIALIZATION SPECIFICATION!
        // according to datasheet, we need at least 40 ms after power rises above 2.7 V
        // before sending commands. Arduino can turn on way before 4.5 V so we'll wait 50
        Thread.Sleep(50);
        // Now we pull both RS and R/W low to begin commands
        _gpio.Write(_rsPin, PinValue.Low);
        _gpio.Write(_enablePin, PinValue.Low);

        if (_rwPin != 255)
            _gpio.Write(_rwPin, PinValue.Low);

        //put the LCD into 4 bit or 8 bit mode
        if (_dataPinMode == DataPinMode.Four)
        {
            // this is according to the Hitachi HD44780 datasheet
            // figure 24, pg 46
            // we start in 8bit mode, try to set 4 bit mode
            Write4Bits(0x03);
            DelayMicroseconds(4500, false); // wait min 4.1ms 
            // second try
            Command(_command.FunctionSet);
            DelayMicroseconds(150, false);
            // third go
            Command(_command.FunctionSet);
        }
        // finally, set # lines, font size, etc.
        Command(_command.FunctionSet);

        Display();
        // clear it off
        Clear();
        // set the entry mode
        Command(_command.EntryMode);
    }

    private void Command(uint value)
    {
        Send(value, PinValue.Low);
    }
    private void Send(uint value, PinValue mode)
    {
        _gpio.Write(_rsPin, mode);

        // if there is a RW pin indicated, set it low to Write
        if (_rwPin != 255) _gpio.Write(_rwPin, PinValue.Low);

        if (_dataPinMode == DataPinMode.Eight)
            Write8Bits(value);
        else
        {
            Write4Bits(value >> 4);
            Write4Bits(value);
        }
    }
    private void SetRowOffsets(uint row0, uint row1, uint row2, uint row3)
    {
        _rowOffsets[0] = row0;
        _rowOffsets[1] = row1;
        _rowOffsets[2] = row2;
        _rowOffsets[3] = row3;
    }
    private void PulseEnable()
    {
        _gpio.Write(_enablePin, PinValue.Low);
        DelayMicroseconds(1, false);
        _gpio.Write(_enablePin, PinValue.High);
        DelayMicroseconds(1, false);    // enable pulse must be >450 ns
        _gpio.Write(_enablePin, PinValue.Low);
        DelayMicroseconds(100, false);   // commands need >37 us to settle
    }
    private void Write4Bits(uint value)
    {
        for (var i = 0; i < 4; i++)
            _gpio.Write(_dataPins[i], ((int)value >> i) & 0x01);
        PulseEnable();
    }
    private void Write8Bits(uint value)
    {
        for (var i = 0; i < 8; i++)
            _gpio.Write(_dataPins[i], ((int)value >> i) & 0x01);
        PulseEnable();
    }

    /********** high level commands, for the user! */
    public void Write(uint value) => Send(value, PinValue.High);
    public void Write(string message)
    {
        foreach (char c in message) Write(c);
    }
    public void Clear()
    {
        Send(_command.ClearDisplay, PinValue.Low);  // clear display, set cursor position to zero
        DelayMicroseconds(2000, false);  // this command takes a long time!
    }
    public void NoDisplay() => Command(_command.NoDisplay);
    public void Display() => Command(_command.Display);
    public void NoBlink() => Command(_command.NoBlink);
    public void Blink() => Command(_command.Blink);
}

