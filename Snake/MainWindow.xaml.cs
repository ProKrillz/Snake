using Snake.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake;

/// <summary>
/// Interaction logic for SnakeWindow.xaml
/// </summary>
public partial class SnakeWindow : Window
{
    public SnakeWindow()
    {
        InitializeComponent();
    }
    const int SnakeSquareSize = 20;
    private SolidColorBrush _snakeBodyBrush = Brushes.Green;
    private SolidColorBrush _snakeHeadBrush = Brushes.YellowGreen;
    private List<SnakePart> _snakeParts = new List<SnakePart>();
    public enum SnakeDirection { Left, Right, Up, Down }
    private SnakeDirection snakeDirection = SnakeDirection.Right;
    private int snakeLength;
    private void Window_ContentRendered(object sender, EventArgs e) => DrawGameArea();
    private void DrawGameArea()
    {
        bool doneDrawingBackground = false, nextIsOdd = false;
        int nextX = 0, nextY = 0, rowCounter = 0;

        while (doneDrawingBackground == false)
        {
            Rectangle rec = new Rectangle {
                Width = SnakeSquareSize, 
                Height = SnakeSquareSize, 
                Fill = nextIsOdd ? Brushes.White : Brushes.Aquamarine 
            };
            GameArea.Children.Add(rec);
            Canvas.SetTop(rec, nextY);
            Canvas.SetLeft(rec, nextX);

            nextIsOdd = !nextIsOdd;
            nextX += SnakeSquareSize;
            if (nextX >= GameArea.ActualWidth)
            {
                nextX = 0;
                nextY += SnakeSquareSize;
                rowCounter++;
                nextIsOdd = (rowCounter % 2 != 0);
            }
            if (nextY >= GameArea.ActualHeight)
                doneDrawingBackground = true;
        }
    }
    private void DrawSnake()
    {
        foreach (SnakePart snakePart in _snakeParts)
        {
            if (snakePart.UiElement == null)
            {
                snakePart.UiElement = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = (snakePart.IsHead ? _snakeHeadBrush : _snakeBodyBrush)
                };
                GameArea.Children.Add(snakePart.UiElement);
                Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
            }
        }
    }
}
