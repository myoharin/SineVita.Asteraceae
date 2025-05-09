namespace SineVita.Muguet.Asteraceae.Cosmosia
{
    public enum CosmosiaChannelId
    {
        N2N_Octave = 0,
        N2N_Second = 1,
        N2N_Diminished = 2,
        N2N_Augmented = 3,
        N2N_Fourth = 4,
        N2N_Fifth = 5,
        N2N_Tritone = 6,
        N2N_Seventh = 7,
        N2R_Octave = 8,
        N2R_Fifth = 9,
        N2R_Fourth = 10,
        N2R_FF = 11,
        N2R_FifthOctave = 12,
        N2R_FourthOctave = 13,
        Null = 255
    }
    public enum CosmosiaResonatorState
    {
        Static, // no inflow or outflow
        Active, // Normal Active
        Defecit, // active but overflow > inflow by a ratio
        Overflow,
        CriticalOverflow,
        CriticalState
    }
}