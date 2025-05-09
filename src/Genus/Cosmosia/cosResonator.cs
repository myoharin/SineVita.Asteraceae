using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using SineVita.Muguet;
using Caprifolium;


namespace SineVita.Muguet.Asteraceae.Cosmosia {
    
    public class ResonatorCosmosia : Resonator// handles the resontor logic of objects which can qualify to be a resonator
    {   
        // * static Constants
        public static int InactivityTolerance { get; } = 2048; // ms
        public static byte InactivityThreshold { get; } = 16; // pulse intensity
        public static bool CheckInactivity { get; } = false;
        
        // * Core Class Info
        public int ResonatorParameterId { get; set; }
        public float SizeMutiplier { get; set; }
        public bool AddPulseLowerThanOrigin { get; set; }

        // * Class Variables (updated during process)
        public CosmosiaResonatorState State = CosmosiaResonatorState.Static;        
        public float Resonance { get; set; }
        public int CriticalOverflowDuration { get; set; } // ms
        public CosmosiaLonicera Lonicera;

        // * Derived Gets
        public ResonatorParameterCosmosia Parameter { get {
            return ResonanceHelperCosmosia.GetResonatorParameter(ResonatorParameterId);
        } }
        public Pulse OriginPulse { get {
            var parameter = Parameter;
            return new Pulse(parameter.Origin, parameter.OriginIntensity);
        } }
        public CosmosiaPulse OriginCosmosiaPulse { get {
            return new CosmosiaPulse(OriginPulse, true);
        } }

        public float NetInflow { get {
            return Math.Min(Lonicera.ProjectedInflowRate(Parameter, SizeMutiplier), Parameter.InflowLimit);
        } }
        public float NetOutflow { get {
            return Lonicera.NetOutflowRate();
        } }
        public float NetOverflow { get {
            return Lonicera.NetOverflowRate();
        } }

        public IReadOnlyList<CosmosiaPulse?> Pulses { get {
            return Lonicera.Nodes;
        } }
        public IReadOnlyList<CosmosiaChannel?> Channels { get {
            return Lonicera.Links;
        } }

        // * Constructor
        public ResonatorCosmosia(int resonatorParameterId, float sizeMutiplier = 1.0f, bool addPulseLowerThanOrigin = false, float resonance = 0, int criticalOverflowDuration = 0) 
            : base(Genus.Cosmosia) {
            ResonatorParameterId = resonatorParameterId;
            SizeMutiplier = sizeMutiplier;
            AddPulseLowerThanOrigin = addPulseLowerThanOrigin;
            Lonicera = new CosmosiaLonicera(OriginCosmosiaPulse);
            Resonance = resonance;
            CriticalOverflowDuration = criticalOverflowDuration;
        }

        // * Pulse Manipulation & Overrides
        public override bool AddPulse(Pulse newPulse) {
            if (!AddPulseLowerThanOrigin && ResonanceHelperCosmosia.GetResonatorParameter(ResonatorParameterId).Origin.Frequency > newPulse.Pitch.Frequency
                || CheckInactivity && newPulse.Intensity < InactivityThreshold
                || Lonicera.Nodes.Any(pulse => pulse != null && pulse.PulseID == newPulse.PulseID)
            )
            {return false;}

            int findIndex() {
                CosmosiaPulse? cachePulse;
                for (int i = 0; i < Lonicera.NodeCount; i++) {
                    cachePulse = Lonicera.Nodes[i];
                    if (cachePulse != null && cachePulse.Pitch.Frequency > newPulse.Pitch.Frequency) {
                        return i;
                    }
                }
                return -1;
            }
            
            int index = findIndex();
            if (index != -1) {Lonicera.Insert(index, new CosmosiaPulse(newPulse), true);}
            else {Lonicera.Add(new CosmosiaPulse(newPulse), true);} 
            return true;
        }
        public override bool DeletePulse(int pulseId) {
            int index = -1;
            CosmosiaPulse? cachePulse;
            for (int i = 0; i < Lonicera.NodeCount; i++) {
                cachePulse = Lonicera.Nodes[i];
                if (cachePulse != null && cachePulse.PulseID == pulseId) {
                    index = i;
                }
            }
            if (index == -1) {return false;}
            Lonicera.RemoveAt(index);
            return true;
        }
        public override bool MutatePulse(int oldId, Pulse newPulse) {
            int index = -1;
            CosmosiaPulse? cachePulse;
            for (int i = 0; i < Lonicera.NodeCount; i++) {
                cachePulse = Lonicera.Nodes[i];
                if (cachePulse != null && cachePulse.PulseID == oldId) {
                    index = i;
                }
            }
            if (index == -1) {return false;}
            Lonicera.MutateNode(index, new CosmosiaPulse(newPulse), true);
            return true;
        }       
        public override List<MagicalEffectData> GetMagicalEffects(byte intensityThreshold = 1) {
            var returnEffects = new List<MagicalEffectData>();
            foreach (CosmosiaChannel? channel in Lonicera.Links) {
                returnEffects.AddRange(channel.MagicEffect(ResonatorParameterId));
            }
            returnEffects.RemoveAll(effect => effect.Intensity < intensityThreshold);
            if (State == CosmosiaResonatorState.CriticalState || State == CosmosiaResonatorState.CriticalOverflow) {
                var parameter = Parameter;
                var intensity = parameter.CriticalEffectIntensity;
                var effectId = parameter.CriticalEffect;
                var criticalEffect = new MagicalEffectData(effectId, intensity);
                returnEffects.Add(criticalEffect);
            }
            return returnEffects;
        }
    
        public override void Process(double deltaTime) {
            // * Set Up
            var parameter = Parameter;

            // * Calculate
            var projNetInflow = Lonicera.ProjectedInflowRate(parameter, SizeMutiplier);
            var projOutflowRate = Lonicera.ProjectedOutflowRate(parameter, SizeMutiplier);
            var projOverflowRate = Lonicera.ProjectedOverflowRate(parameter, SizeMutiplier);
            
            var netInflow = Math.Clamp(projNetInflow, 0, parameter.InflowLimit); // Capped
            var netOutflow = Math.Clamp(projOutflowRate, 0, parameter.OutflowLimit); // Capped
            var netOverflow = 0.0f;

            var resonancePercentage = Resonance / parameter.MaxIdyllAmount; // Apply pressure lerp
            var pressureLerp = ResonanceHelperCosmosia.ResonancePressureLerp(resonancePercentage);
            netOutflow *= pressureLerp;
            
            // * Calculate Resonance
            Resonance += (float)(netInflow * deltaTime);
            Resonance -= (float)(netOutflow * deltaTime);

            // * Determine State and Adjust Outflow
            if (netInflow == 0 && netOutflow == 0) {
                State = CosmosiaResonatorState.Static;
                CriticalOverflowDuration = 0;
            }
            else if (Resonance <= 0) {
                State = CosmosiaResonatorState.Defecit;
                netOutflow += (float)(Resonance / deltaTime);
                Resonance = 0;
                CriticalOverflowDuration = 0;
            }
            else if (Resonance >= parameter.MaxIdyllAmount) {
                State = CosmosiaResonatorState.Overflow;
                netOverflow = (float)((Resonance - parameter.MaxIdyllAmount) / deltaTime);
                Resonance = parameter.MaxIdyllAmount;
                if (netOverflow >= parameter.OverflowLimit) {
                    netOverflow = parameter.OverflowLimit;
                    CriticalOverflowDuration += (int)(deltaTime * 1000);
                    if (CriticalOverflowDuration > parameter.CriticalEffectDurationThreshold) {
                        State = CosmosiaResonatorState.CriticalState;
                    }
                }
                else {CriticalOverflowDuration = 0;}
            }
            else {
                State = CosmosiaResonatorState.Active;
                CriticalOverflowDuration = 0;
            }
            // * Global NaN check
            if (float.IsNaN(Resonance)) {Resonance = 0;}
                       
            // * Update Channel Actual FlowRate
            Lonicera.AssignChannelFlowrate(projOutflowRate, projOverflowRate, netOutflow, netOverflow, SizeMutiplier, parameter);

            // * Check Pulse Inactivity
            Lonicera.CheckPulseInactivity(InactivityTolerance, InactivityThreshold, deltaTime);

        }
    }

    public class CosmosiaLonicera : Lonicera<CosmosiaPulse,CosmosiaChannel> {
        private CosmosiaPulse Origin;
        private static Func<CosmosiaPulse, CosmosiaPulse, CosmosiaChannel> _growthFunction = (pulse1, pulse2) => {
            return new CosmosiaChannel(pulse1, pulse2);
        };
        
        // * Constructor
        public CosmosiaLonicera(CosmosiaPulse origin) : base (
            _growthFunction,
            true
        ) {
            Origin = origin;
            Add(Origin);
            GrowAll();
        }

        // * Get Methods
        public int PulseIntensitySum { get { // [excluded origin] 
            int sum = 0;
            foreach (var node in _nodes) {
                sum += node.Intensity;
            }
            return sum - Origin.Intensity;
        } }
        public int ChannelIntensitySum { get { 
            int sum = 0;
            foreach (var link in _links) {
                sum += link.Intensity;
            }
            return sum;
        } }

        public float IntensityRatio { get { // link / node [exclude origin]
            return ChannelIntensitySum / PulseIntensitySum;
        } }

        public static float LinkNodeRatio(int pulseCount) {
            if (pulseCount == 0) {return 0;}
            return (float)NodesToLinkIndex(0, pulseCount) / (float)pulseCount;
        }
        public float LinkNodeRatio() {
            if (NodeCount == 0) {return 0;}
            return (float)LinkCount / (float)NodeCount;
        }
        
        // * Process Methods - (parameter, sizeMultiplier)

        public float NetOutflowRate() {
            float netFlow = 0;
            foreach (CosmosiaChannel? link in _links) {
                netFlow += link.OutflowRate;
            }
            return netFlow;
        }
        public float NetOverflowRate() {
            float netFlow = 0;
            foreach (CosmosiaChannel? link in _links) {
                netFlow += link.OverflowRate;
            }
            return netFlow;
        }

        public float ProjectedInflowRate(ResonatorParameterCosmosia parameter, float sizeMultiplier) {
            float linkNodeRatio = LinkNodeRatio();
            if (linkNodeRatio == 0) {return 0;}
            float netFlow = 0;
            foreach (CosmosiaChannel? channel in _links) { 
                var inflowMultiplier = parameter.GetChannelParameter((byte)channel.ChannelId).InflowMultiplier;
                netFlow += channel.ScaledInflowRate(sizeMultiplier * inflowMultiplier / linkNodeRatio);
            }
            return netFlow; // Not capped
        }
        public float ProjectedOutflowRate(ResonatorParameterCosmosia parameter, float sizeMultiplier) {
            float linkNodeRatio = LinkNodeRatio();
            if (linkNodeRatio == 0) {return 0;}
            float netFlow = 0;
            foreach (CosmosiaChannel? channel in _links) {
                var outflowMultiplier = parameter.GetChannelParameter((byte)channel.ChannelId).OutflowMultiplier;
                netFlow += channel.ScaledOutflowRate(sizeMultiplier * outflowMultiplier / linkNodeRatio);
            }
            return netFlow; // Not capped
        }
        public float ProjectedOverflowRate(ResonatorParameterCosmosia parameter, float sizeMultiplier) {
            float linkNodeRatio = LinkNodeRatio();
            if (linkNodeRatio == 0) {return 0;}
            float netFlow = 0;
            foreach (CosmosiaChannel? channel in _links) {
                var overflowMultiplier = parameter.GetChannelParameter(channel.ChannelId).OverflowMultiplier;
                netFlow += channel.ScaledOverflowRate(sizeMultiplier * overflowMultiplier / linkNodeRatio);
            }
            return netFlow; // Not capped
        }
        
        public void AssignChannelFlowrate(float projOutflow, float projOverflow, float netOutflowRate, float netOverflowRate, float sizeMultiplier, ResonatorParameterCosmosia parameter) {
            var linkNodeRatio = LinkNodeRatio();
            if (linkNodeRatio == 0) {return;} // no channel links to assign to
            var outflowScaleMultiplier = sizeMultiplier * netOutflowRate / linkNodeRatio / Math.Clamp(projOutflow, 1, float.MaxValue);
            var overflowScaleMultiplier = sizeMultiplier * netOverflowRate / linkNodeRatio / Math.Clamp(projOverflow, 1, float.MaxValue);
            for (int i = 0; i < LinkCount; i++) {
                var channel = parameter.GetChannelParameter((byte)_links[i].ChannelId);
                _links[i].OutflowRate = _links[i].ScaledOutflowRate(outflowScaleMultiplier * channel.OutflowMultiplier);
                _links[i].OverflowRate = _links[i].ScaledOverflowRate(overflowScaleMultiplier * channel.OverflowMultiplier);
            }
        
        }
    
        public void CheckPulseInactivity(int tolerance, byte threshHold, double deltaTime) { // ! NOT DONE
            
        }
    }
    
    public class CosmosiaPulse : Pulse {
        public int InactivityDuration = 0;
        public bool IsOrigin;
        public CosmosiaPulse(Pitch pitch, byte intensity, bool isOrigin = false) : base(pitch, intensity) {IsOrigin = isOrigin;}
        public CosmosiaPulse(Pulse pulse, bool isOrigin = false) : base(pulse.Pitch, pulse.Intensity) {IsOrigin = isOrigin; PulseID = pulse.PulseID;}
    }
    
    public class CosmosiaChannel {
        // * Core Channel Constants
        public PitchInterval Interval;
        public bool IsN2R;
        public bool IsNull;
        
        // * Derived Gets
        public bool IsN2N { get {
            return !IsN2R;
        } }
        public CosmosiaChannelId ChannelId { get {
            return ResonanceHelperCosmosia.IntervalToChannelID(Interval, IsN2R);
        } }
        public List<MagicalEffectData> MagicEffect(int parameterId){
            var effects = new List<MagicalEffectData>();
            var medDuo = ResonanceHelperCosmosia.IntervalToMEDDuo(Interval, IsN2R, 0, parameterId);
            if (medDuo.Outflow != null) {medDuo.Outflow.Intensity = OutflowIntensity; effects.Add(medDuo.Outflow);}
            if (medDuo.Overflow != null) {medDuo.Overflow.Intensity = OverflowIntensity; effects.Add(medDuo.Overflow);}
            return effects;
        }

        public float RawInflowRate { get {
            return (float)Math.Pow(Intensity/16, 2); 
        } }
        public float RawOutflowRate { get {
            return (float)Math.Pow(Intensity/16, 2);
        } }
        public float RawOverflowRate { get {
            return (float)Math.Pow(Intensity/16, 2);
        } }
        
        public float ScaledInflowRate(float multiplier = 1) {
            return RawInflowRate * multiplier;
        }
        public float ScaledOutflowRate(float multiplier = 1) {
            return RawOutflowRate * multiplier;
        }
        public float ScaledOverflowRate(float multiplier = 1) {
            return RawOverflowRate * multiplier;
        }

        public byte OutflowIntensity { get {
            return ResonanceHelperCosmosia.FlowrateToIntensity(OutflowRate);
        } }
        public byte OverflowIntensity { get {
            return ResonanceHelperCosmosia.FlowrateToIntensity(OverflowRate);
        } }

        // * Class Variables
        public byte Intensity; // Derived Input Value
        public float OutflowRate; // Finalised Value
        public float OverflowRate; // Finalised Value
        

        // * Constructor
        public CosmosiaChannel(Pulse pulse1, Pulse pulse2, bool isN2R) { // standard constructor
            IsN2R = isN2R;
            Interval = PitchInterval.CreateInterval(pulse1.Pitch, pulse2.Pitch);
            Intensity = ResonanceHelperCosmosia.CalculateIntervalIntensity(pulse1.Intensity, pulse2.Intensity);
            OutflowRate = 0;
            OverflowRate = 0;
        }
        public CosmosiaChannel(CosmosiaPulse pulse1, CosmosiaPulse pulse2) { // growth constructor
            IsN2R = pulse1.IsOrigin;
            Interval = PitchInterval.CreateInterval(pulse1.Pitch, pulse2.Pitch);
            Intensity = ResonanceHelperCosmosia.CalculateIntervalIntensity(pulse1.Intensity, pulse2.Intensity);
            OutflowRate = 0;
            OverflowRate = 0;
        }
   
    }

    public class MEDDuo {
        private bool isNull;
        public MagicalEffectData? Outflow;
        public MagicalEffectData? Overflow;
        public MEDDuo (MagicalEffectData outflow, MagicalEffectData overflow)
        {
            isNull = false;
            Outflow = outflow;
            Overflow = overflow;
        }
        public MEDDuo() {
            isNull = true;
        }

        public void UpdateIntensity(byte intensity){
            if (!isNull) {
                if (Outflow != null) {Outflow.Intensity = intensity;}
                if (Overflow != null) {Overflow.Intensity = intensity;}
            }
        }
    }
}