// Copyright (C) 2016 ricimi - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace Ricimi
{
    // This class is responsible for loading the next scene in a transition (the core of
    // this work is performed in the Transition class, though).
    public class SceneTransition : MonoBehaviour
    {
        public static SceneTransition GetSceneTransition;
        public string scene = "<Insert scene name>";
        public float duration = 1.0f;
        public Color color = Color.black;

        void Start() {
                GetSceneTransition = this;
            
        }
        public void PerformTransition(int val)
        {
            scene = GetScene();
            if (val == 0)
            {
                Transition.LoadLevel("main", duration, color);
            }
            else if (val == 1) {
                Transition.LoadLevel("ResultScene", duration, color);
            }
            else
            {
                Transition.LoadLevel("start", duration, color);
            }


        }

        public void SetScene(string scene_input) { scene_input = scene;}
        public string GetScene() { return scene; }
    }
}
