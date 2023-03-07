using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrexRunner.Graphics
{
    public class SpriteAnimation
    {
        // props
        public List<SpriteAnimationFrame> _frames = new List<SpriteAnimationFrame>();


        public SpriteAnimationFrame this[int index]                 // indexer.. read more
        {
            get
            {
                return GetFrame(index);
            }
        }

        public int FrameCount => _frames.Count;

        public SpriteAnimationFrame CurrentFrame
        {
            get 
            {
                return _frames
                    .Where(f => f.TimeStamp <= PlaybackProgress)    // lambda func check if less than or equal to
                    .OrderBy(f => f.TimeStamp)                      // high to low
                    .LastOrDefault();                               // incase there is no element, default returns null
            }
        }

        public float Duration
        {
            get
            {
                if(!_frames.Any())
                    return 0f;
                return _frames.Max(f => f.TimeStamp);
            
            }
        }


        public bool IsPlaying { get; private set; }

        public float PlaybackProgress { get; private set; }

        public bool ShouldLoop { get; set; } = true;

        // methods
        public void AddFrame(Sprite sprite, float timeStamp)
        {
            SpriteAnimationFrame frame = new SpriteAnimationFrame(sprite, timeStamp);
            _frames.Add(frame);
        }

        public void Update(GameTime gameTime)
        {
            if (IsPlaying)
            {
                PlaybackProgress += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (PlaybackProgress > Duration)
                {
                    if(ShouldLoop)
                        PlaybackProgress -= Duration;
                    else
                        Stop();
                }
            }

        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            SpriteAnimationFrame frame = CurrentFrame;
            if(frame != null)
                frame.Sprite.Draw(spriteBatch, position);
        }

        public void Play()
        {
            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying=false;
            PlaybackProgress = 0;
        }


        public SpriteAnimationFrame GetFrame(int index)
        {
            if(index < 0 || index >= _frames.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Frame with index {index} does not exist in this animation");
            
            return _frames[index];
        }


        public void Clear()
        {
            Stop();
            _frames.Clear();
        }





        /*NOTES*/
        public void TeachMeSomethinNew()
        {
            // ***************************************************
            // indexer
            // ***************************************************
            // notes
            // todo: move to helloworld.cs
            // indexers are used for convenience
            // say we had
            SpriteAnimation anim = new SpriteAnimation();
            // then added a couple of frames
            // now to access them, we'd normally do
            var x1 = anim.GetFrame(1);
            // for convenience..
            var x2 = anim[2];
            // ***************************************************
        }


        public static SpriteAnimation CreateSimpleAnimation(Texture2D texture, Point startPos, int width, int height, Point offset, int frameCount, float frameLength)
        {
            if(texture == null)
                throw new ArgumentNullException(nameof(texture));

            SpriteAnimation anim = new SpriteAnimation();

            for(int i = 0; i < frameCount; i++)
            {
                Sprite sprite = new Sprite(texture, startPos.X + i * offset.X, startPos.Y + i * offset.Y, width, height);
                anim.AddFrame(sprite, frameLength * 1);

                if(i == frameCount - 1) // last frame
                    anim.AddFrame(sprite, frameLength * (i + 1));
            }

            return anim;

        }


    }
}
