using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace WindowsFormsApp12
{
    public partial class Form1 : Form
    {
        private List<SerializedCircleShape> circles;
        private Timer timer;
        private bool isRunning;
        private bool isFullScreen;
        private bool isMenuVisible;
        private bool collisionOccurred;
        private const double acceleration = 0.1;
        private const int initialSpeed = 5;
        private const double bounceFactor = 0.9;
        private string saveDirectory = Path.Combine(Application.StartupPath, "Saves");
        private string quickSavePath = Path.Combine(Application.StartupPath, "Saves", "quicksave.gamesave");

        private CustomMenu gameMenu;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            InitializeGame();
            InitializeGameMenu();
        }

        private void InitializeGame()
        {
            circles = new List<SerializedCircleShape>();
            PopulateCircles();

            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += (sender, e) => UpdateGame();
            this.Paint += (sender, e) => Draw(e.Graphics);
            this.KeyDown += Form1_KeyDown;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
            isFullScreen = false;
            isRunning = true;
            collisionOccurred = false;
            this.Resize += Form1_Resize;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // При изменении размера окна корректируем положение кругов, чтобы они не выходили за границы
            foreach (var circle in circles)
            {
                circle.Position = new Point(
                    Math.Min(Math.Max(circle.Position.X, 0),
                    Math.Min(Math.Max(circle.Position.Y, 0)
                );
            }
            Invalidate();
        }

        private void InitializeGameMenu()
        {
            gameMenu = new CustomMenu
            {
                Owner = this
            };

            gameMenu.ContinueGame += (s, e) =>
            {
                gameMenu.Hide();
                isMenuVisible = false;
                if (!timer.Enabled)
                {
                    timer.Start();
                }
            };

            gameMenu.SaveGame += (s, e) => SaveGame();
            gameMenu.LoadGame += (s, e) => LoadGame();
            gameMenu.OpenSettings += (s, e) => OpenSettings();
            gameMenu.ExitGame += (s, e) => EndGame();

            gameMenu.CenterInParent();
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void StartGame()
        {
            isRunning = true;
            timer.Start();
            gameMenu.Hide();
        }

        private void EndGame()
        {
            isRunning = false;
            timer.Stop();
            this.Close();
        }

        private void SaveGame()
        {
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.InitialDirectory = saveDirectory;
                saveDialog.Filter = "Game Save (*.gamesave)|*.gamesave";
                saveDialog.DefaultExt = "gamesave";
                saveDialog.AddExtension = true;
                saveDialog.Title = "Save Game";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var state = new GameState
                        {
                            Circles = circles,
                            SaveTime = DateTime.Now,
                            SaveName = Path.GetFileNameWithoutExtension(saveDialog.FileName)
                        };

                        string json = JsonConvert.SerializeObject(state, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            });

                        File.WriteAllText(saveDialog.FileName, json);
                        MessageBox.Show("Game saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void QuickSaveGame()
        {
            try
            {
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                var state = new GameState
                {
                    Circles = circles,
                    SaveTime = DateTime.Now,
                    SaveName = "QUICKSAVE"
                };

                string json = JsonConvert.SerializeObject(state, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                File.WriteAllText(quickSavePath, json);
                MessageBox.Show("Quick save created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during quick save: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGame()
        {
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.InitialDirectory = saveDirectory;
                openDialog.Filter = "Game Save (*.gamesave)|*.gamesave";
                openDialog.DefaultExt = "gamesave";
                openDialog.Title = "Load Game";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string json = File.ReadAllText(openDialog.FileName);
                        var state = JsonConvert.DeserializeObject<GameState>(json,
                            new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            });

                        if (state != null && state.Circles != null)
                        {
                            timer.Stop();
                            circles = state.Circles;
                            Invalidate();
                            MessageBox.Show($"Game loaded successfully!\nSaved at: {state.SaveTime}",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            timer.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void QuickLoadGame()
        {
            try
            {
                if (!File.Exists(quickSavePath))
                {
                    MessageBox.Show("No quick save found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string json = File.ReadAllText(quickSavePath);
                var state = JsonConvert.DeserializeObject<GameState>(json,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                if (state != null && state.Circles != null)
                {
                    timer.Stop();
                    circles = state.Circles;
                    Invalidate();
                    MessageBox.Show($"Quick load complete!\nSaved at: {state.SaveTime}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during quick load: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSettings()
        {
            MessageBox.Show("Game Settings", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Draw(Graphics g)
        {
            g.Clear(Color.Black);
            foreach (var circle in circles)
            {
                circle.Draw(g);
            }
            using (Pen borderPen = new Pen(Color.White, 5))
            {
                g.DrawRectangle(borderPen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }

            if (collisionOccurred)
            {
                using (Font font = new Font("Arial", 24))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    string message = "Collision! Press SPACE to continue";
                    SizeF textSize = g.MeasureString(message, font);
                    g.DrawString(message, font, brush,
                        (ClientSize.Width - textSize.Width) / 2,
                        (ClientSize.Height - textSize.Height) / 2);
                }
            }
        }

        private void UpdateGame()
        {
            if (isMenuVisible) return;

            if (CheckCollisions())
            {
                if (isRunning)
                {
                    isRunning = false;
                    collisionOccurred = true;
                    timer.Stop();
                    Invalidate();
                }
                return;
            }

            if (isRunning)
            {
                foreach (var circle in circles)
                {
                    circle.Update(acceleration);
                    CheckBounds(circle);
                }
            }

            Invalidate();
        }

        private bool CheckCollisions()
        {
            for (int i = 0; i < circles.Count; i++)
            {
                for (int j = i + 1; j < circles.Count; j++)
                {
                    if (AreCirclesColliding(circles[i], circles[j]))
                    {
                        ResolveCollision(circles[i], circles[j]);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool AreCirclesColliding(SerializedCircleShape circle1, SerializedCircleShape circle2)
        {
            Point center1 = new Point(
                circle1.Position.X + circle1.Size.Width / 2,
                circle1.Position.Y + circle1.Size.Height / 2
            );
            Point center2 = new Point(
                circle2.Position.X + circle2.Size.Width / 2,
                circle2.Position.Y + circle2.Size.Height / 2
            );

            int dx = center1.X - center2.X;
            int dy = center1.Y - center2.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            double radius1 = circle1.Size.Width / 2;
            double radius2 = circle2.Size.Width / 2;

            return distance <= (radius1 + radius2);
        }

        private void ResolveCollision(SerializedCircleShape circle1, SerializedCircleShape circle2)
        {
            Point center1 = new Point(
                circle1.Position.X + circle1.Size.Width / 2,
                circle1.Position.Y + circle1.Size.Height / 2
            );
            Point center2 = new Point(
                circle2.Position.X + circle2.Size.Width / 2,
                circle2.Position.Y + circle2.Size.Height / 2
            );

            double dx = center2.X - center1.X;
            double dy = center2.Y - center1.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            double nx = dx / distance;
            double ny = dy / distance;

            double dvx = circle2.Velocity.X - circle1.Velocity.X;
            double dvy = circle2.Velocity.Y - circle1.Velocity.Y;

            double impulse = 2 * (dvx * nx + dvy * ny) / (1 + 1);

            circle1.Velocity = new Vector(
                circle1.Velocity.X + impulse * nx * bounceFactor,
                circle1.Velocity.Y + impulse * ny * bounceFactor
            );
            circle2.Velocity = new Vector(
                circle2.Velocity.X - impulse * nx * bounceFactor,
                circle2.Velocity.Y - impulse * ny * bounceFactor
            );

            double overlap = (circle1.Size.Width / 2 + circle2.Size.Width / 2) - distance;
            circle1.Position = new Point(
                circle1.Position.X - (int)(overlap * nx / 2),
                circle1.Position.Y - (int)(overlap * ny / 2)
            );
            circle2.Position = new Point(
                circle2.Position.X + (int)(overlap * nx / 2),
                circle2.Position.Y + (int)(overlap * ny / 2)
            );
        }

        private void CheckBounds(SerializedCircleShape circle)
        {
            if (circle.Position.X < 0 || circle.Position.X + circle.Size.Width > ClientSize.Width)
            {
                circle.Velocity = new Vector(-circle.Velocity.X * bounceFactor, circle.Velocity.Y);
                circle.Position = new Point(
                    Math.Max(0, Math.Min(ClientSize.Width - circle.Size.Width, circle.Position.X)),
                    circle.Position.Y
                );
            }
            if (circle.Position.Y < 0 || circle.Position.Y + circle.Size.Height > ClientSize.Height)
            {
                circle.Velocity = new Vector(circle.Velocity.X, -circle.Velocity.Y * bounceFactor);
                circle.Position = new Point(
                    circle.Position.X,
                    Math.Max(0, Math.Min(ClientSize.Height - circle.Size.Height, circle.Position.Y))
                );
            }
        }

        private void PopulateCircles()
        {
            Random rand = new Random();
            int maxAttempts = 100;
            int circlesCount = 10;
            int minDistance = 50;

            for (int i = 0; i < circlesCount; i++)
            {
                int attempts = 0;
                bool positionFound = false;
                SerializedCircleShape newCircle = null;

                while (!positionFound && attempts < maxAttempts)
                {
                    attempts++;
                    newCircle = new SerializedCircleShape
                    {
                        Size = new Size(rand.Next(30, 70), rand.Next(30, 70)),
                        Color = Color.FromArgb(rand.Next(150, 255), rand.Next(150, 255), rand.Next(150, 255)),
                        Velocity = new Vector(rand.Next(-3, 3), rand.Next(-3, 3)),
                        Accelerating = false
                    };

                    Point position = new Point(
                        rand.Next(0, ClientSize.Width - newCircle.Size.Width),
                        rand.Next(0, ClientSize.Height - newCircle.Size.Height)
                    );

                    newCircle.Position = position;

                    bool intersects = false;
                    foreach (var existingCircle in circles)
                    {
                        if (AreCirclesTooClose(newCircle, existingCircle, minDistance))
                        {
                            intersects = true;
                            break;
                        }
                    }

                    if (!intersects)
                    {
                        positionFound = true;
                    }
                }

                if (positionFound)
                {
                    circles.Add(newCircle);
                }
            }
        }

        private bool AreCirclesTooClose(SerializedCircleShape circle1, SerializedCircleShape circle2, int minDistance)
        {
            Point center1 = new Point(
                circle1.Position.X + circle1.Size.Width / 2,
                circle1.Position.Y + circle1.Size.Height / 2
            );
            Point center2 = new Point(
                circle2.Position.X + circle2.Size.Width / 2,
                circle2.Position.Y + circle2.Size.Height / 2
            );

            int dx = center1.X - center2.X;
            int dy = center1.Y - center2.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            double radius1 = circle1.Size.Width / 2;
            double radius2 = circle2.Size.Width / 2;

            return distance < (radius1 + radius2 + minDistance);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.M)
            {
                isMenuVisible = !isMenuVisible;
                if (isMenuVisible)
                {
                    timer.Stop();
                    gameMenu.Show();
                }
                else
                {
                    gameMenu.Hide();
                    timer.Start();
                }
            }
            else if (e.KeyCode == Keys.Space)
            {
                if (collisionOccurred)
                {
                    collisionOccurred = false;
                    isRunning = true;
                    timer.Start();
                }
                else
                {
                    isRunning = !isRunning;
                    if (isRunning)
                        timer.Start();
                    else
                        timer.Stop();
                }
            }
            else if (e.KeyCode == Keys.W)
            {
                foreach (var circle in circles)
                {
                    circle.Accelerating = true;
                }
            }
            else if (e.KeyCode == Keys.S)
            {
                foreach (var circle in circles)
                {
                    circle.Velocity = new Vector(initialSpeed * Math.Sign(circle.Velocity.X), initialSpeed * Math.Sign(circle.Velocity.Y));
                    circle.Accelerating = false;
                }
            }
            else if (e.KeyCode == Keys.F)
            {
                if (isFullScreen)
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                }
                isFullScreen = !isFullScreen;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                if (e.Shift)
                    QuickSaveGame();
                else
                    SaveGame();
            }
            else if (e.Control && e.KeyCode == Keys.L)
            {
                if (e.Shift)
                    QuickLoadGame();
                else
                    LoadGame();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (gameMenu != null && !gameMenu.IsDisposed)
            {
                gameMenu.CenterInParent();
            }
        }
    }

    [Serializable]
    public class GameState
    {
        public List<SerializedCircleShape> Circles { get; set; }
        public DateTime SaveTime { get; set; }
        public string SaveName { get; set; }
    }

    [Serializable]
    public abstract class DisplayObject
    {
        public Point Position { get; set; }
        public Size Size { get; set; }
        public Color Color { get; set; }
        public Vector Velocity { get; set; }
        public bool Accelerating { get; set; }

        public abstract void Draw(Graphics g);
        public abstract void Update(double acceleration);
    }

    [Serializable]
    public class SerializedDisplayObject
    {
        public Point Position { get; set; }
        public Size Size { get; set; }
        public Color Color { get; set; }
        public Vector Velocity { get; set; }
        public bool Accelerating { get; set; }

        public virtual void Draw(Graphics g) { }
        public virtual void Update(double acceleration) { }
    }

    [Serializable]
    public class SerializedCircleShape : SerializedDisplayObject
    {
        public override void Draw(Graphics g)
        {
            using (Brush brush = new SolidBrush(Color))
            {
                g.FillEllipse(brush, new Rectangle(Position, Size));
                g.DrawEllipse(Pens.White, new Rectangle(Position, Size));
            }
        }

        public override void Update(double acceleration)
        {
            if (Accelerating)
            {
                Velocity = new Vector(Velocity.X + Math.Sign(Velocity.X) * acceleration, Velocity.Y + Math.Sign(Velocity.Y) * acceleration);
            }
            Position = new Point(Position.X + (int)Velocity.X, Position.Y + (int)Velocity.Y);
        }
    }

    [Serializable]
    public struct Vector
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}