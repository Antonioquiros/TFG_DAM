using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlJugador : MonoBehaviour
{
    public int velocidad;
    public int fuerzaSalto;
    public GameObject bullertPef;
    public float retardoDisparo = 0.5f;
    public float alturaDisparo = 0.2f;
    public float numVidas = 3f; 
    public int tiempoNivel;

    // Variables para las llaves
    public int llavesRecolectadas = 0;
    public bool tieneLlave2 = false;

    public Canvas canvas;
    public controlHUD hud;
    private bool puedeDisparar = true;
    private Rigidbody2D fisica;
    private SpriteRenderer sprite;
    private Animator animacion;
    private bool estaDisparando = false;
    private bool mirandoDerecha = true;
    private Vector3 ultimaDireccionDisparo;
    private bool vulnerable;
    private float tiempoInicio;
    private int tiempoEmpleado;
    private bool mostrandoAnimacionMuerte = false;

    // Variables para el Gulag
    private bool fueAlGulag = false;
    private string escenaGulag = "Gulag";
    public string escenaNivel;

    // Guardar el estado de las variables
    private float vidasGuardadas;
    private int tiempoGuardado;
    private bool fueAlGulagGuardado;

    void Start()
    {
        escenaNivel = SceneManager.GetActiveScene().name; // Guardamos el nivel actual
        hud = canvas.GetComponent<controlHUD>();
        tiempoInicio = Time.time;
        vulnerable = true;
        fisica = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animacion = GetComponent<Animator>();
        hud.setVidasTxt(numVidas); // Actualizamos el HUD con las vidas iniciales

        // Recuperar el estado guardado
        if (PlayerPrefs.HasKey("VidasGuardadas"))
        {
            vidasGuardadas = PlayerPrefs.GetFloat("VidasGuardadas");
            tiempoGuardado = PlayerPrefs.GetInt("TiempoGuardado");
            fueAlGulagGuardado = PlayerPrefs.GetInt("FueAlGulagGuardado") == 1;
        }
        else
        {
            vidasGuardadas = numVidas;
            tiempoGuardado = tiempoNivel;
            fueAlGulagGuardado = false;
        }
    }

    private void FixedUpdate()
    {
        if (mostrandoAnimacionMuerte) return; // No mover durante animación de muerte

        float entradaX = Input.GetAxis("Horizontal");
        fisica.velocity = new Vector2(entradaX * velocidad, fisica.velocity.y);
    }

    private void Update()
    {
        if (mostrandoAnimacionMuerte) return; // No procesar inputs durante animación de muerte

        if (Input.GetKeyDown(KeyCode.UpArrow) && TocandoSuelo())
        {
            fisica.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
        }

        if (fisica.velocity.x > 0)
        {
            sprite.flipX = false;
            mirandoDerecha = true;
        }
        else if (fisica.velocity.x < 0)
        {
            sprite.flipX = true;
            mirandoDerecha = false;
        }

        animarJugador();
        manejarDisparo();

        tiempoEmpleado = (int)(Time.time - tiempoInicio);
        hud.setTiempoTxt(tiempoNivel - tiempoEmpleado);
        hud.setLlavesTxt(llavesRecolectadas);
        if (tiempoNivel - tiempoEmpleado < 0) FinJuego();
    }

    private void animarJugador()
    {
        if (estaDisparando || mostrandoAnimacionMuerte) return;

        if (!TocandoSuelo()) animacion.Play("jugadorSaltando");
        else if ((fisica.velocity.x > 1 || fisica.velocity.x < -1) && fisica.velocity.y == 0) animacion.Play("jugadorCorriendo");
        else if ((fisica.velocity.x < 1 || fisica.velocity.x > -1) && fisica.velocity.y == 0) animacion.Play("jugadorParado");
    }

    private void manejarDisparo()
    {
        if (Input.GetKeyDown(KeyCode.Space) && puedeDisparar && !mostrandoAnimacionMuerte)
        {
            puedeDisparar = false;
            ultimaDireccionDisparo = mirandoDerecha ? Vector3.right : Vector3.left;
            animacion.Play("jugadorDisparando");
            estaDisparando = true;
            Invoke(nameof(DisparoRetardado), retardoDisparo);
            float duracionAnimacion = animacion.GetCurrentAnimatorStateInfo(0).length;
            Invoke(nameof(ResetearDisparo), duracionAnimacion);
            Invoke(nameof(HabilitarDisparo), 1f);
        }
    }

    private void HabilitarDisparo()
    {
        puedeDisparar = true;
    }

    private void DisparoRetardado()
    {
        Vector3 posicionDisparo = transform.position + new Vector3(0, alturaDisparo, 0);
        float desplazamientoHorizontal = mirandoDerecha ? 0.5f : -0.5f;
        posicionDisparo += new Vector3(desplazamientoHorizontal, 0, 0);
        GameObject bullet = Instantiate(bullertPef, posicionDisparo, Quaternion.identity);
        bulletScript bulletComponent = bullet.GetComponent<bulletScript>();
        bulletComponent.setDirection(ultimaDireccionDisparo);
        bulletComponent.SetShooter(gameObject);
    }

    private void ResetearDisparo()
    {
        estaDisparando = false;
    }

    private bool TocandoSuelo()
    {
        RaycastHit2D toca = Physics2D.Raycast(transform.position + new Vector3(0, -2f, 0), Vector2.down, 0);
        return toca.collider != null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("llave1"))
        {
            llavesRecolectadas++;
            Destroy(other.gameObject);

            if (llavesRecolectadas == 5)
            {
                SceneManager.LoadScene("Interlude1");
            }
        }
        else if (other.CompareTag("llave2"))
        {
            tieneLlave2 = true;
            llavesRecolectadas++;
            Destroy(other.gameObject);
            SceneManager.LoadScene("Interlude1");
        }
    }

    public void QuitarVida(float daño)
    {
        if (vulnerable && !mostrandoAnimacionMuerte)
        {
            vulnerable = false;
            numVidas -= daño;
            hud.setVidasTxt(numVidas);

            if (numVidas <= 0)
            {
                if (!fueAlGulag)
                {
                    // Primera muerte - ir al Gulag con EXACTAMENTE 1 vida
                    numVidas = 1f; // Fijamos a 1 vida aquí mismo
                    StartCoroutine(MostrarAnimacionMuerteYGulag());
                }
                else
                {
                    // Muerte definitiva en el Gulag
                    PerderGulag();  // Fin del juego si pierde el Gulag
                }
            }
            else
            {
                // Daño normal (todavía tiene vidas)
                Invoke("HacerVulnerable", 1f);
                sprite.color = Color.red;
            }
        }
    }


    private IEnumerator MostrarAnimacionMuerteYGulag()
    {
        mostrandoAnimacionMuerte = true;
        animacion.Play("jugadorMuerte");

        yield return new WaitForSeconds(1f); // Duración de la animación

        // Guardar el nivel actual para poder volver después
        PlayerPrefs.SetString("UltimoNivel", escenaNivel);
        PlayerPrefs.SetInt("FueAlGulag", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("IrAlGulagSceme"); // Cargar la escena previa al Gulag
    }

    private IEnumerator MostrarAnimacionMuerteYFinJuego()
    {
        mostrandoAnimacionMuerte = true;
        animacion.Play("jugadorMuerte");

        yield return new WaitForSeconds(1f); // Duración de la animación

        FinJuego();
    }

    private void HacerVulnerable()
    {
        vulnerable = true;
        sprite.color = Color.white;
    }

    public void GanarGulag()
    {
        // Esta función se llama desde gulagController cuando el jugador gana

        // Configurar que el jugador debe volver al nivel con 1 vida
        PlayerPrefs.SetInt("VueltaDeGulag", 1);
        PlayerPrefs.Save();

        // Cargar la escena intermedia con el mensaje de victoria
        SceneManager.LoadScene("VolverAlNivelScene");
    }

    public void PerderGulag()
    {
        // Si pierde el Gulag, termina el juego
        FinJuego();
    }

    public void FinJuego()
    {
        // Cargar la escena de fin de juego
        SceneManager.LoadScene("FinJuegoScene");
    }
}
