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
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace Snake;

/// <summary>
/// Interaction logic for SnakeWindow.xaml
/// </summary>
public partial class SnakeWindow : Window
{
    public SnakeWindow()
    {
        InitializeComponent();
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        gameTickTimer.Tick += GameTickTimer_Tick;
        LoadHighscoreList();
    }
    const int _snakeSquareSize = 20,
        _snakeStartLength = 3,
        _snakeStartSpeed = 400,
        _snakeSpeedThreshold = 100,
        _maxHighscoreListEntryCount = 5;
    private int _snakeLength, _currentScore = 0;
    private UIElement _snakeFood = null;
    private SolidColorBrush _foodBrush = Brushes.Red;
    private SolidColorBrush _snakeBodyBrush = Brushes.Green;
    private SolidColorBrush _snakeHeadBrush = Brushes.YellowGreen;
    private List<SnakePart> _snakeParts = new List<SnakePart>();
    private Random _rnd = new();
    private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
    public enum SnakeDirection { Left, Right, Up, Down }
    private SnakeDirection _snakeDirection = SnakeDirection.Right;
    public ObservableCollection<SnakeHighscore> HighscoreList
    {
        get; set;
    } = new ObservableCollection<SnakeHighscore>();
    private void Window_ContentRendered(object sender, EventArgs e)
    {
        DrawGameArea();
        //gameTickTimer.Tick += GameTickTimer_Tick;
        LoadHighscoreList();
    }
    private void DrawGameArea()
    {
        bool doneDrawingBackground = false, nextIsOdd = false;
        int nextX = 0, nextY = 0, rowCounter = 0;

        while (doneDrawingBackground == false)
        {
            Rectangle rec = new Rectangle {
                Width = _snakeSquareSize, 
                Height = _snakeSquareSize, 
                Fill = nextIsOdd ? Brushes.White : Brushes.Aquamarine 
            };
            GameArea.Children.Add(rec);
            Canvas.SetTop(rec, nextY);
            Canvas.SetLeft(rec, nextX);

            nextIsOdd = !nextIsOdd;
            nextX += _snakeSquareSize;
            if (nextX >= GameArea.ActualWidth)
            {
                nextX = 0;
                nextY += _snakeSquareSize;
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
                    Width = _snakeSquareSize,
                    Height = _snakeSquareSize,
                    Fill = (snakePart.IsHead ? _snakeHeadBrush : _snakeBodyBrush)
                };
                GameArea.Children.Add(snakePart.UiElement);
                Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
            }
        }
    }
    private void MoveSnake()
    {
        if (gameTickTimer.IsEnabled == true)
        {
            while (_snakeParts.Count >= _snakeLength)
            {
                GameArea.Children.Remove(_snakeParts[0].UiElement);
                _snakeParts.RemoveAt(0);

            }
            foreach (SnakePart snakePart in _snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = _snakeBodyBrush;
                snakePart.IsHead = false;
            }
            SnakePart snakeHead = _snakeParts[_snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (_snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= _snakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += _snakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= _snakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += _snakeSquareSize;
                    break;
                default:
                    break;
            }
            _snakeParts.Add(new SnakePart {
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            DrawSnake();
            DoCollisionCheck();
        }
    }
    private void GameTickTimer_Tick(object sender, EventArgs e) => MoveSnake();
    private void StartNewGame()
    {
        bdrWelcomeMessage.Visibility = Visibility.Collapsed;
        bdrNewHighscore.Visibility = Visibility.Collapsed;
        bdrHighscoreList.Visibility = Visibility.Collapsed;
        foreach (SnakePart snakeBodyPart in _snakeParts)
        {
            if (snakeBodyPart.UiElement != null)
                GameArea.Children.Remove(snakeBodyPart.UiElement);
        }
        _snakeParts.Clear();
        if (_snakeFood != null)
            GameArea.Children.Remove(_snakeFood);

        _currentScore = 0;
        _snakeLength = _snakeStartLength;
        _snakeDirection = SnakeDirection.Right;
        _snakeParts.Add(new SnakePart() { Position = new Point(_snakeSquareSize * 5, _snakeSquareSize * 5) });
        gameTickTimer.Interval = TimeSpan.FromMilliseconds(_snakeStartSpeed);
        DrawSnake();
        DrawSnakeFood();
        UpdateGameStatus();
        gameTickTimer.IsEnabled = true;
    }
    private Point GetNextFoodPosition()
    {
        int maxX = (int)(GameArea.ActualWidth / _snakeSquareSize);
        int maxY = (int)(GameArea.ActualHeight / _snakeSquareSize);
        int foodX =_rnd.Next(0, maxX) * _snakeSquareSize;
        int foodY =_rnd.Next(0, maxY) * _snakeSquareSize;
        foreach (SnakePart snakePart in _snakeParts)
        {
            if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                return GetNextFoodPosition();
        }
        return new Point(foodX, foodY);
    }
    private void DrawSnakeFood()
    {
        Point foodPosition = GetNextFoodPosition();
        _snakeFood = new Ellipse
        {
            Width = _snakeSquareSize,
            Height = _snakeSquareSize,
            Fill = _foodBrush
        };
        GameArea.Children.Add(_snakeFood);
        Canvas.SetTop(_snakeFood, foodPosition.Y);
        Canvas.SetLeft(_snakeFood, foodPosition.X);
    }
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        SnakeDirection originalSnakeDeirection = _snakeDirection;
        switch (e.Key)
        {
            case Key.Up:
                if (_snakeDirection != SnakeDirection.Down)
                    _snakeDirection = SnakeDirection.Up;
                break;
            case Key.Down:
                if (_snakeDirection != SnakeDirection.Up)
                    _snakeDirection = SnakeDirection.Down;
                break;
            case Key.Left:
                if (_snakeDirection != SnakeDirection.Right)
                    _snakeDirection = SnakeDirection.Left;
                break;
            case Key.Right:
                if (_snakeDirection != SnakeDirection.Left)
                    _snakeDirection= SnakeDirection.Right;
                break;
            case Key.Space:
                StartNewGame();
                break;
            default:
                break;
        }
        if (_snakeDirection != originalSnakeDeirection)
            MoveSnake();
    }
    private void DoCollisionCheck()
    {
        SnakePart snakeHead = _snakeParts[_snakeParts.Count - 1];

        if ((snakeHead.Position.X == Canvas.GetLeft(_snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(_snakeFood)))
        {
            EatSnakeFood();
            return;
        }
        if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) ||
        (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            EndGame();

        foreach (SnakePart snakeBodyPart in _snakeParts.Take(_snakeParts.Count - 1))
        {
            if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                EndGame();
        }
    }
    private void EatSnakeFood()
    {
        _snakeLength++;
        _currentScore++;
        int timerInterval = Math.Max(_snakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (_currentScore * 2));
        gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
        GameArea.Children.Remove(_snakeFood);
        DrawSnakeFood();
        UpdateGameStatus();
    }
    private void UpdateGameStatus()
    {
        this.tbStatusScore.Text = _currentScore.ToString();
        this.tbStatusSpeed.Text = gameTickTimer.Interval.TotalMilliseconds.ToString();
    }
    private void EndGame()
    {
        bool isNewHighscore = false;
        if (_currentScore > 0)
        {
            int lowestHighscore = (this.HighscoreList.Count > 0 ? this.HighscoreList.Min(x => x.Score) : 0);
            if ((_currentScore > lowestHighscore) || (this.HighscoreList.Count < _maxHighscoreListEntryCount))
            {
                bdrNewHighscore.Visibility = Visibility.Visible;
                txtPlayerName.Focus();
                isNewHighscore = true;
            }
        }
        if (!isNewHighscore)
        {
            tbFinalScore.Text = _currentScore.ToString();
            bdrEndOfGame.Visibility = Visibility.Visible;
        }
        gameTickTimer.IsEnabled = false;
    }
    private void Window_MouseDown(object sender, MouseButtonEventArgs e) => this.DragMove();
    private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();
    private void BtnShowHighscoreList_Click(object sender, RoutedEventArgs e)
    {
        bdrWelcomeMessage.Visibility = Visibility.Collapsed;
        bdrHighscoreList.Visibility = Visibility.Visible;
    }
    private void LoadHighscoreList()
    {
        if (File.Exists("snake_highscorelist.xml"))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<SnakeHighscore>));
            using (Stream reader = new FileStream("snake_highscorelist.xml", FileMode.Open))
            {
                List<SnakeHighscore> tempList = (List<SnakeHighscore>)serializer.Deserialize(reader);
                if (tempList != null)
                {
                    this.HighscoreList.Clear();
                    foreach (var item in tempList.OrderByDescending(x => x.Score))
                        this.HighscoreList.Add(item);
                }
            }
        }
    }
    private void SaveHighscoreList()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SnakeHighscore>));
        using (Stream writer = new FileStream("snake_highscorelist.xml", FileMode.Create))
        {
            serializer.Serialize(writer, this.HighscoreList);
        }
    }
    private void BtnAddToHighscoreList_Click(object sender, RoutedEventArgs e)
    {
        int newIndex = 0;
        if ((this.HighscoreList.Count > 0) && (_currentScore < this.HighscoreList.Max(x => x.Score)))
        {
            SnakeHighscore justAbove = this.HighscoreList.OrderByDescending(x => x.Score).First(x => x.Score >= _currentScore);
            if (justAbove != null)
                newIndex = this.HighscoreList.IndexOf(justAbove) + 1;
        }
        this.HighscoreList.Insert(newIndex, new SnakeHighscore()
        {
            PlayerName = txtPlayerName.Text,
            Score = _currentScore
        });
        while (this.HighscoreList.Count > _maxHighscoreListEntryCount)
            this.HighscoreList.RemoveAt(_maxHighscoreListEntryCount);

        SaveHighscoreList();

        bdrNewHighscore.Visibility = Visibility.Collapsed;
        bdrHighscoreList.Visibility = Visibility.Visible;
    }
}
