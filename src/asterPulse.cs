using System;
using SineVita.Muguet;
namespace SineVita.Asteraceae {
    public class Pulse {
        public Pitch Pitch { get; set; }
        public byte Intensity { get; set; }
        public int PulseID { get; set;} // unique to pulses in Peppermint, and transfered and itemized here.
        public Pulse(Pitch pitch, byte intensity) {
            Pitch = pitch;
            Intensity = intensity;
            PulseID = new Random().Next(0, 0x1000000);
        }
    }
}
    