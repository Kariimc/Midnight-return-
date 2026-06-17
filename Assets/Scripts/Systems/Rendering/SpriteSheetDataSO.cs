using System;
using UnityEngine;

namespace MidnightReturn.Systems.Rendering
{
    [Serializable]
    public struct SpriteClip
    {
        public string Name;
        public int    StartFrame;
        public int    FrameCount;
        public float  Fps;
        public bool   Loop;
    }

    // ScriptableObject that describes a sprite sheet atlas layout and named animation clips.
    // Create via Assets > MidnightReturn > Sprite Sheet Data.
    [CreateAssetMenu(fileName = "SpriteSheet_", menuName = "MidnightReturn/Sprite Sheet Data")]
    public class SpriteSheetDataSO : ScriptableObject
    {
        [Header("Atlas Layout")]
        public Texture2D Sheet;
        public int Columns = 1;
        public int Rows    = 1;

        [Header("Animation Clips")]
        public SpriteClip[] Clips;

        public float FrameW => 1f / Columns;
        public float FrameH => 1f / Rows;

        public bool TryGetClip(string clipName, out SpriteClip clip)
        {
            foreach (var c in Clips)
            {
                if (c.Name == clipName)
                {
                    clip = c;
                    return true;
                }
            }
            clip = default;
            return false;
        }

        // Returns UV rect (x=offsetU, y=offsetV, z=scaleU, w=scaleV) for a given frame index.
        public Vector4 GetFrameUV(int frameIndex)
        {
            int col = frameIndex % Columns;
            int row = frameIndex / Columns;
            // UV origin is bottom-left in OpenGL; sprite sheets are top-left — flip row.
            int flippedRow = (Rows - 1) - row;
            return new Vector4(
                col  * FrameW,
                flippedRow * FrameH,
                FrameW,
                FrameH
            );
        }
    }
}
