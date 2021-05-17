using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public abstract class AnimatedValue<T>
        where T : System.IEquatable<T>
    {
        // ------------------------------------------------------------------------------------------------------------

        public T Value
		{
			get => lerpTime >= 1f ? targetValue : GetValue();
			set => Stop(value);
		}

        public T Target
        {
            get => targetValue;
            set => SetTarget(value, speed);
        }

        public void SetTarget(T value)
        {
            SetTarget(value, speed);
        }

        public void SetTarget(T value, float animSpeed)
        {
            if (!Equals(targetValue, value))
            {
                Begin(Value, value, animSpeed);
            }
        }

        public void SetTarget(T startValue, T targetValue)
        {
            SetTarget(startValue, targetValue, speed);
        }

        public void SetTarget(T startValue, T targetValue, float animSpeed)
        {
            if (!Equals(this.targetValue, targetValue) || !Equals(this.startValue, startValue))
            {
                Begin(startValue, targetValue, animSpeed);
            }
        }

        public void Stop()
        {
            startValue = targetValue = Value;
            lerpTime = 1f;
            animating = false;
        }

        public bool Update(float deltaTime)
        {
            if (!animating) return false;
            
            lerpTime = Mathf.Clamp(lerpTime + (deltaTime * speed), 0f, 1f);
            if (lerpTime >= 1f)
            {
                lerpTime = 1f;
                animating = false;
            }

            ValueChanged?.Invoke();

            return true;
        }

        // ------------------------------------------------------------------------------------------------------------

        protected T startValue;
        protected T targetValue;
        protected float lerpTime = 1f;

        private bool animating;
        private float speed = 1f;
        private event System.Action ValueChanged;

        protected AnimatedValue(T value, float animSpeed)
        {
            startValue = value;
            targetValue = value;
            speed = animSpeed;
            
            lerpTime = 1f;
            animating = false;
        }

        protected AnimatedValue(T value, float animSpeed, System.Action callback)
        {
            startValue = value;
            targetValue = value;
            speed = animSpeed;
            
            lerpTime = 1f;
            animating = false;

            ValueChanged -= callback;
            ValueChanged += callback;
        }

        protected void Begin(T newStart, T newTarget, float animSpeed)
        {
            startValue = newStart;
            targetValue = newTarget;
            speed = animSpeed;

            lerpTime = 0f;
            animating = true;
        }

        protected void Stop(T newValue)
        {
            bool invoke = ValueChanged != null && (lerpTime < 1f || !Equals(newValue, GetValue()));

            targetValue = newValue;
            startValue = newValue;

            lerpTime = 1f;
            animating = false;

            if (invoke) ValueChanged.Invoke();
        }

        protected bool Equals(T a, T b)
        {
            return a.Equals(b);
        }

        protected abstract T GetValue();
    }

    // ===================================================================================================================

    public class AnimFloat : AnimatedValue<float>
    {
        public AnimFloat(float value, float speed)
            : base(value, speed)
        { }

        public AnimFloat(float value, float speed, System.Action callback) 
            : base(value, speed, callback)  
        { }

        protected override float GetValue()
        {
            return Mathf.Lerp(startValue, targetValue, lerpTime);
        }
    }

    // ===================================================================================================================

    public class AnimVector3 : AnimatedValue<Vector3>
    {
        public AnimVector3(Vector3 value, float speed)
            : base(value, speed)
        { }

        public AnimVector3(Vector3 value, float speed, System.Action callback)
            : base(value, speed, callback)
        { }

        protected override Vector3 GetValue()
        {
            return Vector3.Lerp(startValue, targetValue, lerpTime);
        }
    }

    // ===================================================================================================================

    public class AnimQuaternion : AnimatedValue<Quaternion>
    {
        public AnimQuaternion(Quaternion value, float speed)
            : base(value, speed)
        { }

        public AnimQuaternion(Quaternion value, float speed, System.Action callback)
            : base(value, speed, callback)
        { }

        protected override Quaternion GetValue()
        {
            return Quaternion.Lerp(startValue, targetValue, lerpTime);
        }
    }

    // ===================================================================================================================
}