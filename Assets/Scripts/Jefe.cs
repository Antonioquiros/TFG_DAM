using System.Collections;
using UnityEngine;

public class Jefe : MonoBehaviour
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

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
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
            anim.SetBool("Caminar", false);
        }

        MirarJugador();
    }

    void MoverHaciaJugador()
    {
        float direccion = jugador.position.x - transform.position.x;
        rb.velocity = new Vector2(Mathf.Sign(direccion) * velocidad, rb.velocity.y);
        anim.SetBool("Caminando", true);
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
        else
        {
            StartCoroutine(PausaCorta());
        }
    }

    IEnumerator PausaCorta()
    {
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.3f);
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
