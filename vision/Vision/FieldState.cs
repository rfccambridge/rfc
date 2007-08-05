using System;
using System.Collections.Generic;
using System.Text;

namespace Vision {
    public class FieldState {
        public static readonly FieldStateForm Form = new FieldStateForm();
        private GameObjects _gameObjects;
        //private GameObjects[] _prevGameObjects;

        public GameObjects GameObjects {
            get { return _gameObjects; }
            set { _gameObjects = value; }
        }
        
     /*   public GameObjects[] PrevGameObjects {
            get { return _prevGameObjects; }
            set { _prevGameObjects = value; }
        }
      */

        public FieldState() {
         //   prevGameObjects = new GameObjects[Constants.get<int>("FRAMES_TO_REMEMBER")];
        }

        public void Update(GameObjects gameObjects) {
            _gameObjects = gameObjects;

            if (Form.Visible)
                Form.UpdateState(gameObjects);
        }
        
    }
}
