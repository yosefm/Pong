using System;
using System.Collections.Generic;

using System.Windows.Forms;
using System.Drawing;

namespace Pong
{
    internal enum Player { Left, Right };
    internal struct Paddle
    {
        public float position; // [-1,1], relative height to game height.
        public Player side;

        public Paddle(Player init_side)
        {
            position = 0;
            side = init_side;
        }
    }

    internal struct Ball
    {
        public Point center;
        public float[] direction;
        public uint radius;

        public Ball(uint rad)
        {
            center = new Point(0, 0);
            direction = new float[] { 0, 0};
            radius = rad;
        }
    }

    public class PongWindow : Form
    {
        private const int gameSide = 500;
        private const uint paddleMargin = 10;
        private const int paddleWidth = 50;

        private PictureBox gameArea;
        private Timer timer = new Timer();

        private Dictionary<Player, Paddle> paddles = new Dictionary<Player, Paddle>();
        private Ball ball;

        public PongWindow()
        {
            // Winforms setup:
            Text = "Pong";
            Size = new Size(gameSide, gameSide);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            CenterToScreen();

            gameArea = new PictureBox();
            gameArea.Size = new Size(gameSide, gameSide);
            gameArea.Paint += new PaintEventHandler(OnPaint);
            Shown += (sender, e) => gameArea.Invalidate();

            Controls.Add(gameArea);

            KeyDown += new KeyEventHandler(OnKey);

            timer.Tick += new EventHandler(OnTick);
            timer.Interval = 100;
            timer.Enabled = true;
            timer.Start();

            // Game setup:
            paddles[Player.Left] = new Paddle(Player.Left);
            paddles[Player.Right] = new Paddle(Player.Right);
            ball = new Ball(5);
            ball.direction = new float[] { 0.5f, 0.886f };
            ball.center = new Point(gameSide / 2, gameSide / 2);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            DrawGameArea(e.Graphics);
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            // The kludge here demonstrate why using structs is problematic:
            // you always get a copy.
            if (e.KeyCode == Keys.Up) {
                var paddle = paddles[Player.Right];
                paddle.position -= 0.05f;
                paddles[Player.Right] = paddle;
            } 
            else if (e.KeyCode == Keys.Down) {
                var paddle = paddles[Player.Right];
                paddle.position += 0.05f;
                paddles[Player.Right] = paddle;
            }
            else if (e.KeyCode == Keys.W) {
                var paddle = paddles[Player.Left];
                paddle.position -= 0.05f;
                paddles[Player.Left] = paddle;
            }
            else if (e.KeyCode == Keys.S) {
                var paddle = paddles[Player.Left];
                paddle.position += 0.05f;
                paddles[Player.Left] = paddle;
            }

            DrawGameArea(gameArea.CreateGraphics());
        }

        private void OnTick(object sender, EventArgs e)
        {
            float velocity = 10;
            ball.center.X += (int)(ball.direction[0] * velocity);
            ball.center.Y += (int)(ball.direction[1] * velocity);

            // Horizontal edges always bounce:
            if (ball.center.Y - ball.radius <= 0) {
                ball.center.Y = (int)ball.radius - ball.center.Y;
                ball.direction[1] = -ball.direction[1];
            }
            else if (ball.center.Y + ball.radius >= gameSide) {
                ball.center.Y = 2 * gameSide - (ball.center.Y + (int)ball.radius);
                ball.direction[1] = -ball.direction[1];
            }
            // Vertical sides bounce if paddle present:
            else if (ball.center.X - ball.radius <= paddleMargin) {
                int paddlePos = (int)(0.5 * gameSide * (paddles[Player.Left].position + 1));
                if (paddlePos + paddleWidth < ball.center.Y || paddlePos - paddleMargin > ball.center.Y)
                    FinishGame(Player.Right);

                ball.center.X = 2*(int)paddleMargin - (ball.center.X - (int)ball.radius);
                ball.direction[0] = -ball.direction[0];
            }
            else if (ball.center.X + ball.radius >= gameSide - paddleMargin) {
                int paddlePos = (int)(0.5 * gameSide * (paddles[Player.Right].position + 1));
                if (paddlePos + paddleWidth < ball.center.Y || paddlePos - paddleWidth > ball.center.Y)
                    FinishGame(Player.Left);

                ball.center.X = 2*(gameSide - (int)paddleMargin) - (ball.center.X + (int)ball.radius);
                ball.direction[0] = -ball.direction[0];
            }
            DrawGameArea(gameArea.CreateGraphics());
        }

        private void FinishGame(Player winner) {
            timer.Stop();
        }

        private void DrawGameArea(Graphics canvas)
        {
            canvas.Clear(Color.White);
            Pen pen1 = new System.Drawing.Pen(Color.Blue, 2F);

            canvas.DrawEllipse(pen1,
                ball.center.X - ball.radius, ball.center.Y - ball.radius, ball.radius*2, ball.radius*2);

            foreach (var paddle in paddles) {
                var x = (int)(paddle.Key == Player.Left ? paddleMargin : gameSide - paddleMargin);
                var y = (int)(0.5*gameSide*(paddle.Value.position + 1));
                canvas.DrawLine(pen1, new Point(x, y - paddleWidth/2), new Point(x, y + paddleWidth / 2));
            }
        }

        static public void Main()
        {
            Application.Run(new PongWindow());
        }
    }
}
