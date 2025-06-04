using System.Collections;
using UnityEngine;

public class bossGulag : MonoBehaviour
{
    public float velocidad = 2f;
    public float rangoDeteccion = 8f;
    public float rangoAtaque = 1.5f;
    public float vida = 5f;
    public float daño = 1f;

    public Transform puntoAtaque;
    public float radioAtaque = 0.5f;
    public float distanciaAtaque = 0.5f; // Distancia en la que el punto de ataque se mueve hacia el enemigo

    private Transform jugador;
    private Animator anim;
    private Rigidbody2D rb;
    private bool atacando = false;
    private bool muerto = false;
    private bool mirandoDerecha = true;

    public AudioClip disparoFX;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        // Esta parte del codigo es para que ignore los obstaculos y asi se pueda mover bien por el mapa
        Collider2D bossCollider = GetComponent<Collider2D>();

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

    void Update()
    {
        if (muerto || jugador == null) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        if (distancia <= rangoAtaque && !atacando)  // Si está cerca y no está atacando
        {
            StartCoroutine(Atacar());
        }
        else if (distancia <= rangoDeteccion && !atacando)  // Si está dentro del rango de detección y no está atacando
        {
            MoverHaciaJugador();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        MirarJugador();
    }

    void MoverHaciaJugador()
    {
        float direccion = jugador.position.x - transform.position.x;
        rb.velocity = new Vector2(Mathf.Sign(direccion) * velocidad, rb.velocity.y);
    }

    IEnumerator Atacar()
    {
        atacando = true;
        audioSource.PlayOneShot(disparoFX);
        rb.velocity = Vector2.zero;

        // Elegir ataque aleatorio
        int tipo = Random.Range(0, 2);

        // Activar el trigger específico para cada ataque
        if (tipo == 0)
            anim.SetTrigger("Ataque1");
        else
            anim.SetTrigger("Ataque2");

        // Esperar al punto de impacto
        yield return new WaitForSeconds(1f);

        // Lógica de daño
        Collider2D[] golpeados = Physics2D.OverlapCircleAll(puntoAtaque.position, radioAtaque);
        foreach (var col in golpeados)
        {
            if (col.CompareTag("Player"))
            {
                col.GetComponent<ControlJugador>().QuitarVida(daño);
            }
        }

        atacando = false;
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

    public void RecibirDaño(float dañoRecibido)
    {
        if (muerto) return;

        vida -= dañoRecibido;

        if (vida <= 0)
        {
            StartCoroutine(Morir());
        }
    }

    IEnumerator Morir()
    {
        muerto = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        anim.SetTrigger("Muerte");
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("bullet"))
        {
            RecibirDaño(1f);
            Destroy(other.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (puntoAtaque != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(puntoAtaque.position, radioAtaque);
        }
    }
}
