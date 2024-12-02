using System.Linq;

namespace  VirtualStreetSnap.Models;

/// <summary>
/// This class is used to store the size of the camera like 16:9, 4:3, 1:1.
/// It provides methods to get the width and height based on the aspect ratio.
/// </summary>
public class SizeRatio
{
    public SizeRatio(string label)
    {
        Label = label;
    }

    
    public string Label { get; set; }
    
    /// <summary>
    /// Gets the width based on the height and the aspect ratio.
    /// </summary>
    /// <param name="height">The height to calculate the width from.</param>
    /// <returns>The calculated width.</returns>
    public int GetWidth(int height)
    {
        

        var ratio = Label.Split(":").Select(int.Parse).ToArray();
        return height * ratio[0] / ratio[1];
    }
    
    /// <summary>
    /// Gets the height based on the width and the aspect ratio.
    /// </summary>
    /// <param name="width">The width to calculate the height from.</param>
    /// <returns>The calculated height.</returns>
    public int GetHeight(int width)
    {
        var ratio = Label.Split(":").Select(int.Parse).ToArray();
        return width * ratio[1] / ratio[0];
    }
}