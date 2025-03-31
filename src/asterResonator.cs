using System.Collections.Generic;

namespace SineVita.Asteraceae {
    public abstract class Resonator{
        public AsterGenus Genus { get; set; }
        public Resonator(AsterGenus genus) {Genus = genus;}
        public virtual void Process(double deltaTime) {}
        public virtual bool AddPulse(Pulse newPulse) {return false;}
        public virtual bool DeletePulse(int pulseId) {return false;}
        public virtual bool MutatePulse(int oldId, Pulse newPulse) {return false;}
        public virtual List<MagicalEffectData> GetMagicalEffects(byte intensityThreshold = 1) {return new List<MagicalEffectData>();}
    }
}