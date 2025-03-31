using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using SineVita.Muguet;
using System.Text.Json;



namespace SineVita.Asteraceae.Cosmosia {
    public class ResonanceHelperCosmosia : ResonanceHelper // Muguet is the magic system it is currently operating under. Additional magic system can be added later.
    {
        // * Helper Constants
        private static List<int> _intervalIsStructual = new List<int>(){
            0, 5, 7, 10, 12, 14, 15, 17, 19, 20, 21, 22
        };

        // * Override
        public static new ResonatorParameterCosmosia GetResonatorParameter(int id) { // rough implmentation
            try {
                return (ResonatorParameterCosmosia)ResonatorParamaters[id];
            }
            catch (Exception) { // does not exist
                ResonatorParamatersAddCache(id);
                return (ResonatorParameterCosmosia)ResonatorParamaters[id];
            }
        
        }

        // * Mana Calculation functions
        public static byte CalculateIntervalIntensity(byte p1, byte p2) { // geometric mean
            int product = p1 * p2;
            int result = 0;
            int bit = 1 << 30;
            while (bit > product) bit >>= 2;
            while (bit != 0) {
                if (product >= result + bit) {
                    product -= result + bit;
                    result += bit << 1;
                }
                result >>= 1;
                bit >>= 2;
            }
            return (byte)result;
        }
        public static byte FlowrateToIntensity(float flowRate) { // max = 256
            return (byte)(Math.Pow(Math.Abs(flowRate), 1/2) * 16);
        }   
        public static float ResonancePressureLerp(float percentage) {
            float a = 1/12;
            return Math.Clamp((float)((Math.Pow(a, percentage) - 1)/(a - 1)),0,1);
        }
    
        // * From Interval Methods
        public static MEDDuo IntervalToMEDDuo(PitchInterval interval, bool isN2R, byte intensity, int resonatorParameterID){ // DONE - return list of possible channels(channelID, grade) or List(channelID, degree)
            int calculateDegree(int num, int Base) {return (int)Math.Floor(num / (float)Base);}
            int midiIndex = (int)MidiPitchInterval.ToIndex(interval);
            if (midiIndex < 0) {return new MEDDuo();}

            // working capital
            int outFlowEffectID;
            int overFlowEffectID;
            Tuple<byte, int, bool> returnTuple;
            
            if (isN2R) // byte channelID, int degree
            {
                bool isFifth = false;
                bool isForuth = false;
                bool isOctave = false;

                if (midiIndex % 12 == 0 && midiIndex > 0) {isOctave = true;}
                if (midiIndex % 7 == 0) {isFifth = true;}
                if (midiIndex % 5 == 0) {isForuth = true;}

                if (midiIndex == 0) {isFifth = false; isForuth = false;}

                if (!isFifth && !isForuth && isOctave) {returnTuple = new Tuple<byte, int, bool>(8, calculateDegree(midiIndex, 12), false);}
                else if (isFifth && !isForuth && !isOctave) {returnTuple = new Tuple<byte, int, bool>(9, calculateDegree(midiIndex, 7), false);}
                else if (!isFifth && isForuth && !isOctave) {returnTuple = new Tuple<byte, int, bool>(10, calculateDegree(midiIndex, 5), false);}
                else if (isFifth && isForuth && !isOctave) {returnTuple = new Tuple<byte, int, bool>(11, calculateDegree(midiIndex, 35), false);}
                else if (isFifth && !isForuth && isOctave) {returnTuple = new Tuple<byte, int, bool>(12, calculateDegree(midiIndex, 84), false);}
                else if (!isFifth && isForuth && isOctave) {returnTuple = new Tuple<byte, int, bool>(13, calculateDegree(midiIndex, 60), false);}
                else {return new MEDDuo();} 
                outFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OutflowEffect;
                overFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OverflowEffect;
                
                var arguments = new Dictionary<AsterArgumentType, float>() {{AsterArgumentType.COS_N2R_DEGREE, returnTuple.Item2}};
    
                MagicalEffectData outflow = new MagicalEffectData(outFlowEffectID, intensity, arguments);
                MagicalEffectData overflow = new MagicalEffectData(overFlowEffectID, intensity, arguments);

                return new MEDDuo(outflow, overflow);
            }
            else // byte channelID, int grade, bool type
            { // can only be one of them
                int grade = calculateDegree(midiIndex, 12);
                midiIndex = midiIndex % 12;
                

                if (midiIndex == 0 && grade >= 0) {returnTuple = new Tuple<byte, int, bool>(0, grade, false);}
                else if (midiIndex == 1) {returnTuple = new Tuple<byte, int, bool>(1, grade, false);}
                else if (midiIndex == 2) {returnTuple = new Tuple<byte, int, bool>(1, grade, true);}
                else if (midiIndex == 3) {returnTuple =new Tuple<byte, int, bool>(2, grade, false);}
                else if (midiIndex == 9) {returnTuple =new Tuple<byte, int, bool>(2, grade, true);}
                else if (midiIndex == 8) {returnTuple =new Tuple<byte, int, bool>(3, grade, false);}
                else if (midiIndex == 4) {returnTuple =new Tuple<byte, int, bool>(3, grade, true);}
                else if (midiIndex == 5) {returnTuple =new Tuple<byte, int, bool>(4, grade, false);}
                else if (midiIndex == 7) {returnTuple =new Tuple<byte, int, bool>(5, grade, false);}
                else if (midiIndex == 6) {returnTuple =new Tuple<byte, int, bool>(6, grade, false);}
                else if (midiIndex == 10) {returnTuple =new Tuple<byte, int, bool>(7, grade, true);}
                else if (midiIndex == 11) {returnTuple =new Tuple<byte, int, bool>(7, grade, false);}
                else {return new MEDDuo();}

                outFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OutflowEffect;
                overFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OverflowEffect;
                
                var arguments = new Dictionary<AsterArgumentType, float>() {
                    {AsterArgumentType.COS_N2N_GRADE, returnTuple.Item2},
                    {AsterArgumentType.COS_N2N_TYPE, returnTuple.Item3 ? 1 : 0}
                };
    
                MagicalEffectData outflow = new MagicalEffectData(outFlowEffectID, intensity, arguments);
                MagicalEffectData overflow = new MagicalEffectData(overFlowEffectID, intensity, arguments);
                return new MEDDuo(outflow, overflow);
            }
 
        }
        public static CosmosiaChannelId IntervalToChannelID(PitchInterval interval, bool isN2R) {
            int midiIndex = (int)MidiPitchInterval.ToIndex(interval);
            // data validation
            if (midiIndex < 0) {return CosmosiaChannelId.Null;}
            int calculateDegree(int num, int Base) {
            return (int)Math.Floor(num / (float)Base);
            }
            if (isN2R) // byte channelID
            {
            bool isFifth = false;
            bool isForuth = false;
            bool isOctave = false;
            
            if (midiIndex == 0) {return CosmosiaChannelId.N2R_Octave;}
            if (midiIndex % 12 == 0 && midiIndex > 0) {isOctave = true;}
            if (midiIndex % 7 == 0) {isFifth = true;}
            if (midiIndex % 5 == 0) {isForuth = true;}

            if (!isFifth && !isForuth && isOctave) {return CosmosiaChannelId.N2R_Octave;}
            else if (isFifth && !isForuth && !isOctave) {return CosmosiaChannelId.N2R_Fifth;}
            else if (!isFifth && isForuth && !isOctave) {return CosmosiaChannelId.N2R_Fourth;}
            else if (isFifth && isForuth && !isOctave) {return CosmosiaChannelId.N2R_FF;}
            else if (isFifth && !isForuth && isOctave) {return CosmosiaChannelId.N2R_FifthOctave;}
            else if (!isFifth && isForuth && isOctave) {return CosmosiaChannelId.N2R_FourthOctave;}

            else {return CosmosiaChannelId.Null;}
            }
            else // byte channelID
                { // can only be one of them
                midiIndex = midiIndex % 12;
                int grade = calculateDegree(midiIndex, 12);

                if (midiIndex == 0 && grade >= 0) {return CosmosiaChannelId.N2N_Octave;}
                else if (midiIndex == 1) {return CosmosiaChannelId.N2N_Second;}
                else if (midiIndex == 2) {return CosmosiaChannelId.N2N_Second;}
                else if (midiIndex == 3) {return CosmosiaChannelId.N2N_Diminished;}
                else if (midiIndex == 9) {return CosmosiaChannelId.N2N_Diminished;}
                else if (midiIndex == 8) {return CosmosiaChannelId.N2N_Augmented;}
                else if (midiIndex == 4) {return CosmosiaChannelId.N2N_Augmented;}
                else if (midiIndex == 5) {return CosmosiaChannelId.N2N_Fourth;}
                else if (midiIndex == 7) {return CosmosiaChannelId.N2N_Fifth;}
                else if (midiIndex == 6) {return CosmosiaChannelId.N2N_Tritone;}
                else if (midiIndex == 10) {return CosmosiaChannelId.N2N_Seventh;}
                else if (midiIndex == 11) {return CosmosiaChannelId.N2N_Seventh;}

                else {return CosmosiaChannelId.Null;}
            }
        }
        public static bool IntervalIsStructual(PitchInterval interval) {
            // convert
            int midiIndex = (int)MidiPitchInterval.ToIndex(interval);
            if (midiIndex >= 24) {return true;}
            return _intervalIsStructual.Contains(midiIndex);
        }
    
        public static ChannelParameterCosmosia GetCosmosiaChannelParameter(int resonatorParameterID, byte ChannelID) {
            if (ChannelID == 255) {return new ChannelParameterCosmosia();}
            try {return ((ResonatorParameterCosmosia)ResonatorParamaters[resonatorParameterID]).GetChannelParameter(ChannelID);}
            catch (Exception) { // does not exist
                try {
                    ResonatorParamatersAddCache(resonatorParameterID); 
                    return ((ResonatorParameterCosmosia)ResonatorParamaters[resonatorParameterID]).GetChannelParameter(ChannelID);
                }
                catch (Exception) {throw new Exception("Failed to get Cosmosia Channel Parameter");}
            } 
        }
       
    }


    
}