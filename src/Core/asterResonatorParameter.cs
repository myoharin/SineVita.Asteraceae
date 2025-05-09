namespace SineVita.Muguet.Asteraceae {
    public abstract class ResonatorParameter {
        // Meta data
        public Genus Genus { get; set; }
        protected ResonatorParameter(Genus genus) {
            Genus = genus;
        }
    }
}