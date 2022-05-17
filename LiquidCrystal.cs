using System.Device;
using System.Device.Gpio;
using System.Threading;
using LCD1602;

namespace LiquidCrystal
{
    public class LiquidCrystal
    {
        private readonly int _rsPin; // LOW: command. HIGH: character.
        private readonly int _rwPin; // LOW: write to LCD. HIGH: read from LCD.
        private readonly int _enablePin; // activated by a HIGH pulse.
        private readonly int[] _dataPins = new int[8];

        private readonly GpioController _gpio = new();
        private readonly DataPinMode _dataPinMode;
        private DisplayFunctionCommand _displayFunctionCommand;
        private DisplayEntryModeCommand _displayEntryModeCommand;
        private DisplayCursorShiftCommand _displayCursorShiftCommand;
        private DisplayOnOffControlCommand _displayOnOffControlCommand;
        private CustomCommand _customCommand;

        public LiquidCrystal(int rs, int enable,
            int d0, int d1, int d2, int d3,
             uint lines = 1, int rwPin = 255, uint dotSize = 0x00)
        {
            _rsPin = rs;
            _rwPin = rwPin;
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

            Initialize(lines, dotSize);
        }
        public LiquidCrystal(int rs, int enable,
            int d0, int d1, int d2, int d3,
            int d4, int d5, int d6, int d7,
            int rwPin = 255, uint lines = 1, uint dotSize = 0x00)
        {
            _rsPin = rs;
            _rwPin = rwPin;
            _enablePin = enable;
            _dataPins[0] = d0;
            _dataPins[1] = d1;
            _dataPins[2] = d2;
            _dataPins[3] = d3;
            _dataPins[4] = d4;
            _dataPins[5] = d5;
            _dataPins[6] = d6;
            _dataPins[7] = d7;
            _dataPinMode = DataPinMode.Eight;

            Initialize(lines, dotSize);
        }

        private void Initialize(uint lines, uint dotSize)
        {
            _displayFunctionCommand = new DisplayFunctionCommand(lines, dotSize, _dataPinMode);
            _displayEntryModeCommand = new DisplayEntryModeCommand(lines);
            _displayCursorShiftCommand = new DisplayCursorShiftCommand();
            _displayOnOffControlCommand = new DisplayOnOffControlCommand();
            _customCommand = new CustomCommand();

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
                DelayHelper.DelayMicroseconds(4500, false); // wait min 4.1ms 
                // second try
                Command(_displayFunctionCommand.FunctionSet);
                DelayHelper.DelayMicroseconds(150, false);
                // third go
                Command(_displayFunctionCommand.FunctionSet);
            }
            // finally, set # lines, font size, etc.
            Command(_displayFunctionCommand.FunctionSet);

            Display();
            // clear it off
            Clear();
            // set the entry mode
            Command(_displayEntryModeCommand.EntryModeSet);
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
        private void PulseEnable()
        {
            _gpio.Write(_enablePin, PinValue.Low);
            DelayHelper.DelayMicroseconds(1, false);
            _gpio.Write(_enablePin, PinValue.High);
            DelayHelper.DelayMicroseconds(1, false);    // enable pulse must be >450 ns
            _gpio.Write(_enablePin, PinValue.Low);
            DelayHelper.DelayMicroseconds(100, false);   // commands need >37 us to settle
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
            Send(_displayFunctionCommand.ClearDisplay, PinValue.Low);  // clear display, set cursor position to zero
            DelayHelper.DelayMicroseconds(2000, false);  // this command takes a long time!
        }
        public void NoDisplay() => Command(_displayOnOffControlCommand.NoDisplay);
        public void Display() => Command(_displayOnOffControlCommand.Display);
        public void NoBlink() => Command(_displayOnOffControlCommand.NoBlink);
        public void Blink() => Command(_displayOnOffControlCommand.Blink);

        public void NoCursor() => Command(_displayOnOffControlCommand.NoCursor);
        public void Cursor() => Command(_displayOnOffControlCommand.Cursor);

        public void Home()
        {
            Command(_displayFunctionCommand.ReturnHome);  // set cursor position to zero
            DelayHelper.DelayMicroseconds(2000, false);  // this command takes a long time!
        }

        public void SetCursor(uint col, uint row) => Command(_displayEntryModeCommand.SetCursor(col, row));
        public void ScrollDisplayLeft() => Command(_displayCursorShiftCommand.ScrollDisplayLeft);
        public void ScrollDisplayRight() => Command(_displayCursorShiftCommand.ScrollDisplayRight);
        public void LeftToRight() => Command(_displayEntryModeCommand.LeftToRight);
        public void RightToLeft() => Command(_displayEntryModeCommand.RightToLeft);
        public void AutoScroll() => Command(_displayEntryModeCommand.AutoScroll);
        public void NoAutoScroll() => Command(_displayEntryModeCommand.NoAutoScroll);
        // Allows us to fill the first 8 CGRAM locations
        // with custom characters
        public void CreateChar(uint location, uint[] charMap)
        {
            Command(_customCommand.CreateChar(location));
            for (var i = 0; i < 8; i++) Write(charMap[i]);
        }
    }
}