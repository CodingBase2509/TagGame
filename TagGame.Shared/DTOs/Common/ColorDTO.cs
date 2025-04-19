namespace TagGame.Shared.DTOs.Common;

public class ColorDTO
{
    public int A { get; set; } = 0;
    
    public int R { get; set; } = 0;
    
    public int G { get; set; } = 0;
    
    public int B { get; set; } = 0;

    public static ColorDTO FromArgb(int a, int r, int g, int b)
    {
        if (notInRange(a) || notInRange(r) || notInRange(g) || notInRange(b))
            throw new System.ArgumentException("Color must be in range [A, R, G, B].");
        
        return new ColorDTO { A = a, R = r, G = g, B = b };

        bool notInRange(int value) =>
            value is < 0 or > 255;
    }
}