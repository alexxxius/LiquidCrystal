using System.Device.Gpio;
using System.Threading;
using static System.Device.DelayHelper;

namespace LCD1602
{
    public class DisplayOnOffControl
    {
        private const uint DisplayOn = 0x04;
        private readonly uint LCD_DISPLAYOFF = 0x00;
        private readonly uint LCD_CURSORON = 0x02;
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

    public class Command
    {
        private readonly DisplayOnOffControl _displayOnOffControl;
        public readonly uint LCD_RETURNHOME = 0x02;

        public readonly uint LCD_ENTRYMODESET = 0x04;
        private readonly uint _displaycontrol = 0x08;
        public readonly uint LCD_CURSORSHIFT = 0x10;
        public readonly uint LCD_FUNCTIONSET = 0x20;
        public readonly uint LCD_SETCGRAMADDR = 0x40;
        public readonly uint LCD_SETDDRAMADDR = 0x80;

        public Command(DisplayOnOffControl displayOnOffControl)
        {
            _displayOnOffControl = displayOnOffControl;
        }

        public uint ClearDisplay => 0x01;

        public uint NoDisplay => _displaycontrol | _displayOnOffControl.NoDisplay;
        public uint Display => _displaycontrol | _displayOnOffControl.Display;
        public uint NoBlink => _displaycontrol | _displayOnOffControl.NoBlink;
        public uint Blink => _displaycontrol | _displayOnOffControl.Blink;

    }

    public class LiquidCrystal
    {
        // commands

        // flags for display entry mode     
        readonly uint _entryRight = 0x00;
        readonly uint _entryLeft = 0x02;
        private readonly uint _entryShiftIncrement = 0x01;
        readonly uint _entryShiftDecrement = 0x00;

        // flags for display on/off control 

        // flags for display/cursor shift   ;
        readonly uint LCD_DISPLAYMOVE = 0x08;
        readonly uint LCD_CURSORMOVE = 0x00;
        readonly uint LCD_MOVERIGHT = 0x04;
        readonly uint LCD_MOVELEFT = 0x00;

        // flags for function set           ;
        readonly uint LCD_8BITMODE = 0x10;
        readonly uint LCD_4BITMODE = 0x00;
        readonly uint LCD_2LINE = 0x08;
        readonly uint LCD_1LINE = 0x00;
        readonly uint LCD_5x10DOTS = 0x04;
        readonly uint LCD_5x8DOTS = 0x00;


        readonly int _rsPin; // LOW: command. HIGH: character.
        readonly int _rwPin; // LOW: write to LCD. HIGH: read from LCD.
        readonly int _enablePin; // activated by a HIGH pulse.
        readonly int[] _dataPins = new int[8];

        uint _displayFunction;
        uint _displayMode;

        uint _initialized;
        uint _numlines;

        readonly uint[] _rowOffsets = new uint[4];
        private readonly GpioController _gpio = new();
        private readonly Command _command = new(new DisplayOnOffControl());

        public LiquidCrystal(int rs, int enable,
                 int d0, int d1, int d2, int d3, uint lines = 1, uint dotSize = 0x00)
        {
            _rsPin = rs;
            _rwPin = 255;
            _enablePin = enable;
            _numlines = lines;
            _dataPins[0] = d0;
            _dataPins[1] = d1;
            _dataPins[2] = d2;
            _dataPins[3] = d3;
            _dataPins[4] = 0;
            _dataPins[5] = 0;
            _dataPins[6] = 0;
            _dataPins[7] = 0;

            _displayFunction = LCD_4BITMODE | LCD_1LINE | LCD_5x8DOTS;

            //  _displayFunction = LCD_8BITMODE | LCD_1LINE | LCD_5x8DOTS;
            if ((dotSize != LCD_5x8DOTS) && (lines == 1)) 
                _displayFunction |= LCD_5x10DOTS;
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
            for (var i = 0; i < ((_displayFunction & LCD_8BITMODE) == 1 ? 8 : 4); ++i)
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
            if ((_displayFunction & LCD_8BITMODE) == 0)
            {
                // this is according to the Hitachi HD44780 datasheet
                // figure 24, pg 46
                // we start in 8bit mode, try to set 4 bit mode
                Write4Bits(0x03);
                DelayMicroseconds(4500, false); // wait min 4.1ms 

                // second try
                Command(_command.LCD_FUNCTIONSET | _displayFunction);
                DelayMicroseconds(150, false);

                // third go
                Command(_command.LCD_FUNCTIONSET | _displayFunction);
            }
            // finally, set # lines, font size, etc.
            Command(_command.LCD_FUNCTIONSET | _displayFunction);

            Display();

            // clear it off
            Clear();

            // Initialize to default text direction (for romance languages)
            _displayMode = _entryLeft | _entryShiftDecrement;
            // set the entry mode
            Command(_command.LCD_ENTRYMODESET | _displayMode);
        }

        private void Command(uint value)
        {
            Send(value, PinValue.Low);
        }
        private void Send(uint value, PinValue mode)
        {
            _gpio.Write(_rsPin, mode);

            // if there is a RW pin indicated, set it low to Write
            if (_rwPin != 255)
            {
                _gpio.Write(_rwPin, PinValue.Low);
            }

            if ((_displayFunction & LCD_8BITMODE) == 1)
            {
                Write8Bits(value);
            }
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
}
