﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Arduino.Tests;
using UnitsNet;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// Basic firmata integrations tests. These tests require functional hardware, that is an Arduino, loaded with a matching firmata firmware. It can also
    /// run against the *ExtendedConfigurableFirmata" simulator. If neither is found, the test is marked as "Inconclusive".
    /// </summary>
    public sealed class BasicFirmataIntegrationTests : IClassFixture<FirmataTestFixture>
    {
        private readonly FirmataTestFixture _fixture;

        public BasicFirmataIntegrationTests(FirmataTestFixture fixture)
        {
            _fixture = fixture;
            Skip.If(_fixture.Board == null, "No Board found");
            Board = _fixture.Board;
        }

        public ArduinoBoard Board
        {
            get;
        }

        [SkippableFact]
        public void CheckFirmwareVersion()
        {
            Assert.False(string.IsNullOrWhiteSpace(Board.FirmwareName));
            Assert.NotEqual(new Version(), Board.FirmwareVersion);
            Assert.True(Board.FirmwareVersion >= Version.Parse("2.11"));
        }

        [SkippableFact]
        public void CheckFirmataVersion()
        {
            Assert.NotNull(Board.FirmataVersion);
            Assert.True(Board.FirmataVersion >= Version.Parse("2.5"));
        }

        /// <summary>
        /// Verifies the pin capability message. Also verifies that the arduino is configured properly for these tests
        /// </summary>
        [SkippableFact]
        public void CheckBoardFeatures()
        {
            var caps = Board.SupportedPinConfigurations;
            Assert.NotNull(caps);
            Assert.True(caps.Count > 0);
            // These are minimum numbers all Arduinos should support, so this test should also pass on hardware (when all required modules are present)
            Assert.True(caps.Count(x => x.PinModes.Contains(SupportedMode.DigitalOutput)) > 10);
            Assert.True(caps.Count(x => x.PinModes.Contains(SupportedMode.DigitalInput)) > 10);
            Assert.True(caps.Count(x => x.PinModes.Contains(SupportedMode.AnalogInput)) > 5);
            Assert.True(caps.Count(x => x.PinModes.Contains(SupportedMode.I2c)) >= 2);
            Assert.True(caps.Count(x => x.PinModes.Contains(SupportedMode.Spi)) >= 3);
        }

        [SkippableFact]
        public void CanBlink()
        {
            var ctrl = Board.CreateGpioController();
            Assert.NotNull(ctrl);
            ctrl.OpenPin(6, PinMode.Output);
            ctrl.SetPinMode(6, PinMode.Output);
            ctrl.Write(6, PinValue.High);
            Thread.Sleep(100);
            ctrl.Write(6, PinValue.Low);
            ctrl.SetPinMode(6, PinMode.Input);
            ctrl.ClosePin(6);
        }

        [SkippableFact]
        public void SetPinMode()
        {
            var ctrl = Board.CreateGpioController();
            Assert.NotNull(ctrl);
            ctrl.OpenPin(6, PinMode.Output);
            ctrl.SetPinMode(6, PinMode.Output);
            Assert.Equal(PinMode.Output, ctrl.GetPinMode(6));
            ctrl.SetPinMode(6, PinMode.Input);
            Assert.Equal(PinMode.Input, ctrl.GetPinMode(6));

            ctrl.SetPinMode(6, PinMode.InputPullUp);
            Assert.Equal(PinMode.InputPullUp, ctrl.GetPinMode(6));

            Assert.Throws<InvalidOperationException>(() => ctrl.SetPinMode(6, PinMode.InputPullDown));
            ctrl.ClosePin(6);
        }

        [SkippableFact]
        public void ReadAnalog()
        {
            int pinNumber = GetFirstAnalogPin(Board);
            var ctrl = Board.CreateAnalogController(0);
            var pin = ctrl.OpenPin(pinNumber);
            Assert.NotNull(pin);
            var result = pin.ReadVoltage();
            Assert.True(result >= ElectricPotential.Zero && result <= ElectricPotential.FromVolts(5.1));
            ctrl.Dispose();
        }

        private static int GetFirstAnalogPin(ArduinoBoard board)
        {
            int analogPin = 14;
            foreach (var pin in board.SupportedPinConfigurations)
            {
                if (pin.AnalogPinNumber == 0)
                {
                    analogPin = pin.Pin;
                    break;
                }
            }

            return analogPin;
        }
    }
}
