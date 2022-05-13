using System.Device;
using System.Device.Gpio;
using System.Threading;

namespace LCD1602
{
    public class ControlDisplay
    {
        public readonly uint LCD_DISPLAYON = 0x04;
        public readonly uint LCD_DISPLAYOFF = 0x00;
        public readonly uint LCD_CURSORON = 0x02;
        public readonly uint LCD_CURSOROFF = 0x00;
        public readonly uint LCD_BLINKON = 0x01;
        public readonly uint LCD_BLINKOFF = 0x00;

        public ControlDisplay()
        {
        }
    }

    public class Command
    {
        public readonly uint LCD_RETURNHOME = 0x02;
        public readonly uint LCD_ENTRYMODESET = 0x04;
        public readonly uint LCD_DISPLAYCONTROL = 0x08;
        public readonly uint LCD_CURSORSHIFT = 0x10;
        public readonly uint LCD_FUNCTIONSET = 0x20;
        public readonly uint LCD_SETCGRAMADDR = 0x40;
        public readonly uint LCD_SETDDRAMADDR = 0x80;
        public readonly uint Cleardisplay = 0x01;

        public Command()
        {
        }
    }

    public class LiquidCrystal
    {
        // commands

        // flags for display entry mode     
        readonly uint LCD_ENTRYRIGHT = 0x00;
        readonly uint LCD_ENTRYLEFT = 0x02;
        readonly uint LCD_ENTRYSHIFTINCREMENT = 0x01;
        readonly uint LCD_ENTRYSHIFTDECREMENT = 0x00;

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


        int _rs_pin; // LOW: command. HIGH: character.
        int _rw_pin; // LOW: write to LCD. HIGH: read from LCD.
        int _enable_pin; // activated by a HIGH pulse.
        int[] _data_pins = new int[8];

        uint _displayfunction;
        uint _displaycontrol;
        uint _displaymode;

        uint _initialized;
        uint _numlines;

        readonly uint[] _row_offsets = new uint[4];
        private GpioController _gpio;
        private readonly ControlDisplay _controlDisplay;
        private readonly Command _command;

        public LiquidCrystal(int rs, int enable,
                 int d0, int d1, int d2, int d3)
        {
            _gpio = new GpioController();
            Init(1, rs, 255, enable, d0, d1, d2, d3, 0, 0, 0, 0);
            _controlDisplay = new ControlDisplay();
            _command = new Command();
        }

        private void Init(uint fourbitmode, int rs, int rw, int enable,
             int d0, int d1, int d2, int d3,
             int d4, int d5, int d6, int d7)
        {
            _rs_pin = rs;
            _rw_pin = rw;
            _enable_pin = enable;

            _data_pins[0] = d0;
            _data_pins[1] = d1;
            _data_pins[2] = d2;
            _data_pins[3] = d3;
            _data_pins[4] = d4;
            _data_pins[5] = d5;
            _data_pins[6] = d6;
            _data_pins[7] = d7;

            if (fourbitmode == 1)
                _displayfunction = LCD_4BITMODE | LCD_1LINE | LCD_5x8DOTS;
            else
                _displayfunction = LCD_8BITMODE | LCD_1LINE | LCD_5x8DOTS;

            Begin(16, 1);
        }

        public void Begin(uint cols, uint lines, uint dotsize = 0x00)
        {
            _numlines = lines;
            SetRowOffsets(0x00, 0x40, 0x00 + cols, 0x40 + cols);

            if ((dotsize != LCD_5x8DOTS) && (lines == 1))
            {
                _displayfunction |= LCD_5x10DOTS;
            }

            _gpio.OpenPin(_rs_pin, PinMode.Output);

            if (_rw_pin != 255)
            {
                _gpio.OpenPin(_rw_pin, PinMode.Output);
            }
            _gpio.OpenPin(_enable_pin, PinMode.Output);

            // Do these once, instead of every time a character is drawn for speed reasons.
            for (int i = 0; i < ((_displayfunction & LCD_8BITMODE) == 1 ? 8 : 4); ++i)
            {
                _gpio.OpenPin(_data_pins[i], PinMode.Output);
            }

            // SEE PAGE 45/46 FOR INITIALIZATION SPECIFICATION!
            // according to datasheet, we need at least 40 ms after power rises above 2.7 V
            // before sending commands. Arduino can turn on way before 4.5 V so we'll wait 50
            Thread.Sleep(50);
            // Now we pull both RS and R/W low to begin commands
            _gpio.Write(_rs_pin, PinValue.Low);
            _gpio.Write(_enable_pin, PinValue.Low);

            if (_rw_pin != 255)
                _gpio.Write(_rw_pin, PinValue.Low);

            //put the LCD into 4 bit or 8 bit mode
            if ((_displayfunction & LCD_8BITMODE) == 0)
            {
                // this is according to the Hitachi HD44780 datasheet
                // figure 24, pg 46
                // we start in 8bit mode, try to set 4 bit mode
                Write4bits(0x03);
                DelayHelper.DelayMicroseconds(4500, false); // wait min 4.1ms 

                // second try
                Command(_command.LCD_FUNCTIONSET | _displayfunction);
                DelayHelper.DelayMicroseconds(150, false);

                // third go
                Command(_command.LCD_FUNCTIONSET | _displayfunction);
            }
            // finally, set # lines, font size, etc.
            Command(_command.LCD_FUNCTIONSET | _displayfunction);

            // turn the display on with no cursor or blinking default
            _displaycontrol = _controlDisplay.LCD_DISPLAYON | _controlDisplay.LCD_CURSOROFF | _controlDisplay.LCD_BLINKOFF;
            Display();

            // clear it off
            Clear();

            // Initialize to default text direction (for romance languages)
            _displaymode = LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT;
            // set the entry mode
            Command(_command.LCD_ENTRYMODESET | _displaymode);
        }
        void Command(uint value)
        {
            Send(value, PinValue.Low);
        }


        public void Write(uint value)
        {
            Send(value, PinValue.High);
        }


        void Send(uint value, PinValue mode)
        {
            _gpio.Write(_rs_pin, mode);

            // if there is a RW pin indicated, set it low to Write
            if (_rw_pin != 255)
            {
                _gpio.Write(_rw_pin, PinValue.Low);
            }

            if ((_displayfunction & LCD_8BITMODE) == 1)
            {
                Write8bits(value);
            }
            else
            {
                Write4bits(value >> 4);
                Write4bits(value);
            }
        }
        void SetRowOffsets(uint row0, uint row1, uint row2, uint row3)
        {
            _row_offsets[0] = row0;
            _row_offsets[1] = row1;
            _row_offsets[2] = row2;
            _row_offsets[3] = row3;
        }

        void PulseEnable()
        {
            _gpio.Write(_enable_pin, PinValue.Low);
            DelayHelper.DelayMicroseconds(1, false);
            _gpio.Write(_enable_pin, PinValue.High);
            DelayHelper.DelayMicroseconds(1, false);    // enable pulse must be >450 ns
            _gpio.Write(_enable_pin, PinValue.Low);
            DelayHelper.DelayMicroseconds(100, false);   // commands need >37 us to settle
        }

        private void Write4bits(uint value)
        {
            for (int i = 0; i < 4; i++)
            {
                _gpio.Write(_data_pins[i], ((int)value >> i) & 0x01);
            }

            PulseEnable();
        }

        void Write8bits(uint value)
        {
            for (int i = 0; i < 8; i++)
            {
                _gpio.Write(_data_pins[i], ((int)value >> i) & 0x01);
            }
            PulseEnable();
        }

        /********** high level commands, for the user! */
        public void Clear()
        {
            Command(_command.Cleardisplay);  // clear display, set cursor position to zero
            DelayHelper.DelayMicroseconds(2000, false);  // this command takes a long time!
        }

        // Turn the display on/off (quickly)
        void NoDisplay()
        {
            _displaycontrol &= ~_controlDisplay.LCD_DISPLAYON;
            Command(_command.LCD_DISPLAYCONTROL | _displaycontrol);
        }
        void Display()
        {
            _displaycontrol |= _controlDisplay.LCD_DISPLAYON;
            Command(_command.LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // Turn on and off the blinking cursor
        void NoBlink()
        {
            _displaycontrol &= ~_controlDisplay.LCD_BLINKON;
            Command(_command.LCD_DISPLAYCONTROL | _displaycontrol);
        }
        void Blink()
        {
            _displaycontrol |= _controlDisplay.LCD_BLINKON;
            Command(_command.LCD_DISPLAYCONTROL | _displaycontrol);
        }
    }
}
