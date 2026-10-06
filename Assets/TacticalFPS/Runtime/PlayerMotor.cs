using UnityEngine;

namespace TacticalFPS
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        public Camera View;
        public GameDirector Director;
        public float Stamina { get; private set; } = 100f;
        public bool Crouched { get; private set; }
        public float Sensitivity = 1.8f;
        CharacterController body;
        float pitch, vertical;
        public Health Health { get; private set; }
        void Awake() { body = GetComponent<CharacterController>(); Health = GetComponent<Health>(); }
        void Update()
        {
            if (!Director || !Director.Active || Health.Dead) return;
            float yaw = Input.GetAxisRaw("Mouse X") * Sensitivity;
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * Sensitivity, -85f, 85f);
            transform.Rotate(0, yaw, 0); View.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            bool wantsCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
            if (!wantsCrouch && Crouched && Physics.CheckSphere(transform.position + Vector3.up * 1.7f, .32f, WorldBuilder.WorldMask)) wantsCrouch = true;
            Crouched = wantsCrouch; body.height = Crouched ? 1.2f : 1.85f; body.center = Vector3.up * body.height * .5f;
            View.transform.localPosition = new Vector3(0, Mathf.Lerp(View.transform.localPosition.y, Crouched ? 1.05f : 1.65f, Time.deltaTime * 14), 0);
            float x = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            float z = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
            Vector3 movement = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1);
            bool sprint = Input.GetKey(KeyCode.LeftShift) && !Crouched && Stamina > 1 && movement.sqrMagnitude > .1f;
            Stamina = Mathf.Clamp(Stamina + (sprint ? -24 : 15) * Time.deltaTime, 0, 100);
            if (body.isGrounded) { vertical = -2; if (Input.GetKeyDown(KeyCode.Space) && !Crouched) vertical = 6.2f; }
            vertical = Mathf.Max(-25, vertical - 20 * Time.deltaTime);
            body.Move((movement * (Crouched ? 2.3f : sprint ? 7.2f : 4.5f) + Vector3.up * vertical) * Time.deltaTime);
            if (transform.position.y < -15) Health.Damage(10000, Team.Hostile);
            if (Input.GetKeyDown(KeyCode.E)) Director.Interact();
            if (Input.GetKeyDown(KeyCode.H) && Director.Medkits > 0 && Health.Current < Health.Maximum)
            { Director.Medkits--; Health.Heal(45); }
        }
    }
}
