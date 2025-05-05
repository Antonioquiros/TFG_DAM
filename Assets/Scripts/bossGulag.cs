using System.Collections;
using UnityEngine;

public class bossGulag : MonoBehaviour
{
    public float velocidad = 2f;
    public float rangoDeteccion = 8f;
    public float rangoAtaque = 1.5f;
    public float vida = 5f;
    public float da�o = 1f;

    public Transform puntoAtaque;
    public float radioAtaque = 0.5f;
    public float distanciaAtaque = 0.5f; // Distancia en la que el punto de ataque se mueve hacia el enemigo

    private Transform jugador;
    private Animator anim;
    private Rigidbody2D rb;
    private bool atacando = false;
    private bool muerto = false;
    private bool mirandoDerecha = true;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Hacer que mire a la derecha al empezar
        transform.localScale = new Vector3(1, 1, 1);
        mirandoDerecha = true;
    }

    void Update()
    {
        if (muerto || jugador == null) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        // Restablecer par�metros por defecto
        anim.SetBool("andarGulag", false);

        if (distancia <= rangoAtaque && !atacando)  // Si est� cerca y no est� atacando
        {
            StartCoroutine(Atacar());
        }
        else if (distancia <= rangoDeteccion && !atacando)  // Si est� dentro del rango de detecci�n y no est� atacando
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
        anim.SetBool("andarGulag", true);
    }

    IEnumerator Atacar()
    {
        atacando = true;
        rb.velocity = Vector2.zero;

        // Elegir ataque aleatorio
        int tipo = Random.Range(0, 2);

        // Activar el trigger espec�fico para cada ataque
        if (tipo == 0)
            anim.SetTrigger("Ataque1");
        else
            anim.SetTrigger("Ataque2");

        // Esperar al punto de impacto
        yield return new WaitForSeconds(0.5f);

        // L�gica de da�o
        Collider2D[] golpeados = Physics2D.OverlapCircleAll(puntoAtaque.position, radioAtaque);
        foreach (var col in golpeados)
        {
            if (col.CompareTag("Player"))
            {
                col.GetComponent<ControlJugador>().QuitarVida(da�o);
            }
        }

        // Esperar a que la animaci�n termine completamente
        // Esto es importante: necesitamos saber cu�ndo termina la animaci�n
        yield return new WaitForSeconds(1.0f); // Ajusta este tiempo seg�n la duraci�n de tus animaciones

        atacando = false;
    }

    void MirarJugador()
    {
        if (jugador.position.x > transform.position.x && !mirandoDerecha)
        {
            transform.localScale = new Vector3(1, 1, 1);
            mirandoDerecha = true;
        }
        else if (jugador.position.x < transform.position.x && mirandoDerecha)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            mirandoDerecha = false;
        }
    }

    public void RecibirDa�o(float da�oRecibido)
    {
        if (muerto) return;

        vida -= da�oRecibido;

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
            RecibirDa�o(1f);
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
