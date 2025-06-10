using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JefeNv2 : MonoBehaviour
{
    public float velocidad = 2f;
    public float vida = 10f;
    public float distanciaDetectar = 20f;
    public float distanciaAtaque = 2f;
    public float dañoAtaque = 1f;
    public Transform jugador;
    public Transform controladorAtaque;
    public float radioAtaque = 1.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool mirandoDerecha = false;
    private bool atacando = false;
    private bool estaMuerto = false;

    public AudioClip disparoFX;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);// Esto es para que el jugador se gire correctamente al iniciar el juego

        // Esta parte del codigo es para que ignore los obstaculos y asi se pueda mover bien por el mapa
        Collider2D bossCollider = GetComponent<Collider2D>();

        GameObject[] fisicos = GameObject.FindGameObjectsWithTag("fisico");

        foreach (GameObject obj in fisicos)
        {
            Collider2D objCollider = obj.GetComponent<Collider2D>();
            if (objCollider != null)
            {
                Physics2D.IgnoreCollision(bossCollider, objCollider);
            }
        }
        // Ignorar colisión con el jugador
        if (jugador != null)
        {
            Collider2D playerCollider = jugador.GetComponent<Collider2D>();
            if (playerCollider != null && bossCollider != null)
            {
                Physics2D.IgnoreCollision(bossCollider, playerCollider, true);
            }
        }
    }

    private void Update()
    {
        if (estaMuerto) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        if (distancia < distanciaDetectar && !atacando)
        {
            MoverHaciaJugador();

            if (distancia < distanciaAtaque)
            {
                rb.velocity = Vector2.zero;
                StartCoroutine(Atacar());
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
  
        }

        MirarJugador();
    }

    void MoverHaciaJugador()                   // Jefe Nv2
    {
        // Dirección normalizada hacia el jugador
        Vector2 direccion = (jugador.position - transform.position).normalized;
        rb.velocity = direccion * velocidad;
    }

    void MirarJugador()
    {
        bool mirarDerecha = jugador.position.x > transform.position.x;

        if (mirarDerecha != mirandoDerecha)
        {
            mirandoDerecha = mirarDerecha;
            Vector3 escala = transform.localScale;
            escala.x *= -1;
            transform.localScale = escala;
        }
    }

    IEnumerator Atacar()
    {
        atacando = true;
        anim.SetTrigger("Ataque");
        yield return new WaitForSeconds(0.5f); // Tiempo de la animación hasta el golpe
        audioSource.PlayOneShot(disparoFX);

        Collider2D[] golpeados = Physics2D.OverlapCircleAll(controladorAtaque.position, radioAtaque);
        foreach (var col in golpeados)
        {
            if (col.CompareTag("Player"))
            {
                col.GetComponent<ControlJugador>().QuitarVida(dañoAtaque);
            }
        }

        yield return new WaitForSeconds(1f); // Tiempo entre ataques
        atacando = false;
    }

    public void RecibirDaño(float daño)
    {
        if (estaMuerto) return;

        vida -= daño;

        if (vida <= 0)
        {
            StartCoroutine(MorirConAnimacion());
        }
    }

    IEnumerator MorirConAnimacion()
    {
        estaMuerto = true;
        rb.velocity = Vector2.zero;
        anim.SetTrigger("Muerte");
        GetComponent<Collider2D>().enabled = false;
        rb.isKinematic = true;

        // Esperar a que termine la animación actual
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("bullet"))
        {
            RecibirDaño(1);
            Destroy(collision.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (controladorAtaque != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(controladorAtaque.position, radioAtaque);
        }
    }
}
