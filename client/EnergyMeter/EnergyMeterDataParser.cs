using System.Globalization;
using System.Text.RegularExpressions;

namespace client.EnergyMeter;

public static class EnergyMeterDataParser
{
    public static RawEnergyMeterData Parse(string data)
    {
        var payload = data.Split("data\":")[1];
        var parser = Parser.ResolveParser(payload);

        var parsed = new RawEnergyMeterData
        {
            ActiveEnergyOutlet = parser.ParseProperty(Parser.Properties.ActiveEnergyOutlet),
            ActiveEnergyInput = parser.ParseProperty(Parser.Properties.ActiveEnergyInput),
            ReactiveEnergyOutlet = parser.ParseProperty(Parser.Properties.ReactiveEnergyOutlet),
            ReactiveEnergyInput = parser.ParseProperty(Parser.Properties.ReactiveEnergyInput),
            ActivePowerOutlet = parser.ParseProperty(Parser.Properties.ActivePowerOutlet),
            ActivePowerInput = parser.ParseProperty(Parser.Properties.ActivePowerInput),
            ReactivePowerOutlet = parser.ParseProperty(Parser.Properties.ReactivePowerOutlet),
            ReactivePowerInput = parser.ParseProperty(Parser.Properties.ReactivePowerInput),
            ActivePowerOutlet1 = parser.ParseProperty(Parser.Properties.ActivePowerOutlet1),
            ActivePowerInput1 = parser.ParseProperty(Parser.Properties.ActivePowerInput1),
            ActivePowerOutlet2 = parser.ParseProperty(Parser.Properties.ActivePowerOutlet2),
            ActivePowerInput2 = parser.ParseProperty(Parser.Properties.ActivePowerInput2),
            ActivePowerOutlet3 = parser.ParseProperty(Parser.Properties.ActivePowerOutlet3),
            ActivePowerInput3 = parser.ParseProperty(Parser.Properties.ActivePowerInput3),
            ReactivePowerOutlet1 = parser.ParseProperty(Parser.Properties.ReactivePowerOutlet1),
            ReactivePowerInput1 = parser.ParseProperty(Parser.Properties.ReactivePowerInput1),
            ReactivePowerOutlet2 = parser.ParseProperty(Parser.Properties.ReactivePowerOutlet2),
            ReactivePowerInput2 = parser.ParseProperty(Parser.Properties.ReactivePowerInput2),
            ReactivePowerOutlet3 = parser.ParseProperty(Parser.Properties.ReactivePowerOutlet3),
            ReactivePowerInput3 = parser.ParseProperty(Parser.Properties.ReactivePowerInput3),
            PhaseCurrent1 = parser.ParseProperty(Parser.Properties.PhaseCurrent1),
            PhaseCurrent2 = parser.ParseProperty(Parser.Properties.PhaseCurrent2),
            PhaseCurrent3 = parser.ParseProperty(Parser.Properties.PhaseCurrent3),
            PhaseVoltage1 = parser.ParseProperty(Parser.Properties.PhaseVoltage1),
            PhaseVoltage2 = parser.ParseProperty(Parser.Properties.PhaseVoltage2),
            PhaseVoltage3 = parser.ParseProperty(Parser.Properties.PhaseVoltage3)
        };

        return parsed;
    }

    public abstract class Parser
    {
        public enum Properties
        {
            ActiveEnergyOutlet,
            ActiveEnergyInput,
            ReactiveEnergyOutlet,
            ReactiveEnergyInput,
            ActivePowerOutlet,
            ActivePowerInput,
            ReactivePowerOutlet,
            ReactivePowerInput,
            ActivePowerOutlet1,
            ActivePowerInput1,
            ActivePowerOutlet2,
            ActivePowerInput2,
            ActivePowerOutlet3,
            ActivePowerInput3,
            ReactivePowerOutlet1,
            ReactivePowerInput1,
            ReactivePowerOutlet2,
            ReactivePowerInput2,
            ReactivePowerOutlet3,
            ReactivePowerInput3,
            PhaseCurrent1,
            PhaseCurrent2,
            PhaseCurrent3,
            PhaseVoltage1,
            PhaseVoltage2,
            PhaseVoltage3
        }

        private const string KamstrupV0001 = "Kamstrup_V0001";

        protected readonly string Payload;

        protected Parser(string payload) => Payload = payload;

        public static Parser ResolveParser(string payload)
        {
            if (payload.Contains(KamstrupV0001))
            {
                return new KamstrupV0001Parser(payload);
            }

            return new P1Parser(payload);
        }

        protected static decimal ParseDecimal(string value, decimal scale = 1)
        {
            try
            {
                return decimal.Parse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture) / scale;
            }
            catch (FormatException)
            {
                return 0;
            }
        }

        public abstract decimal ParseProperty(Properties value);
    }

    private class P1Parser : Parser
    {
        private readonly Dictionary<Properties, string> _identifiers = new()
        {
            { Properties.ActiveEnergyOutlet, "1-0:1.8.0" },
            { Properties.ActiveEnergyInput, "1-0:2.8.0" },
            { Properties.ReactiveEnergyOutlet, "1-0:3.8.0" },
            { Properties.ReactiveEnergyInput, "1-0:4.8.0" },
            { Properties.ActivePowerOutlet, "1-0:1.7.0" },
            { Properties.ActivePowerInput, "1-0:2.7.0" },
            { Properties.ReactivePowerOutlet, "1-0:3.7.0" },
            { Properties.ReactivePowerInput, "1-0:4.7.0" },
            { Properties.ActivePowerOutlet1, "1-0:21.7.0" },
            { Properties.ActivePowerOutlet2, "1-0:41.7.0" },
            { Properties.ActivePowerOutlet3, "1-0:61.7.0" },
            { Properties.ActivePowerInput1, "1-0:22.7.0" },
            { Properties.ActivePowerInput2, "1-0:42.7.0" },
            { Properties.ActivePowerInput3, "1-0:62.7.0" },
            { Properties.ReactivePowerOutlet1, "1-0:23.7.0" },
            { Properties.ReactivePowerOutlet2, "1-0:43.7.0" },
            { Properties.ReactivePowerOutlet3, "1-0:63.7.0" },
            { Properties.ReactivePowerInput1, "1-0:24.7.0" },
            { Properties.ReactivePowerInput2, "1-0:44.7.0" },
            { Properties.ReactivePowerInput3, "1-0:64.7.0" },
            { Properties.PhaseCurrent1, "1-0:31.7.0" },
            { Properties.PhaseCurrent2, "1-0:51.7.0" },
            { Properties.PhaseCurrent3, "1-0:71.7.0" },
            { Properties.PhaseVoltage1, "1-0:32.7.0" },
            { Properties.PhaseVoltage2, "1-0:52.7.0" },
            { Properties.PhaseVoltage3, "1-0:72.7.0" }
        };

        public P1Parser(string payload) : base(payload)
        {
        }

        public override decimal ParseProperty(Properties property)
        {
            if (!_identifiers.TryGetValue(property, out var value)) return 0;

            var pattern = $@"{value}\((\d+([\.,]\d+)*)";
            var match = new Regex(pattern).Match(Payload);

            return match.Success ? ParseDecimal(match.Groups[1].Value) : 0;
        }
    }

    private class KamstrupV0001Parser : Parser
    {
        private readonly Dictionary<Properties, (string, int)> _identifiers = new()
        {
            { Properties.ActiveEnergyOutlet, ("1.*.1.8.0.255", 1) },
            { Properties.ActiveEnergyInput, ("1.*.2.8.0.255", 1) },
            { Properties.ReactiveEnergyOutlet, ("1.*.3.8.0.255", 1) },
            { Properties.ReactiveEnergyInput, ("1.*.4.8.0.255", 1) },
            { Properties.ActivePowerOutlet, ("1.1.1.7.0.255", 1000) },
            { Properties.ActivePowerInput, ("1.1.2.7.0.255", 1000) },
            { Properties.ReactivePowerOutlet, ("1.*.3.7.0.255", 1) },
            { Properties.ReactivePowerInput, ("1.*.4.7.0.255", 1) },
            { Properties.PhaseCurrent1, ("1.*.31.7.0.255", 100) },
            { Properties.PhaseCurrent2, ("1.*.51.7.0.255", 100) },
            { Properties.PhaseCurrent3, ("1.*.71.7.0.255", 100) },
            { Properties.PhaseVoltage1, ("1.*.32.7.0.255", 1) },
            { Properties.PhaseVoltage2, ("1.*.52.7.0.255", 1) },
            { Properties.PhaseVoltage3, ("1.*.72.7.0.255", 1) }
        };

        public KamstrupV0001Parser(string payload) : base(payload)
        {
        }

        public override decimal ParseProperty(Properties property)
        {
            if (!_identifiers.TryGetValue(property, out var value)) return 0;

            var (identifier, scale) = value;
            var pattern = $"{identifier}:(\\d+)";
            var match = new Regex(pattern).Match(Payload);

            return match.Success ? ParseDecimal(match.Groups[1].Value, scale) : 0;
        }
    }
}