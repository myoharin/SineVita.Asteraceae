using System;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;
using SineVita.Muguet;
namespace SineVita.Asteraceae.Cosmosia
{
    public class ResonatorParameterCosmosia : ResonatorParameter
    {
        // basic information
        public int ResonatorParameterId { get; set; }
        public Pitch Origin { get; set; }
        public byte OriginIntensity { get; set; }
        public float MaxIdyllAmount { get; set; } // max amount before the the mana starts going out the overflow limit
        public int CriticalEffect { get; set; }
        public int CriticalEffectDurationThreshold { get; set; } // in ms
        public byte CriticalEffectIntensity { get; set; }

        // limit on Idyllflow. If limit reached, then it will be distributed according to their intensity ratio
        public float InflowLimit { get; set; } // caping net inflow rate (pulse intensity)
        public float OutflowLimit { get; set; } // once limit reached, excess idyll flow gets stored (interval intensity)
        public float OverflowLimit { get; set; } // once limit reached, wait some time before critical effect (interval intensity)

        public List<ChannelParameterCosmosia> ChannelParameters{ get; set; }   // 13 items for now

        public ChannelParameterCosmosia GetChannelParameter(int channelID) {
            if (channelID >= 255 || channelID >= ChannelParameters.Count) {
                return new ChannelParameterCosmosia();
            }
            if (ChannelParameters[channelID] == null) {
                return new ChannelParameterCosmosia();
            }
            return ChannelParameters[channelID];
        }
        public ChannelParameterCosmosia GetChannelParameter(CosmosiaChannelId channelId) {
            return GetChannelParameter((byte)channelId);
        }

        // * Derived Constructors
        public ResonatorParameterCosmosia(int resonatorParameterID)
            : this(Path.Combine(ResonanceHelper.ParametersFolderPath ?? "", "cosmosia" ,$"{resonatorParameterID}.json")) {}
        public ResonatorParameterCosmosia(string paramaterPath) : base(AsterGenus.Cosmosia) {
            if (int.TryParse(paramaterPath.Split("\\").Last().Split(".")[0], out int result)) {
                ResonatorParameterId = result;
            } else {throw new FileNotFoundException("ParamaterIDNotSpecified");}

            if (!File.Exists(paramaterPath)) {throw new FileNotFoundException($"The specified JSON file was not found: {paramaterPath }");}
            
            string jsonString = File.ReadAllText(paramaterPath);
            var resonatorParameter = FromJson(jsonString);
            ResonatorParameterId = resonatorParameter.ResonatorParameterId;
            Origin = resonatorParameter.Origin ?? new MidiPitch(69);
            OriginIntensity = resonatorParameter.OriginIntensity;
            MaxIdyllAmount = resonatorParameter.MaxIdyllAmount;
            CriticalEffect = resonatorParameter.CriticalEffect;
            InflowLimit = resonatorParameter.InflowLimit;
            OutflowLimit = resonatorParameter.OutflowLimit;
            OverflowLimit = resonatorParameter.OverflowLimit;
            ChannelParameters = resonatorParameter.ChannelParameters ?? new List<ChannelParameterCosmosia>();
        }
    
        // * FromJson
        private ResonatorParameterCosmosia() : base(AsterGenus.Cosmosia) {Origin = new MidiPitch(69); ChannelParameters = new List<ChannelParameterCosmosia>();}
        public static ResonatorParameterCosmosia FromJson(string jsonString) {
            var jsonDocument = JsonDocument.Parse(jsonString);
            var rootElement = jsonDocument.RootElement;
            var returnParameter = new ResonatorParameterCosmosia();

            returnParameter.ResonatorParameterId = rootElement.GetProperty("ResonatorParameterId").GetInt32();
            
            returnParameter.OriginIntensity = rootElement.GetProperty("OriginIntensity").GetByte();
            returnParameter.MaxIdyllAmount = rootElement.GetProperty("MaxIdyllAmount").GetSingle();
            returnParameter.CriticalEffect = rootElement.GetProperty("CriticalEffect").GetInt32();
            returnParameter.CriticalEffectDurationThreshold = rootElement.GetProperty("CriticalEffectDurationThreshold").GetInt32();
            returnParameter.CriticalEffectIntensity = rootElement.GetProperty("CriticalEffectIntensity").GetByte();

            returnParameter.InflowLimit = rootElement.GetProperty("InflowLimit").GetSingle();
            returnParameter.OutflowLimit = rootElement.GetProperty("OutflowLimit").GetSingle();
            returnParameter.OverflowLimit = rootElement.GetProperty("OverflowLimit").GetSingle();

            // * Origin Pitch
            string? originJsonString = rootElement.GetProperty("Origin").ToString();
            if (originJsonString != null) {
                returnParameter.Origin = Pitch.FromJson(originJsonString);
            }
            else {
                returnParameter.Origin = Pitch.Empty;
            }

            // * Channel Parameters
            var channelParametersElement = rootElement.GetProperty("ChannelParameters");
            for (int i = 0; i < channelParametersElement.GetArrayLength(); i++) {
                var parameterJsonString = channelParametersElement[i].ToString();
                if (parameterJsonString == null) {throw new JsonException("Channel Parameter is null");}
                returnParameter.ChannelParameters.Add(ChannelParameterCosmosia.FromJson(parameterJsonString));
            }
            return returnParameter;
        }
    }
    
    public class ChannelParameterCosmosia {
        public CosmosiaChannelId ChannelId { get; set; }
        public float InflowMultiplier { get; set; }
        public float OutflowMultiplier { get; set; }
        public float OverflowMultiplier { get; set; }
        public int InflowEffect { get; set; }
        public int OutflowEffect { get; set; }
        public int OverflowEffect { get; set; }
        public bool IsNull { get; set; }

        // * Constructor
        public ChannelParameterCosmosia(CosmosiaChannelId channelId, float inflowMultiplier, float outflowMultiplier, float overflowMultiplier, int inflowEffect, int outflowEffect, int overflowEffect) {
            ChannelId = channelId;
            InflowMultiplier = inflowMultiplier;
            OutflowMultiplier = outflowMultiplier;
            OverflowMultiplier = overflowMultiplier;
            InflowEffect = inflowEffect;
            OutflowEffect = outflowEffect;
            OverflowEffect = overflowEffect;
            IsNull = false;
        }
        
        // * FromJson
        public ChannelParameterCosmosia() : this((CosmosiaChannelId)255, 0, 0, 0, -1, -1, -1) {IsNull = true;}
        public static ChannelParameterCosmosia FromJson(string jsonString) {
            var jsonDocument = JsonDocument.Parse(jsonString);
            var rootElement = jsonDocument.RootElement;
            var returnChannel = new ChannelParameterCosmosia();

            returnChannel.ChannelId = (CosmosiaChannelId)rootElement.GetProperty("ChannelId").GetInt32();
            returnChannel.InflowMultiplier = rootElement.GetProperty("InflowMultiplier").GetSingle();
            returnChannel.OutflowMultiplier = rootElement.GetProperty("OutflowMultiplier").GetSingle();
            returnChannel.OverflowMultiplier = rootElement.GetProperty("OverflowMultiplier").GetSingle();
            returnChannel.InflowEffect = rootElement.GetProperty("InflowEffect").GetInt32();
            returnChannel.OutflowEffect = rootElement.GetProperty("OutflowEffect").GetInt32();
            returnChannel.OverflowEffect = rootElement.GetProperty("OverflowEffect").GetInt32();
            returnChannel.IsNull = false;
            
            return returnChannel;
        }
    }

}