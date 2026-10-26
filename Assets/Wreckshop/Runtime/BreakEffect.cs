using UnityEngine;

namespace PocketWreckshop
{
    // Sprite flipbook or a rigid piece sliding across the tabletop. No downward gravity.
    public sealed class BreakEffect : MonoBehaviour
    {
        SpriteRenderer visual;
        Sprite[] frames;
        Vector3 velocity;
        float born,lifetime,spin,drag;
        Color tint;
        public void Play(SpriteRenderer renderer,Sprite[] sequence,float duration,Vector3 drift,float rotationSpeed,float damping,Color color)
        {
            visual=renderer;frames=sequence;lifetime=duration;velocity=drift;spin=rotationSpeed;drag=damping;tint=color;born=Time.unscaledTime;
            visual.sprite=frames[0];visual.color=tint;
        }
        void Update()
        {
            float age=Time.unscaledTime-born,p=Mathf.Clamp01(age/lifetime);
            if(p>=1){Destroy(gameObject);return;}
            visual.sprite=frames[Mathf.Min(frames.Length-1,(int)(p*frames.Length))];
            transform.localPosition+=velocity*Time.unscaledDeltaTime;velocity*=Mathf.Exp(-drag*Time.unscaledDeltaTime);
            transform.Rotate(0,0,spin*Time.unscaledDeltaTime);spin*=Mathf.Exp(-drag*Time.unscaledDeltaTime);
            var color=tint;color.a*=1-Mathf.InverseLerp(.60f,1,p);visual.color=color;
        }
    }
}
