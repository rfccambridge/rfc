using System;
using System.Collections.Generic;
using System.Text;

namespace Vision {
    public class FieldState {
        public static readonly FieldStateForm Form = new FieldStateForm();
        private GameObjects _gameObjects;

        public GameObjects GameObjects {
            get { return _gameObjects; }
            set { _gameObjects = value; }
        }

        public FieldState() {
        }

        public void Update(GameObjects gameObjects) {
            _gameObjects = gameObjects;

            if (Form.Visible)
                Form.UpdateState(gameObjects);
        }
        
    }
}
