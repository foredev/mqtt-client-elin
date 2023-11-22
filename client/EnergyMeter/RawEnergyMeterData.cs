namespace client.EnergyMeter;

public class RawEnergyMeterData
{
    /// <summary>
    /// kW
    /// </summary>
    public decimal ActiveEnergyOutlet { get; set; } 
    /// <summary>
    /// kW
    /// </summary>
    public decimal ActiveEnergyInput { get; set; }
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactiveEnergyOutlet { get; set; } 
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactiveEnergyInput { get; set; }
    /// <summary>
    /// kW
    /// </summary>
    public decimal ActivePowerOutlet { get; set; }
    /// <summary>
    /// kW
    /// </summary>
    public decimal ActivePowerInput { get; set; }
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactivePowerOutlet { get; set; }
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactivePowerInput { get; set; }
    /// <summary>
    /// Ignore
    /// </summary>
    public decimal ActivePowerOutlet1 { get; set; }
    /// <summary>
    /// Ignore
    /// </summary>
    public decimal ActivePowerOutlet2 { get; set; }
    /// <summary>
    /// Ignore
    /// </summary>
    public decimal ActivePowerOutlet3 { get; set; }
    /// <summary>
    /// Ignore
    /// </summary>
    public decimal ActivePowerInput1 { get; set; }
    /// <summary>
    /// Ignore
    /// </summary>
    public decimal ActivePowerInput2 { get; set; }
    /// <summary>
    /// Ignore
    /// </summary>
    public decimal ActivePowerInput3 { get; set; }
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactivePowerOutlet1 { get; set; } 
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactivePowerOutlet2 { get; set; }
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactivePowerOutlet3 { get; set; } 
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactivePowerInput1 { get; set; }
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactivePowerInput2 { get; set; }
    /// <summary>
    /// W
    /// </summary>
    public decimal ReactivePowerInput3 { get; set; }
    /// <summary>
    /// Volt
    /// </summary>
    public decimal PhaseVoltage1 { get; set; }
    /// <summary>
    /// Ampere
    /// </summary>
    public decimal PhaseCurrent1 { get; set; } 
    /// <summary>
    /// Volt
    /// </summary>
    public decimal PhaseVoltage2 { get; set; }
    /// <summary>
    /// Ampere
    /// </summary>
    public decimal PhaseCurrent2 { get; set; } 
    /// <summary>
    /// Volt
    /// </summary>
    public decimal PhaseVoltage3 { get; set; } 
    /// <summary>
    /// Ampere
    /// </summary>
    public decimal PhaseCurrent3 { get; set; }
}