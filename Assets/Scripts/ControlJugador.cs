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
    public int tieneLlave2 = 0;

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

    public AudioClip dispararFX;

    private AudioSource audioSource;

    public PlayerStats playerStats;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    { 

        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats no encontrado en el jugador.");
        }


        hud = canvas.GetComponent<controlHUD>();
        tiempoInicio = Time.time;
        vulnerable = true;
        fisica = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animacion = GetComponent<Animator>();
        hud.setVidasTxt(numVidas); // Actualizamos el HUD con las vidas iniciales


        // Solo cargamos PlayerPrefs si no hemos establecido ya el valor
        if (!fueAlGulag)
        {
            fueAlGulag = PlayerPrefs.GetInt("FueAlGulagGuardado", 0) == 1;
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
        //Salto
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

        // Aqui controlamos que si el tiempo llega a 0, se acaba la partida
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
            audioSource.PlayOneShot(dispararFX);
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

    // Con este metodo se contorla que el jugador no pueda saltar cuando está en el aire
    private bool TocandoSuelo()
    {
        RaycastHit2D toca = Physics2D.Raycast(transform.position + new Vector3(0, -2f, 0), Vector2.down, 0);
        return toca.collider != null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("llave1NV1"))
        {
            llavesRecolectadas++;
            Destroy(other.gameObject);

            if (llavesRecolectadas == 5)
            {
                SceneManager.LoadScene("Interlude1");
            }
        }
        else if (other.CompareTag("llave1NV2"))
        {
            llavesRecolectadas++;
            Destroy(other.gameObject);

            if (llavesRecolectadas == 5)
            {
                SceneManager.LoadScene("Interlude2");
            }
        }
        else if (other.CompareTag("llave2NV1"))
        {
            tieneLlave2++;
            Destroy(other.gameObject);
            if (tieneLlave2 == 1) {
                SceneManager.LoadScene("InterludeNv2");
            }

        } else if (other.CompareTag("llave2NV2")) {
            tieneLlave2++;
            Destroy(other.gameObject);
            if (tieneLlave2 == 1)
            {
                SceneManager.LoadScene("VictoriaScene");
                GanarPartida();
            }

        }
        if (other.CompareTag("pinchos"))
        {
            QuitarVida(1);
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
                    // Primera muerte: Ir al Gulag y guardar el flag
                    fueAlGulag = true;
                    PlayerPrefs.SetInt("FueAlGulagGuardado", 1);
                    PlayerPrefs.Save();
                    StartCoroutine(MostrarAnimacionMuerteYGulag());
                }
                else
                {
                    // Segunda muerte: Fin del juego
                    StartCoroutine(MostrarAnimacionMuerteYFinJuego());
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

        SceneManager.LoadScene("IrAlGulagSceme"); // Cargar la escena previa al Gulag
    }

    private IEnumerator MostrarAnimacionMuerteYFinJuego()
    {
        mostrandoAnimacionMuerte = true;
        animacion.Play("jugadorMuerte");
        PlayerPrefs.DeleteKey("FueAlGulagGuardado"); // Limpiar el flag al perder

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
        // Cargar la escena intermedia con el mensaje de victoria
        SceneManager.LoadScene("VolverAlNivelScene");
    }

    public void PerderGulag()
    {
        FinJuego();
    }
    private void GanarPartida()// Este metodo hay que ir mejorandolo cuando hagamos el nivel 2 y va con la logica de las llaves que ya mejoraremos mas adelante
    {
        playerStats.PartidaGanada();
        SceneManager.LoadScene("VictoriaScene");
    }
    public void FinJuego()
    {
        playerStats.PartidaPerdida();
        // Cargar la escena de fin de juego
        SceneManager.LoadScene("FinJuegoScene");
    }

    public void RegistrarEnemigoEliminado() { 
        playerStats.EnemigoEliminado();
    }
}
