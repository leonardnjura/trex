using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TrexRunner.Graphics;

namespace TrexRunner.Entities
{
    public class Trex : IGameEntity, ICollidable
    {
        private const float RUN_ANIMATION_FRAME_LENGTH = 1 / 10f;
        private const float MIN_JUMP_HEIGHT = 40f;

        private const float GRAVITY = 1600f;
        private const float JUMP_START_VELOCITY = -580f;           // pixies jumped per second, no pun tinkerbell 😉
        private const float DROP_VELOCITY = 600f;           

        private const float CANCEL_JUMP_VELOCITY = -100f;

        private const float BLINK_ANIMATION_RANDOM_MIN = 2f;
        private const float BLINK_ANIMATION_RANDOM_MAX = 10f;
        private const float BLINK_ANIMATION_EYE_CLOSE_TIME = 0.5f;

        public const int TREX_DEFAULT_SPRITE_WIDTH = 44;
        public const int TREX_DEFAULT_SPRITE_HEIGHT = 50;

        public const int TREX_DUCKING_SPRITE_WIDTH = 59;
        public const int TREX_DUCKING_SPRITE_HEIGHT = 50;       

        private const int TREX_IDLE_BACKGROUND_SPRITE_POS_X = 40;
        private const int TREX_IDLE_BACKGROUND_SPRITE_POS_Y = 0;

        private const int TREX_DEFAULT_SPRITE_POS_X = 848;
        private const int TREX_DEFAULT_SPRITE_POS_Y = 0;

        private const int TREX_RUNNING_SPRITE_ONE_POS_X = TREX_DEFAULT_SPRITE_POS_X + TREX_DEFAULT_SPRITE_WIDTH * 2;
        private const int TREX_RUNNING_SPRITE_ONE_POS_Y = 0;

        private const int TREX_DUCKING_SPRITE_ONE_POS_X = TREX_DEFAULT_SPRITE_POS_X + TREX_DEFAULT_SPRITE_WIDTH * 6;
        private const int TREX_DUCKING_SPRITE_ONE_POS_Y = 0;

        private const int TREX_DEAD_SPRITE_POS_X = 1068;
        private const int TREX_DEAD_SPRITE_POS_Y = 0;

        public const float START_SPEED = 280f;
        public const float MAX_SPEED = 900f;

        private const float ACCELERATION_PPS_PER_SECOND = 3f;

        private const int COLLISION_BOX_INSET = 3;
        private const int DUCK_COLLISION_REDUCTION = 20;

        private Sprite _idleBackgroundSprite;

        private Sprite _idleSprite;
        private Sprite _idleBlinkSprite;
        private Sprite _deadSprite;

        private SoundEffect _jumpSound;

        private SpriteAnimation _blinkAnimation;
        private SpriteAnimation _runAnimation;
        private SpriteAnimation _duckAnimation;

        private Random _random;

        private float _verticalVelocity;
        private float _dropVelocity;

        private float _startPosY;

        public event EventHandler JumpComplete;
        public event EventHandler Died;

        // ***************************************************
        // props with private fields
        // ***************************************************
        // todo: move notes to helloworld.cs or some c# jupyter
        // kinda fully private props
        // if you'll need to expose fields to outer world use props shorthand and move to props section, mutable {get;set;} immutable{get;};
        //
        // long way..
        // pros 👌🏼
        //      +using the cute identifiable dart-like _privateVar when used within class but still retain props exposed outside, :)
        // eg
        // private string _firstName; //field

        // public string FirstName {  //property
        //    get => _firstName; 
        //    set => _firstName = value; 
        // }
        // 
        // longer way..
        // pros 👌🏼
        //      +has logic eg check setters violating a rule 
        // eg
        // private string _lastName; 
        // public string LastName  
        // {
        //    get 
        //    { 
        //        return _lastName;
        //    }
        //    set 
        //    {
        //        if(value == null)
        //            throw new ArgumentNullException("value", "lastName cannot be null");
        //        _lastName = value; 
        //    }
        // }

        // shorthand..
        // public string FirstName { get; set; }
        // public string BiologicalFather { get; } // read-only fields can only be assigned during creation like in this line or in the constructor, after this, they cannot be changed
        // ***************************************************



        // props
        
        public TrexState State { get; private set; }    // custom enum type

        public Vector2 Position { get; set; }

        public float Speed { get; set; }                // pixies per secs

        public bool IsAlive { get; private set; }

        public int DrawOrder { get; set; }

        public Rectangle CollisionBox
        {
            get
            {
                Rectangle box = new Rectangle(
                (int)Math.Round(Position.X),
                (int)Math.Round(Position.Y),
                TREX_DEFAULT_SPRITE_WIDTH,
                TREX_DEFAULT_SPRITE_HEIGHT
                );
                box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);

                if(State == TrexState.Ducking)
                {
                    box.Y += DUCK_COLLISION_REDUCTION;
                    box.Height -= DUCK_COLLISION_REDUCTION;
                }

                return box;
            }
        }


        // overloads
        public Trex(Texture2D spriteSheet, Vector2 position, SoundEffect jumpSound)
        {
            Position = position;
            State = TrexState.Idle;

            _idleBackgroundSprite = new Sprite(spriteSheet, TREX_IDLE_BACKGROUND_SPRITE_POS_X, TREX_IDLE_BACKGROUND_SPRITE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);

            _random = new Random();

            _jumpSound = jumpSound;

            _startPosY = position.Y;

            _idleSprite = new Sprite(spriteSheet, TREX_DEFAULT_SPRITE_POS_X, TREX_DEFAULT_SPRITE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);
            _idleBlinkSprite = new Sprite(spriteSheet, TREX_DEFAULT_SPRITE_POS_X + TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT);

            _blinkAnimation = new SpriteAnimation();        // empty animation
             CreateBlinkAnimation();
            _blinkAnimation.Play();

            _runAnimation = new SpriteAnimation();          // empty animation
            _runAnimation.AddFrame(new Sprite(spriteSheet, TREX_RUNNING_SPRITE_ONE_POS_X, TREX_RUNNING_SPRITE_ONE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT), 0);
            _runAnimation.AddFrame(new Sprite(spriteSheet, TREX_RUNNING_SPRITE_ONE_POS_X + TREX_DEFAULT_SPRITE_WIDTH, TREX_RUNNING_SPRITE_ONE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH, TREX_DEFAULT_SPRITE_HEIGHT), RUN_ANIMATION_FRAME_LENGTH);
            _runAnimation.AddFrame(_runAnimation[0].Sprite, RUN_ANIMATION_FRAME_LENGTH * 2);       // indexer in use.. quite convenient
            _runAnimation.Play();

            _duckAnimation = new SpriteAnimation();
            _duckAnimation.AddFrame(new Sprite(spriteSheet, TREX_DUCKING_SPRITE_ONE_POS_X, TREX_DUCKING_SPRITE_ONE_POS_Y, TREX_DUCKING_SPRITE_WIDTH, TREX_DUCKING_SPRITE_HEIGHT), 0);
            _duckAnimation.AddFrame(new Sprite(spriteSheet, TREX_DUCKING_SPRITE_ONE_POS_X + TREX_DUCKING_SPRITE_WIDTH, TREX_DUCKING_SPRITE_ONE_POS_Y, TREX_DUCKING_SPRITE_WIDTH, TREX_DUCKING_SPRITE_HEIGHT), RUN_ANIMATION_FRAME_LENGTH);
            _duckAnimation.AddFrame(_duckAnimation[0].Sprite, RUN_ANIMATION_FRAME_LENGTH * 2);       // indexer in use.. quite convenient
            _duckAnimation.Play();

            _deadSprite = new Sprite(spriteSheet, TREX_DEAD_SPRITE_POS_X, TREX_DEAD_SPRITE_POS_Y, TREX_DEFAULT_SPRITE_WIDTH, TREX_DUCKING_SPRITE_HEIGHT);

            IsAlive = true;

        }


        // methods
        public void Initialize()
        {
            Speed = START_SPEED;
            State = TrexState.Running;
            IsAlive = true;
            Position = new Vector2(Position.X, _startPosY);

        }

        
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

            if(IsAlive)
            {
                if (State == TrexState.Idle)
                {
                    _idleBackgroundSprite.Draw(spriteBatch, Position);
                    _blinkAnimation.Draw(spriteBatch, Position);
                }
                else if (State == TrexState.Jumping || State == TrexState.Falling)
                {
                    _idleSprite.Draw(spriteBatch, Position);
                }
                else if (State == TrexState.Running)
                {
                    _runAnimation.Draw(spriteBatch, Position);
                }
                else if (State == TrexState.Ducking)
                {
                    _duckAnimation.Draw(spriteBatch, Position);
                }
            }
            else
            {
                _deadSprite.Draw(spriteBatch, Position);
            }
         }

        public void Update(GameTime gameTime)
        {
            if(State == TrexState.Idle)
            {

                if (!_blinkAnimation.IsPlaying)
                {
                    CreateBlinkAnimation();
                    _blinkAnimation.Play();
                }

                _blinkAnimation.Update(gameTime);
                
            }
            else if (State == TrexState.Jumping || State == TrexState.Falling)
            {
                Position = new Vector2(Position.X, Position.Y + _verticalVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds + _dropVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds) ;
                _verticalVelocity += GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if(_verticalVelocity >= 0)
                    State = TrexState.Falling;

                if(Position.Y >= _startPosY)
                {
                    Position = new Vector2(Position.X, _startPosY);
                    _verticalVelocity = 0;
                    State = TrexState.Running;

                    OnJumpComplete();
                }
            }
            else if (State == TrexState.Running)
            {
                _runAnimation.Update(gameTime);
            }
            else if (State == TrexState.Ducking)
            {
                _duckAnimation.Update(gameTime);
            }

            // increase trex speed as game progresses
            // but dont exceed limit, :)

            if (State != TrexState.Idle)
                Speed += ACCELERATION_PPS_PER_SECOND * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(Speed > MAX_SPEED)
                Speed = MAX_SPEED;


            _dropVelocity = 0;

        }


        private void CreateBlinkAnimation()
        {
            _blinkAnimation.Clear();
            _blinkAnimation.ShouldLoop = false;

            double blinkTimeStamp = BLINK_ANIMATION_RANDOM_MIN + _random.NextDouble() * (BLINK_ANIMATION_RANDOM_MAX - BLINK_ANIMATION_RANDOM_MIN);

            _blinkAnimation.AddFrame(_idleSprite, 0);                                                                   //0 sec
            _blinkAnimation.AddFrame(_idleBlinkSprite, (float)blinkTimeStamp);                                          //trigger second frame at 1/20f sec
            _blinkAnimation.AddFrame(_idleSprite, (float)blinkTimeStamp + BLINK_ANIMATION_EYE_CLOSE_TIME);              //indicator for end of animation

        }


        public bool BeginJump()
        {
            if (State == TrexState.Jumping || State == TrexState.Falling)
                return false;

            _jumpSound.Play();

            State = TrexState.Jumping;

            _verticalVelocity = JUMP_START_VELOCITY;

            return true;

        }
        public bool CancelJump()
        {
            if(State != TrexState.Jumping || (_startPosY - Position.Y) < MIN_JUMP_HEIGHT)
                return false;

            _verticalVelocity = _verticalVelocity < CANCEL_JUMP_VELOCITY ? CANCEL_JUMP_VELOCITY : 0;

            return true;

        }

        public bool Duck()
        {
            if (State == TrexState.Jumping || State == TrexState.Falling)
                return false;

            State = TrexState.Ducking;

            return true;
        }

        public bool GetUp()
        {
            if(State != TrexState.Ducking)
                return false ;

            State = TrexState.Running;

            return true;
        }

        public bool Drop()
        {
            if(State != TrexState.Falling && State != TrexState.Jumping) 
                return false;

            State = TrexState.Falling;
            _dropVelocity = DROP_VELOCITY;

            return true;
        }

        protected virtual void OnJumpComplete()
        {
            EventHandler handler = JumpComplete;
            handler?.Invoke(this, EventArgs.Empty);
            
        }

        protected virtual void OnDied()
        { 
            EventHandler handler = Died;
            handler?.Invoke(this, EventArgs.Empty);
            
        }

        public bool Die()
        {
            if(!IsAlive)
                return false;

            State = TrexState.Idle;
            Speed = 0;
            IsAlive = false;

            OnDied();

            return true;
        }

    }
}
