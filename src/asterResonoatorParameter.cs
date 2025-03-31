namespace SineVita.Asteraceae {
    public abstract class ResonatorParameter {
        // Meta data
        public AsterGenus Genus { get; set; }
        protected ResonatorParameter(AsterGenus genus) {
            Genus = genus;
        }
    }
}