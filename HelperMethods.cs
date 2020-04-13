using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F1Telemetry
{
    public static class HelperMethods
    {
        public static Texture2D DrawRectangle(GraphicsDevice gd, SpriteBatch sb, Rectangle input, Color boxColour)
        {
            Texture2D _texture;
            _texture = new Texture2D(gd, 1, 1);
            _texture.SetData(new Color[] { Color.White });
            sb.Draw(_texture, input, boxColour);

            return _texture;
        }


        //public static Texture2D BlankTexture(this SpriteBatch s)
        //{
        //    if (_blankTexture == null)
        //    {
        //        _blankTexture = new Texture2D(s.GraphicsDevice, 1, 1);
        //        _blankTexture.SetData(new[] { Color.White });
        //    }
        //    return _blankTexture;
        //}
    }
}
