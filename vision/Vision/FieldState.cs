using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Vision {
    public class FieldState {
        public static readonly FieldStateForm Form = new FieldStateForm();
        private VisionMessage _visionMessage;
        

        public VisionMessage VisionMessage {
            get { return _visionMessage; }
            set { _visionMessage = value; }
        }

        public FieldState() {
        }

        public void Update(VisionMessage visionMessage) {
            _visionMessage = visionMessage;

            if (Form.Visible)
                Form.UpdateState(visionMessage);
        }
        
    }
}
