using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensi.Response
{

    public class TemperatureOffset
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }

    public class Scheduling
    {
        public int MaxStepsPerDay { get; set; }
        public int SeparateDaysPerWeek { get; set; }
    }

    public class HumidityOffset
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }
    

    public class HeatLimits
    {
        public Temperature Min { get; set; }
        public Temperature Max { get; set; }
    }

    public class Capabilities
    {
        public TemperatureOffset TemperatureOffset { get; set; }
        public List<string> SystemModes { get; set; }
        public Scheduling Scheduling { get; set; }
        public HumidityOffset HumidityOffset { get; set; }
        public bool HumidityDisplay { get; set; }
        public HeatLimits HeatLimits { get; set; }
        public List<string> HeatCycleRates { get; set; }
        public List<string> HoldModes { get; set; }
        public string IndoorEquipment { get; set; }
        public int IndoorStages { get; set; }
        public string OutdoorEquipment { get; set; }
        public int OutdoorStages { get; set; }
    }
    

    public class EnvironmentControls
    {
        public Temperature HeatSetpoint { get; set; }
        public string FanMode { get; set; }
        public string SystemMode { get; set; }
        public string ScheduleMode { get; set; }
        public string HoldMode { get; set; }
    }
    
    public class Running
    {
        public string Mode { get; set; }
        public TimeSpan Time { get; set; }
    }

    public class Heat
    {
        public int F { get; set; }
        public int C { get; set; }
    }

    public class ScheduleTemps
    {
        public Heat Heat { get; set; }
    }

    public class OperationalStatus
    {
        public Temperature Temperature { get; set; }
        public int Humidity { get; set; }
        private float _BatteryVoltage;
        public float BatteryVoltage
        {
            get { return _BatteryVoltage; }
            set { _BatteryVoltage = value / 1000F; }
        }

        public Running Running { get; set; }
        public bool LowPower { get; set; }
        public string OperatingMode { get; set; }
        public ScheduleTemps ScheduleTemps { get; set; }
        public int PowerStatus { get; set; }

        public OperationalStatus()
        {
            this.Temperature = new Temperature();
            this.Running = new Running();
            this.ScheduleTemps = new ScheduleTemps();
        }
    }

    public class Step
    {
        public string Time { get; set; }
        public Temperature Heat { get; set; }
        public Temperature Cool { get; set; }
    }

    public class Daily
    {
        public List<string> Days { get; set; }
        public List<Step> Steps { get; set; }
    }

    public class Schedule2
    {
        public string ObjectId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<Daily> Daily { get; set; }
    }

    public class Active
    {
        public string Heat { get; set; }
        public string Cool { get; set; }
        public string Auto { get; set; }
        public string Running { get; set; }
    }


    public class Projection
    {
        public string Day { get; set; }
        public string ProjectionType { get; set; }
        public string Time { get; set; }
        public Temperature Heat { get; set; }
    }

    public class Schedule
    {
        public List<Schedule2> Schedules { get; set; }
        public Active Active { get; set; }
        public List<Projection> Projection { get; set; }
    }

    public class Settings
    {
        public string Degrees { get; set; }
        public int TemperatureOffset { get; set; }
        public Mode ComfortRecovery { get; set; }
        public Mode LocalTimeDisplay { get; set; }
        public Mode KeypadLockout { get; set; }
        public Mode ContinuousBacklight { get; set; }
        public Mode LocalHumidityDisplay { get; set; }
        public int HumidityOffset { get; set; }
        public HeatCycleRate HeatCycleRate { get; set; }
    }

    public class Firmware
    {
        public string Bootloader { get; set; }
        public string Thermostat { get; set; }
        public string Wireless { get; set; }
    }

    public class Product
    {
        public string MacAddress { get; set; }
        public Firmware Firmware { get; set; }
        public string ModelNumber { get; set; }
    }
    public abstract class OnlineResponseBase
    {
        public Capabilities Capabilities { get; set; }
        public EnvironmentControls EnvironmentControls { get; set; }
        public OperationalStatus OperationalStatus { get; set; }
        public Schedule Schedule { get; set; }
        public Settings Settings { get; set; }
        public Product Product { get; set; }
    }
    public class OnlineResponse : OnlineResponseBase { }
}
