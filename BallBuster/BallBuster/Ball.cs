using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace BallBuster
{
    class Ball
    {
        Texture2D Texture;
        int XPosition;
        int YPosition;
        Vector2 Position;
        public bool IsAlive;
        public bool IsUsed;

        public enum BallColor
        {
            Blue = 0,
            Green,
            Purple,
            Red,
            Teal,
            Yellow
        }

        public BallColor color;

        public Ball()
        {
            IsAlive = false;
            IsUsed = false;
        }

        public int Width
        {
            get { return Texture.Width; }
        }

        public int Height
        {
            get { return Texture.Height; }
        }

        public int X
        {
            get { return XPosition; }
            set
            {
                XPosition = value;
                Position.X = XPosition * Width;
            }
        }

        public int Y
        {
            get { return YPosition; }
            set
            {
                YPosition = value;
                Position.Y = (YPosition * Height) + 100;
            }
        }

        public void Initialize(Texture2D texture, int xPosition, int yPosition, BallColor ballColor)
        {
            Texture = texture;
            XPosition = xPosition;
            YPosition = yPosition;
            Position = new Vector2(xPosition * Width, (yPosition * Height) + 100);
            IsAlive = true;
            color = ballColor;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}
