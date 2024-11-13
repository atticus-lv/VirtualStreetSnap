using System.Linq;

namespace  VirtualStreetSnap.Models;

public class SizeRadio
{
    public SizeRadio(string label)
    {
        Label = label;
    }

    // this is a class that is used to store the size of the camera like 16:9, 4:3, 1:1
    public string Label { get; set; }
    
    public int GetWidth(int height)
    {
        // split by : and get the elements and convert it to int
        // then multiply the height by the first element and divide it by the second element

        var ratio = Label.Split(":").Select(int.Parse).ToArray();
        return height * ratio[0] / ratio[1];
    }
    
    public int GetHeight(int width)
    {
        var ratio = Label.Split(":").Select(int.Parse).ToArray();
        return width * ratio[1] / ratio[0];
    }
}