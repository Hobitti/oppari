using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Mirror.Build.shootterMP
{
    public class Bullet : NetworkBehaviour
    {
        public float speed;
        public float lifetime;
        Rigidbody rb;
        
        //ottaa rigidbodyn ja antaa sille momenttumia mennä eteenpäin
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * speed);
            Invoke(nameof(DestroySelf), lifetime);
        }
        //tuhoaa bulletin lifetime ajan jälkeen
        [Server]
        void DestroySelf()
        {
            NetworkServer.Destroy(gameObject);
        }
           
        //tuhoaa bulletin osuessa
        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            DestroySelf();
        }
    }
}