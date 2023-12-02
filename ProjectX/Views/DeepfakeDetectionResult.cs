namespace ProjectX.Views;
using Newtonsoft.Json;

public class DeepfakeDetectionResult
{
    public string code { get; set; }
    public bool is_live { get; set; }
    public bool is_deepfake { get; set; }
    public FaceMatch face_match { get; set; }
}

public class FaceMatch
{
    public bool isMatch { get; set; }
    public double similarity { get; set; }
}