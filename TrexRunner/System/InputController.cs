using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using TrexRunner.Entities;

namespace TrexRunner.System
{
    public class InputController
    {

        private Trex _trex;
        private KeyboardState _previousKeyboardState;
        private bool _isBlocked;


        // overloads
        public InputController(Trex trex)
        {
            _trex = trex;
        }


        // methods
        public void ProcessControlls(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (!_isBlocked)
            {
                

                bool isJumpKeyPressed = keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space);
                bool isDropKeyPressed = keyboardState.IsKeyDown(Keys.Down);

                bool wasJumpKeyPressed = _previousKeyboardState.IsKeyDown(Keys.Up) || _previousKeyboardState.IsKeyDown(Keys.Space);


                if (!wasJumpKeyPressed && isJumpKeyPressed) 
                {
                    // adD jump logic
                    // if the up key was first pressed start jumping
                    // if the up key was previously pressed (not held down but pressed continuosy with short bursts like a nervous gamer would) then, call continue jump ie make trex go up the highest he can like he got power boots

                    if (_trex.State != TrexState.Jumping)
                    {
                        _trex.BeginJump();
                    }
                }
                else if(_trex.State == TrexState.Jumping && !isJumpKeyPressed)
                {
                    _trex.CancelJump();

                }
                else if (isDropKeyPressed)
                {
                    if (_trex.State == TrexState.Jumping || _trex.State == TrexState.Falling)
                        _trex.Drop();
                    else
                        _trex.Duck();
                }
                else if (_trex.State == TrexState.Ducking && !isDropKeyPressed)
                {
                    _trex.GetUp();
                }
            }


            _isBlocked = false;
            _previousKeyboardState = keyboardState;
        }

        public void BlockInputsTemporarily()
        {
            // mainly prevents jump if space/up is pressed to restart game
            _isBlocked = true;
        }
    }

}
