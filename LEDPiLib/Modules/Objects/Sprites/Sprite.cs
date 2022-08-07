using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LEDPiLib.Modules.Objects.Sprites
{
    public class Sprite
    {
        int x, y, w;
        float speed, index;
        int len;
        List<Image<Rgba32>> animations;
        public Sprite(List<Image<Rgba32>> animations, int x, int y, float speed)
        {
            this.x = x;
            this.y = y;
            this.animations = animations;
            this.w = animations[0].Width;
            this.len = animations.Count;
            this.speed = speed;
            index = 0;
        }

        public void Show(Image<Rgba32> canvas)
        {
            int index2 = Convert.ToInt32(Math.Floor(Convert.ToDouble(index)) % len);
            int localX = x;
            int localY = y;

            Image<Rgba32> currentImage = animations[index2].Clone();

            if (x < 0)
            {
                localX = 0;
                int cropX = (currentImage.Width - x) - currentImage.Width;
                currentImage.Mutate(c => c.Crop(new Rectangle(cropX, 0, currentImage.Width - cropX, currentImage.Height)));
            }

            if (x + w > canvas.Width)
            {
                int cropLength = currentImage.Width + canvas.Width - x - w;

                if (cropLength == 0)
                    return;

                currentImage.Mutate(c => c.Crop(cropLength, currentImage.Height));
            }

            canvas.Mutate(c => c.DrawImage(currentImage, new Point(localX, y), 1));
            
        }

        public void Animate(int width)
        {
            index += speed;
            x += Convert.ToInt32(speed * 10);

            if (x > width)
            {
                x = -w + 1;
            }
        }
    }
}
