using System.Collections.Generic;

namespace SineVita.Muguet.Asteraceae {
    public abstract class Resonator{
        public Genus Genus { get; set; }
        public Resonator(Genus genus) {Genus = genus;}
        public virtual void Process(double deltaTime) {}
        public virtual bool AddPulse(Pulse newPulse) {return false;}
        public virtual bool DeletePulse(int pulseId) {return false;}
        public virtual bool MutatePulse(int oldId, Pulse newPulse) {return false;}
        public virtual List<MagicalEffectData> GetMagicalEffects(byte intensityThreshold = 1) {return new List<MagicalEffectData>();}
    }
}