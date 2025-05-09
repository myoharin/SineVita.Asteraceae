// magic effects class parameters are used to balance mana intensity input with each other
// aka the same intensity in two different magical effect shoudl produce energy in simular quantity
// that does not mean the ability needs to be balanced, just that some thermodynamics is conserved

// when resonator inevitable applify the energy in without the appropriate lost, there would be a system to handle that.

// it is a backend class that helps bridge and connect to the Peppermint magic effect manifestation implementation.

namespace SineVita.Muguet.Asteraceae {
    public class MagicalEffectData {
        public int MagicEffectID { get; set; }
        public byte Intensity { get; set; } 
        public Dictionary<AsterArgumentType, float> Arguments;
        public MagicalEffectData(int magicalEffectID, byte intensity, Dictionary<AsterArgumentType, float>? arguments = null) {
            MagicEffectID = magicalEffectID; // -1 meanis its a null value
            Intensity = intensity;
            Arguments = arguments ?? new Dictionary<AsterArgumentType, float>();
        }
    }

    public enum MagicEffectId {
        None = 0,
    }
    
    public enum AsterArgumentType {
        // * Cosmosia Variants
        COS_N2R_DEGREE,
        COS_N2N_TYPE,
        COS_N2N_GRADE
    }
}