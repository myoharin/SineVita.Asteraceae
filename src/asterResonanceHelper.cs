using SineVita.Asteraceae.Cosmosia;
namespace SineVita.Asteraceae {
    public class ResonanceHelper{

        public static string? ParametersFolderPath = null;
        public static bool FolderPathSet { get {return ParametersFolderPath == null;} }
        protected const int DefaultTimeOutDeletionDuration = 32768; // ms

        // ! ALL THIS NEEDS TO BE FIXED TO GENERIC PARAMETERS

        // * Manual Parameter Cache Management
        protected static Dictionary<int, ResonatorParameter> ResonatorParamaters { get; set; } = new Dictionary<int, ResonatorParameter>();
        public static void ResonatorParamatersAddCache(int newResonatorParamaterID, bool autoDeletionTimer = false) { // ! GENERALIZE
            ResonatorParameterCosmosia newResonatorParameter = new ResonatorParameterCosmosia(newResonatorParamaterID);
            ResonatorParamaters.Add(newResonatorParamaterID, newResonatorParameter);
        }
        public static void ResonatorParamatersAddCache(string newResonatorParamaterPath, bool autoDeletionTimer = false) { // ! GENERALIZE
            ResonatorParameterCosmosia newResonatorParameter;
            try{
                newResonatorParameter = new ResonatorParameterCosmosia(newResonatorParamaterPath);
            }
            catch(Exception) {
                throw new FileNotFoundException("FileNotFound");
            }
            if (int.TryParse(newResonatorParamaterPath.Split("\\").Last().Split(".")[0], out int result))
            {
                int ID = result;
                ResonatorParamaters.Add(ID, newResonatorParameter);
            }
            else{
                throw new FileNotFoundException("IDnotFound");
            }
        }
                
        public static bool ResonatorParamatersDeleteCache(int deletionResonatorParamaterID) {
            return ResonatorParamaters.Remove(deletionResonatorParamaterID);
        }
        public static bool ResonatorParamatersDeleteCache(string deletionResonatorParamaterPath) {
            if (int.TryParse(deletionResonatorParamaterPath.Split("\\").Last().Split(".")[0], out int result))
            {
                int ID = result;
                return ResonatorParamaters.Remove(ID);
            }
            else{
                throw new FileNotFoundException("ParamaterIDNotSpecified");
            }
        }

        // * Safe Parameter Access
        public static ResonatorParameter GetResonatorParameter(int ResonatorParamaterID) {
            try {
                return ResonatorParamaters[ResonatorParamaterID];
            }
            catch (Exception) { // does not exist
                ResonatorParamatersAddCache(ResonatorParamaterID);
                return ResonatorParamaters[ResonatorParamaterID];
            }
        }
             
    }
}