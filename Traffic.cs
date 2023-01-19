using Avalonia.Media;

namespace NetGraph;

public struct Traffic
{
   public double Overlap { get; set; }
   public double Difference { get; set; }
   public SolidColorBrush DifferenceColour { get; set; }
}