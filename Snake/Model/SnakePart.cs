using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Snake.Model;

public class SnakePart
{
    public UIElement UiElement { get; set; }
    public Point Position { get; set; }
    public bool IsHead { get; set; }
}
public class SnakeHighscore
{
    public string PlayerName { get; set; }

    public int Score { get; set; }
}
