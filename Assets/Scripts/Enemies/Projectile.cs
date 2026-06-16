using UnityEngine;
using MidnightReturn.Data;

namespace MidnightReturn.Enemies
{
    public class Projectile : MonoBehaviour
    {
        private Vector3    _velocity;
        private int        _damage;
        private DamageType _type;
        private float      _lifetime = 5f;

        public void Init(Vector3 velocity, int damage, DamageType type)
        {
            _velocity = velocity;
            _damage   = damage;
            _type     = type;
        }

        private void Update()
        {
            transform.position += _velocity * Time.deltaTime;
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<Player.PlayerController>()?.TakeDamage(_damage, _type);
                Destroy(gameObject);
            }
            else if (!other.CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }
    }
}
