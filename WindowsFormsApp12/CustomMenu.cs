using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp12
{
    public class CustomMenu : Form
    {
        private const int MenuWidth = 300;
        private const int MenuHeight = 350;
        private const int ButtonHeight = 40;
        private const int ButtonMargin = 10;

        public CustomMenu()
        {
            InitializeMenu();
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.Size = new Size(MenuWidth, MenuHeight);
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Opacity = 0.95;
            this.Padding = new Padding(20);
        }

        public void CenterInParent()
        {
            if (this.Owner != null && this.Owner.Width > 0 && this.Owner.Height > 0)
            {
                this.Location = new Point(
                    this.Owner.Left + (this.Owner.Width - this.Width) / 2,
                    this.Owner.Top + (this.Owner.Height - this.Height) / 2
                );
            }
        }

        private void InitializeMenu()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var continueButton = CreateMenuButton("Продолжить", Color.FromArgb(62, 62, 64));
            continueButton.Click += (s, e) => ContinueGame?.Invoke(this, EventArgs.Empty);

            var saveButton = CreateMenuButton("Сохранить", Color.FromArgb(62, 62, 64));
            saveButton.Click += (s, e) => SaveGame?.Invoke(this, EventArgs.Empty);

            var loadButton = CreateMenuButton("Загрузить", Color.FromArgb(62, 62, 64));
            loadButton.Click += (s, e) => LoadGame?.Invoke(this, EventArgs.Empty);

            var settingsButton = CreateMenuButton("Настройки", Color.FromArgb(62, 62, 64));
            settingsButton.Click += (s, e) => OpenSettings?.Invoke(this, EventArgs.Empty);

            var exitButton = CreateMenuButton("Выход", Color.FromArgb(90, 30, 30));
            exitButton.Click += (s, e) => ExitGame?.Invoke(this, EventArgs.Empty);

            mainPanel.Controls.Add(exitButton);
            mainPanel.Controls.Add(settingsButton);
            mainPanel.Controls.Add(loadButton);
            mainPanel.Controls.Add(saveButton);
            mainPanel.Controls.Add(continueButton);

            this.Controls.Add(mainPanel);
        }

        private Button CreateMenuButton(string text, Color backColor)
        {
            var button = new Button
            {
                Text = text,
                Height = ButtonHeight,
                Dock = DockStyle.Top,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, ButtonMargin, 0, 0),
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 82);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 100, 102);

            return button;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(80, 80, 80), 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }

            using (var brush = new SolidBrush(Color.White))
            using (var font = new Font("Arial", 12, FontStyle.Bold))
            {
                e.Graphics.DrawString("Меню игры", font, brush, 20, 15);
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this.Hide();
            ContinueGame?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ContinueGame;
        public event EventHandler SaveGame;
        public event EventHandler LoadGame;
        public event EventHandler OpenSettings;
        public event EventHandler ExitGame;
    }
}